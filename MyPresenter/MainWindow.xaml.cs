using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace MyPresenter
{
    public enum Loop_Types { ANNOUNCEMENTS, BACKGROUND }

    public class LineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string total = "";

            foreach (string val in values)
                total += "\r\n" + val;

            return total.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
        }

        public static class SleepUtil
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private BackgroundWorker _notesWorker = new BackgroundWorker();
        private BackgroundWorker _pcoWorker = new BackgroundWorker();

        private RemoteControl _remoteControl;
        private LiveOutputWindow _liveOutputWindow;
        private MediaElement _liveOutputMediaPlayer;
        private MediaPlayer _audioPlayer;
        private bool _useDarkText;
        private bool _muteAudio;
        private FileSystemWatcher _watcher;
        private ObservableCollection<ServiceItem> _serviceItemList;
        private string pco_last_update;

        private static MediaElement _prevVideo = new MediaElement();
        private static MediaElement _currVideo = new MediaElement();
        private static MediaElement _NextVideo = new MediaElement();

        private bool LiveActive { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //scriptTextBlock.DataContext = this;

            _serviceItemList = new ObservableCollection<ServiceItem>();

            _remoteControl = new RemoteControl();
            _remoteControl.RemoteControlEvent += _remoteControl_OnRemoteControl;
            _remoteControl.StartServer();

            _notesWorker.WorkerSupportsCancellation = true;
            _notesWorker.DoWork += notesWorker_DoWork;
            _notesWorker.RunWorkerCompleted += notesWorker_RunWorkerCompleted;

            _pcoWorker.WorkerSupportsCancellation = true;
            _pcoWorker.DoWork += pcoWorker_DoWork;
            _pcoWorker.RunWorkerCompleted += pcoWorker_RunWorkerCompleted;

            // diable screen saver while running
            SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            // display remote control address
            Label1.Content = _remoteControl.status;
        }

        void _remoteControl_OnRemoteControl(object sender, RemoteControl.RemoteControlArgs e)
        {
            if (e.PageParams.Count == 0) return;

            // action from remote page
            foreach (var item in e.PageParams.ToList())
            {
                if (item.Value == "play")
                    playVideo();    // calls generateHtml()
                else if (item.Value == "stop")
                    stopLive();     // calls generateHtml()
                else if (item.Value == "next")
                    ScriptListView.SelectedIndex = ScriptListView.SelectedIndex + 1; // does NOT call generateHtml(); saves current slide to App.Current.Properties["card"]
                else if (item.Value == "prev")
                    ScriptListView.SelectedIndex = ScriptListView.SelectedIndex - 1; // does NOT call generateHtml(); saves current slide to App.Current.Properties["card"]
                else
                {
                    foreach (ServiceItem si in _serviceItemList)
                        if (si.Title == WebUtility.UrlDecode(item.Value.ToString()))
                        {
                            serviceListBox.SelectedItem = si; // calls generateHtml()

                            break;
                        }
                } 
            }
        }
        
        private bool UseDarkText
        {
            get { return _useDarkText; }
            set
            {
                _useDarkText = value;

                RaisePropertyChanged("UseDarkText");
            }
        }

        private bool MuteAudio
        {
            get { return _muteAudio; }
            set
            {
                _liveOutputMediaPlayer.IsMuted = value;
                _audioPlayer.IsMuted = value;

                _muteAudio = value;

                if (_muteAudio)
                    muteImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/MyPresenter;component/AudioMuteOn.png", UriKind.Absolute));
                else
                    muteImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/MyPresenter;component/AudioMuteOff.png", UriKind.Absolute));
            }
        }

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set UI
            loadBackgroundLoops();
            loadBackgroundAudio();
            loadBumperVideos();
            loadBackgroundImages();
            loadLiveOutputWindow(Properties.Resources.PlaceholderImagePath);

            // set up directory watchers
            _watcher = new FileSystemWatcher(Properties.Resources.LibraryPath, "*.*");
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.Renamed += new RenamedEventHandler(FileSystemRenamed);
            _watcher.Created += new FileSystemEventHandler(FileSystemChanged);
            _watcher.Deleted += new FileSystemEventHandler(FileSystemChanged);

            // disable play/stop buttons until service list is downloaded
            playVideoButton.IsEnabled = false;
            stopVideoButton.IsEnabled = false;

            // set service data context
            serviceListBox.ItemsSource = _serviceItemList;
            serviceListBox.SelectedIndex = -1;

            // load notes
            notesTextBox.Document.Blocks.Clear();
            notesTextBox.Document.Blocks.Add(new Paragraph(new Run(getNotes())));

            // set window caption
#if DEBUG
            this.Title = "MyPresenter - DEBUG";
#else
           this.Title = "MyPresenter - " + DateTime.Now.ToString("MMM d, yyyy");
#endif

            //WebBrowser1.Source = new Uri("https://play.spotify.com/user/spotify/playlist/6Higf6awk4pfVmjvlaCn7b?play=true&utm_source=open.spotify.com&utm_medium=open"); //Properties.Resources.SpotifyPath);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // get songs from PCO
            _pcoWorker.RunWorkerAsync();

            // load notes
            //_notesWorker.RunWorkerAsync();

            // set up high res timer
            DispatcherTimer oneSecondTimer = new System.Windows.Threading.DispatcherTimer();
            oneSecondTimer.Tick += oneSecondTimer_Tick;
            oneSecondTimer.Interval = new TimeSpan(0, 0, 1);
            oneSecondTimer.Start();
        }

        // Define the event handlers. 
        private void FileSystemChanged(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.VideoPath) loadBumperVideos();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.PadPath) loadBackgroundAudio();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.LoopPath) loadBackgroundLoops();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.ImagePath) loadBackgroundImages();
        }

        private void FileSystemRenamed(object source, RenamedEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.VideoPath) loadBumperVideos();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.PadPath) loadBackgroundAudio();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.LoopPath) loadBackgroundLoops();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.ImagePath) loadBackgroundImages();
        }

        private void loadLiveOutputWindow(string path)
        {
            _liveOutputWindow = new LiveOutputWindow();

            //second monitor full screen   
            try
            {
                int screens = System.Windows.Forms.Screen.AllScreens.Count<Screen>();

                var area = System.Windows.Forms.Screen.AllScreens[screens - 1].WorkingArea;

                _liveOutputWindow.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                _liveOutputWindow.Left = area.Left;
                _liveOutputWindow.Top = area.Top;
                _liveOutputWindow.Width = 320;
                _liveOutputWindow.Height = 240;

                if (screens > 1)
                {
                    _liveOutputWindow.WindowStyle = System.Windows.WindowStyle.None;
                    _liveOutputWindow.Loaded += (sender, e) => { _liveOutputWindow.WindowState = WindowState.Maximized; };
                }

                // get mediaplayer
                _liveOutputMediaPlayer = (MediaElement)_liveOutputWindow.FindName("VideoPlayer");
                _liveOutputMediaPlayer.MediaOpened += _liveOutputMediaPlayer_MediaOpened;

                // load static image
                setVideo(new Uri(path, UriKind.Absolute));

                // set preview player fill
                VideoDisplay.Fill = new VisualBrush(_liveOutputMediaPlayer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
        }

        void _liveOutputMediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void loadServiceList()
        {
            //DateTime _pco_last_update;

            //DateTime.TryParse(pco_last_update, out _pco_last_update);

            try
            {
                var rand = new Random();
                var files = Directory.GetFiles(Properties.Resources.LoopPath, "background*.mp*"); // filter for auto-selecting bg loops
                double _index = 0.0;
                double _count = 0.0;

                this.Dispatcher.Invoke((Action)delegate()
                {
                    _serviceItemList.Clear();

                    // alert user to PCO status
                    _serviceItemList.Add(new ServiceItem(true, "DOWNLOADING SONGS...PLEASE WAIT"));
                });

                pcoItems.Items _pcoItems = PCO.getItems();
                
                // set initial update for error tracking
                pco_last_update = PCO.pcoLastUpdate();

                for (int i = 0; i < _pcoItems.data.Count; i++)
                    if (_pcoItems.data[i].attributes.item_type == "song") _count++;

                for (int i = 0; i < _pcoItems.data.Count; i++)
                {
                    if (_pcoItems.data[i].attributes.item_type == "song" && _pcoItems.data[i].attributes.service_position == "during")
                    {
                        _serviceItemList[0].Progress = (int)((++_index / _count) * 100);

                        if (!File.Exists(Properties.Resources.SongPath + _pcoItems.data[i].attributes.title + ".xml"))
                        {
                            //System.Windows.MessageBox.Show("Lyrics for '" + items.data[i].attributes.title + "' are missing.", "Song Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                            this.Dispatcher.Invoke((Action)delegate()
                            {
                                _serviceItemList.Add(new ServiceItem(true, "MISSING: " + _pcoItems.data[i].attributes.title));
                            });

                            // clear timestamp to indicate error
                            pco_last_update = "";

                            continue;
                        }

                        New.song _song = Serializer.DeserializeFromXML<New.song>(Properties.Resources.SongPath + _pcoItems.data[i].attributes.title + ".xml");
                        string _loop = _song.loop;

                        // select random loop if none specified in XML file
                        if (_loop == "") _loop = files[rand.Next(files.Length)];

                        if (_song != null)
                        {
                            this.Dispatcher.BeginInvoke((Action)delegate()
                            {
                                if (_song.key == "")
                                    _serviceItemList.Add(new ServiceItem(true, "MISSING PAD: " + _song.title));
                                else
                                    _serviceItemList.Add(new ServiceItem() { Title = _song.title, Pad = _song.key, Loop = _loop });
                            });

                            // update if not in error condition
                            if (pco_last_update != "") pco_last_update = PCO.pcoLastUpdate();
                        }
                        else // error loading file
                        {
                            this.Dispatcher.BeginInvoke((Action)delegate()
                            {
                                _serviceItemList.Add(new ServiceItem(true, "FILE ERROR: " + _pcoItems.data[i].attributes.title));
                            });
                        }

                        // ui hack to show items as each is loaded
                        // Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                    }
                }

                // remove "downloading"
                this.Dispatcher.Invoke((Action)delegate()
                {
                    _serviceItemList.RemoveAt(0);
                });

                generateHtml();                                    
            }
            catch (Exception ex)
            {
                if (!File.Exists(Properties.Resources.SettingsFilePath)) return;
                if (new FileInfo(Properties.Resources.SettingsFilePath).Length == 0) return;

                _serviceItemList = Serializer.DeserializeFromXML<ServiceItem>();

                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    serviceListBox.SelectedIndex = -1;
                    
                    // alert user to PCO status
                    //_serviceItemList.Insert(0, new ServiceItem(true, "Error importing from PCO - OFFLINE"));
                });

                //System.Windows.MessageBox.Show("Error importing from PCO.", "PCO Import Error - OFFLINE", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
        }

        private void generateHtml()
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    ServiceItem selectedItem = (ServiceItem)serviceListBox.SelectedItem;
                    string indexContent = "";

                    // get index page content
                    using (TextReader tr = new StreamReader(Properties.Resources.RemoteControlIndexTemplate))
                    {
                        indexContent = tr.ReadToEnd();
                    }

                    int i = 1;
                    string _content = "";
                    const string _divContent = "<div class=\"collapse.in text-center\" id=\"[[--CARDID--]]\"><div class=\"card card-block\"><div class=\"card-text\">[[--CARD--]]</div></div></div>";

                    indexContent = indexContent.Replace("[[--DATE--]]", DateTime.Now.ToString("HH:mm:ss"));
                    indexContent = indexContent.Replace("[[--REMOTE--]]", _remoteControl.LocalAddress);

                    if (LiveActive)
                    {
                        indexContent = indexContent.Replace("[[--HIDDEN--]]", "");
                        indexContent = indexContent.Replace("[[--LIVEACTIVE--]]", "active");
                        indexContent = indexContent.Replace("[[--PLAYDISABLED--]]", "disabled");
                        indexContent = indexContent.Replace("[[--STOPDISABLED--]]", "");
                        indexContent = indexContent.Replace("[[--NEXTDISABLED--]]", "");
                        indexContent = indexContent.Replace("[[--PREVDISABLED--]]", "");
                    }
                    else
                    {
                        indexContent = indexContent.Replace("[[--HIDDEN--]]", "hidden");
                        indexContent = indexContent.Replace("[[--LIVEACTIVE--]]", "");
                        indexContent = indexContent.Replace("[[--PLAYDISABLED--]]", "");
                        indexContent = indexContent.Replace("[[--STOPDISABLED--]]", "disabled");
                        indexContent = indexContent.Replace("[[--NEXTDISABLED--]]", "disabled");
                        indexContent = indexContent.Replace("[[--PREVDISABLED--]]", "disabled");
                    }
                        
                    foreach (ServiceItem si in _serviceItemList)
                    {
                        if (selectedItem == null)
                            _content += "<button name=\"button_" + i++.ToString() + "\" value=\"" + si.Title + "\" class=\"btn btn-lg btn-primary btn-block post\">" + si.Title + "</button>";
                        else if (selectedItem.Title == si.Title)
                        {
                            string _temp = "<button name=\"button_" + i++.ToString() + "\" value=\"" + si.Title + "\" class=\"btn btn-lg btn-primary btn-success btn-block post active\" data-toggle=\"collapse\" data-target=\"#collapse_" + i.ToString() + "\" >" + si.Title + "</button>";

                            if (LiveActive)
                            {
                                _temp += _divContent.Replace("[[--CARDID--]]", "collapse_" + i.ToString()).Replace("[[--CARD--]]", scriptTextBlock.Text.Replace("\r\n", "<br />"));

                                _content = _temp + _content;
                            }
                            else
                                _content += _temp;
                        }
                        else // not null and not selected
                            _content += "<button name=\"button_" + i++.ToString() + "\" value=\"" + si.Title + "\" class=\"btn btn-lg btn-primary btn-block post\">" + si.Title + "</button>";
                    }

                    indexContent = indexContent.Replace("[[--CONTENT--]]", _content);

                    App.Current.Properties["html"] = indexContent;

                    // write new index content
                    //using (TextWriter tw = new StreamWriter(Properties.Resources.RemoteControlIndexPage, false))
                    //{
                    //    tw.Write(indexContent);
                    //}
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
        }

        private void gotoBreak(Uri ImageHolder)
        {
            //if (_liveOutputWindow != null) VideoPlayer.Play();
        }

        private void oneSecondTimer_Tick(object sender, EventArgs e)
        {
            DateTime startTime;
            TimeSpan ts;

            // display numlock state
            if (System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.NumLock))
                numLockLabel.Visibility = System.Windows.Visibility.Visible;
            else
                numLockLabel.Visibility = System.Windows.Visibility.Hidden;

            // set current time display
            currentTimeLabel.Content = string.Format("{0:HH:mm}", DateTime.Now);

            // set media time display
            try
            {
                if (_liveOutputMediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    ts = new TimeSpan(_liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks - _liveOutputMediaPlayer.Position.Ticks);

                    // elapsed / total
                    mediaTimeLabel.Content = string.Format("{0:%m}m {0:%s}s", ts) + "/";
                    mediaTimeLabel.Content += string.Format("{0:%m}m {0:%s}s", new TimeSpan(_liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }

            // cancel if not auto-start
            if (startTimeTextBox.Text == "") return;

            // set countdown display
            if (DateTime.TryParseExact(startTimeTextBox.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out startTime))
            {
                ts = new TimeSpan(startTime.Ticks - DateTime.Now.Ticks);

                countdownTimeLabel.Content = string.Format("{0:%h}h {0:%m}m {0:%s}s", ts);
            }
            else
                countdownTimeLabel.Content = "";

            // AUTO START
            try
            {
                if (DateTime.Now.CompareTo(startTime.AddMinutes(-5)) >= 0)
                {
                    string _startTime = startTimeTextBox.Text;

                    startTimeTextBox.Text = "";

                    // set random bumper/countdown if none selected
                    if (BumperVideoComboBox.SelectedIndex < 1)
                    {
                        // get count of bumper videos to set index of countdown (since they are alphabetical)
                        DirectoryInfo di = new DirectoryInfo(Properties.Resources.VideoPath);
                        FileInfo[] directories = di.GetFiles("bumper_*.*");

                        BumperVideoComboBox.SelectedIndex = new Random().Next(directories.GetLength(0), BumperVideoComboBox.Items.Count - 1);
                    }

                    loadLiveOutputWindow(Properties.Resources.VideoPath + BumperVideoComboBox.SelectedItem.ToString());

                    _liveOutputMediaPlayer.MediaOpened += (se, ea) => { goLive(startTime); };
                }
            }
            catch (Exception ex)
            { Debug.WriteLine(ex.Message); }
        }

        private void goLive(DateTime startTime)
        {
            //long video = _liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks;
            long offset = startTime.Ticks - DateTime.Now.Ticks;

            // if offset is negative, start time has passed; this call did not come from auto-start (oneSecondTimer_Tick)
            if (offset < 0) return;

            if (_liveOutputMediaPlayer.NaturalDuration.HasTimeSpan)
            {
                // most likely spotify is running, so mute initial output
                MuteAudio = true;

                // play entire clip if media length is less than offset
                if (offset > _liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks)
                {
                    // sleep until offset
                    Thread.Sleep((int)(_liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks / TimeSpan.TicksPerMillisecond));

                    // allow entire clip to play
                    offset = 0;
                }

                _liveOutputMediaPlayer.Clock.Controller.Seek(new TimeSpan(_liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks - offset), TimeSeekOrigin.BeginTime);
                _liveOutputMediaPlayer.Clock.Controller.Resume();
            }

            if (!_liveOutputWindow.IsVisible) _liveOutputWindow.Show();

            // TODO: combine Play/Pause button (possibly do away with Pause)
            playVideoButton.IsEnabled = false;
        }

        private void stopLive()
        {
            _liveOutputMediaPlayer.Clock = null;

            _audioPlayer.Stop();

            _liveOutputWindow.Close();

            VideoDisplay.Fill = null;

            playVideoButton.IsEnabled = true;

            LiveActive = false;

            BumperVideoComboBox.SelectedIndex = 0;
            BackgroundAudioComboBox.SelectedIndex = 0;
            BackgroundVideoComboBox.SelectedIndex = 0;
            backgroundImageComboBox.SelectedIndex = 0;

            serviceListBox.SelectedIndex = -1;

            generateHtml();
        }

        private void playVideoButton_Click(object sender, RoutedEventArgs e)
        {
            playVideo();
        }

        private void playVideo()
        {
            serviceListBox.SelectedIndex = -1;

            if (PresentationSource.FromVisual(_liveOutputWindow) == null)
            {
                if (BackgroundVideoComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.LoopPath + BackgroundVideoComboBox.SelectedItem.ToString());
                else if (BumperVideoComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.VideoPath + BumperVideoComboBox.SelectedItem.ToString());
                else if (backgroundImageComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.ImagePath + backgroundImageComboBox.SelectedItem.ToString());
                else
                {
                    if ((bool)useBlankImageCheckBox.IsChecked)
                        loadLiveOutputWindow(Properties.Resources.PlaceholderImagePath.Replace("LivingRoomTitle.jpg", "_blank.jpg"));
                    else
                        loadLiveOutputWindow(Properties.Resources.PlaceholderImagePath);
                }
            }

            _pcoWorker.CancelAsync();

            _liveOutputMediaPlayer.IsMuted = MuteAudio;
            _liveOutputMediaPlayer.Clock.Controller.Resume();

            playVideoButton.IsEnabled = false;

            _liveOutputWindow.Show();

            LiveActive = true;
            
            generateHtml();
        }

        private void stopVideoButton_Click(object sender, RoutedEventArgs e)
        {
            stopLive();
        }

        private void loadBumperVideos()
        {
            if (this.Dispatcher.CheckAccess())
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Resources.VideoPath);
                var directories = di.GetFiles("*.*");

                BumperVideoComboBox.Items.Clear();
                BumperVideoComboBox.Items.Add("Select Bumper/Countdown...");

                foreach (FileInfo d in directories)
                    BumperVideoComboBox.Items.Add(d.Name);
            }
            else
                this.Dispatcher.BeginInvoke(new Action(loadBumperVideos));
        }

        private void loadBackgroundLoops()
        {
            if (this.Dispatcher.CheckAccess())
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Resources.LoopPath);
                var directories = di.GetFiles("welcome*.mpg");

                BackgroundVideoComboBox.Items.Clear();
                BackgroundVideoComboBox.Items.Add("Select Loop...");

                foreach (FileInfo d in directories)
                    BackgroundVideoComboBox.Items.Add(d.Name);
            }
            else
                this.Dispatcher.BeginInvoke(new Action(loadBackgroundLoops));
        }

        private void loadBackgroundAudio()
        {
            if (this.Dispatcher.CheckAccess())
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Resources.MusicPath);
                var directories = di.GetFiles("*.mp3");

                BackgroundAudioComboBox.Items.Clear();
                BackgroundAudioComboBox.Items.Add("Select Audio...");

                foreach (FileInfo d in directories)
                    BackgroundAudioComboBox.Items.Add(d.Name);
            }
            else
                this.Dispatcher.BeginInvoke(new Action(loadBackgroundAudio));
        }

        private void loadBackgroundImages()
        {
            if (this.Dispatcher.CheckAccess())
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Resources.ImagePath);
                var directories = di.GetFiles("*.*");

                backgroundImageComboBox.Items.Clear();
                backgroundImageComboBox.Items.Add("Select Image...");

                foreach (FileInfo d in directories)
                    backgroundImageComboBox.Items.Add(d.Name);
            }
            else
                this.Dispatcher.BeginInvoke(new Action(loadBackgroundImages));
        }

        private void setSong(string songName)
        {
            if (!File.Exists(Properties.Resources.SongPath + songName + ".xml")) return;

            New.song _currentSong = Serializer.DeserializeFromXML<New.song>(Properties.Resources.SongPath + "\\" + songName + ".xml");

            ScriptListView.Items.Clear();

            foreach (New.songVerse v in _currentSong.lyrics)
            {
                Bitmap flag = new Bitmap(320, 240);
                Graphics flagGraphics = Graphics.FromImage(flag);

                //flagGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                //flagGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //flagGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                flagGraphics.DrawString(v.name, new Font("Century Gothic", 10), System.Drawing.Brushes.DarkGray, new PointF(0, 0));

                MemoryStream ms = new MemoryStream();
                flag.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                System.Windows.Controls.ListViewItem lvi = new System.Windows.Controls.ListViewItem();
                lvi.Content = v.lyric.Replace("_", "\n");
                lvi.Background = new ImageBrush(bi);
                lvi.Name = v.name;

                if ((string)lvi.Content == "") lvi.Content = "[ BLANK ]";

                ScriptListView.Items.Add(lvi);
            }
        }

        private void BumperVideoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BumperVideoComboBox.SelectedIndex < 1) return;

            BackgroundVideoComboBox.SelectedIndex = 0;
            backgroundImageComboBox.SelectedIndex = 0;

            _audioPlayer.Close();

            setVideo(new Uri(Properties.Resources.VideoPath + BumperVideoComboBox.SelectedItem.ToString()));

            if (PresentationSource.FromVisual(_liveOutputWindow) != null)
                _liveOutputMediaPlayer.Clock.Controller.Resume();
            else
                _liveOutputMediaPlayer.Clock.Controller.Pause();
        }

        private void BackgroundVideoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundVideoComboBox.SelectedIndex < 1) return;
            backgroundImageComboBox.SelectedIndex = 0;
            BumperVideoComboBox.SelectedIndex = 0;

            _audioPlayer.Close();

            setVideo(new Uri(Properties.Resources.LoopPath + BackgroundVideoComboBox.SelectedItem.ToString(), UriKind.Absolute));

            if (PresentationSource.FromVisual(_liveOutputWindow) != null) _liveOutputMediaPlayer.Clock.Controller.Resume();
        }

        private void BackgroundAudioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_audioPlayer != null)
                _audioPlayer.Close();
            else
                _audioPlayer = new MediaPlayer();

            if (BackgroundAudioComboBox.SelectedIndex > 0)
            {
                _audioPlayer.Open(new Uri(Properties.Resources.PadPath + BackgroundAudioComboBox.SelectedValue.ToString()));
                _audioPlayer.Play();
            }
        }

        private void backgroundImageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (backgroundImageComboBox.SelectedIndex < 1) return;
            BackgroundVideoComboBox.SelectedIndex = 0;
            BumperVideoComboBox.SelectedIndex = 0;

            //if ((bool)SlideShowCheckBox.IsChecked)
            //{
            //    System.Threading.Timer t = new System.Threading.Timer(state => KA(stuff, state));

            setVideo(new Uri(Properties.Resources.ImagePath + backgroundImageComboBox.SelectedValue.ToString()));

            if (PresentationSource.FromVisual(_liveOutputWindow) != null) _liveOutputMediaPlayer.Clock.Controller.Resume();
        }

        private void ScriptListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListViewItem lvi;

            scriptTextBlock.Text = "";

            // do not continue if nothing is selected
            if (ScriptListView.SelectedIndex < 0) return;

            // get selected item
            lvi = (System.Windows.Controls.ListViewItem)ScriptListView.SelectedItem;

            if ((string)lvi.Content == "[ BLANK ]")
            {
                // show text in preview window
                scriptTextBlock.Text = "[ BLANK ]";

                // display text in output window
                _liveOutputWindow.DisplayText("");
            }
            else
            {
                // show text in preview window
                scriptTextBlock.Text = (string)lvi.Content;

                // display text in output window
                _liveOutputWindow.DisplayText((string)lvi.Content);
            }

            //ScriptListView.SelectedItem = lvi;

            App.Current.Properties["card"] = scriptTextBlock.Text.Replace("\r\n", "<br />");

            double _index = ScriptListView.SelectedIndex;
            double _count = ScriptListView.Items.Count - 1;

            _serviceItemList[serviceListBox.SelectedIndex].Progress = (int)((_index / _count) * 100);
        }

        private void serviceListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            object item = serviceListBox.SelectedItem;
            int index = serviceListBox.SelectedIndex;

            // ----  SAVE FOR POSSIBLE FUTURE USE ---- //
            // --------------------------------------- //
            //if (Keyboard.IsKeyDown(Key.F5))
            //{
            //    int breakCount = 1;

            //    for (int i = 0; i < serviceListBox.Items.Count; i++)
            //        if (serviceListBox.Items[i].ToString().Contains("Break")) breakCount++;

            //    serviceListBox.Items.Insert(index + 1, "Break " + breakCount.ToString() + " - 10:00");
            //}
            // --------------------------------------- //

            if (e.Key == Key.OemMinus)
                _serviceItemList.Remove((ServiceItem)serviceListBox.SelectedItem);
        }

        private void serviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            New.song _currentSong = new New.song();

            if (serviceListBox.SelectedIndex < 0) ScriptListView.Items.Clear();
            if (serviceListBox.SelectedIndex < 0) return;

            _liveOutputWindow.DisplayText("");

            // get song selection
            ServiceItem _song = (ServiceItem)serviceListBox.SelectedItem;

            try
            {
                // create song object
                _currentSong = Serializer.DeserializeFromXML<New.song>(Properties.Resources.SongPath + _song.Title + ".xml");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading '" + _song.Title + "'.", "XML Error");

                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }

            if (PresentationSource.FromVisual(_liveOutputWindow) != null)
            {
                var rand = new Random();
                var files = Directory.GetFiles(Properties.Resources.LoopPath, "background*.mp*"); // filter for auto-selecting bg loops
                string _loop = _currentSong.loop;

                // select random loop if none specified in XML file
                if (_loop == "") _loop = System.IO.Path.GetFileName(files[rand.Next(files.Length)]);

                setLoop(_loop); //getLoopSelection(serviceListBox.SelectedItem));
                startPad(_currentSong.key); //getPadSelection(serviceListBox.SelectedItem));

                MuteAudio = false;
            }

            setSong(_song.Title);

            double _count = ScriptListView.Items.Count;
            double _progress = (((ServiceItem)_serviceItemList[serviceListBox.SelectedIndex]).Progress / 100.0);
            double _index = _count * _progress;

            ScriptListView.SelectedIndex = (int)_index;
            ScriptListView.Focus();
            ScriptListView.ScrollIntoView(ScriptListView.SelectedItem);

            if (ScriptListView.SelectedItem != null)
                ((ListBoxItem)ScriptListView.SelectedItem).Focus();

            //if (serviceListBox.SelectedItem.ToString().Contains("Break"))
            //    gotoBreak(new Uri(Properties.Resources.ImagePath + "\\LivingRoomLive.jpg", UriKind.Absolute));

            // update HTML for remote control
            generateHtml();

            //killSpotify();
        }

        private void killSpotify()
        {
            try
            {
                //foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                //{
                //    if (p.ProcessName == "Spotify")
                //    {
                //        p.Kill();
                //    }
                //} 
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
        }

        public string getPadSelection(object PadListBoxItem)
        {
            string _pad = "";

            var _container = serviceListBox.ItemContainerGenerator.ContainerFromItem(serviceListBox.SelectedItem);

            if (_container != null)
            {
                var _children = AllChildren(_container);
                var _control = (System.Windows.Controls.ComboBox)_children.First(c => c.Name == "padComboBox");

                System.Windows.Controls.ComboBox padCombo = (System.Windows.Controls.ComboBox)_control;

                if (padCombo.SelectedIndex > -1) _pad = padCombo.SelectedItem.ToString();
            }

            return _pad;
        }

        public string getLoopSelection(object LoopListBoxItem)
        {
            string _loop = "";

            var _container = serviceListBox.ItemContainerGenerator.ContainerFromItem(serviceListBox.SelectedItem);

            if (_container != null)
            {
                var _children = AllChildren(_container);
                var _control = (System.Windows.Controls.ComboBox)_children.First(c => c.Name == "loopComboBox");

                System.Windows.Controls.ComboBox loopCombo = (System.Windows.Controls.ComboBox)_control;

                if (loopCombo.SelectedIndex > -1) _loop = loopCombo.SelectedItem.ToString();
            }

            return _loop;
        }

        public List<System.Windows.Controls.Control> AllChildren(DependencyObject parent)
        {
            var _list = new List<System.Windows.Controls.Control> { };

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _child = VisualTreeHelper.GetChild(parent, i);

                if (_child is System.Windows.Controls.Control)
                    _list.Add(_child as System.Windows.Controls.Control);

                _list.AddRange(AllChildren(_child));
            }

            return _list;
        }

        public bool FindListViewItem(DependencyObject obj)
        {
            bool _found = false;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                System.Windows.Controls.ListViewItem lv = obj as System.Windows.Controls.ListViewItem;

                if (lv != null)
                    _found = true;
                else
                    FindListViewItem(VisualTreeHelper.GetChild(obj as DependencyObject, i));
            }

            return _found;
        }

        private void clearTextButton_Click(object sender, RoutedEventArgs e)
        {
            scriptTextBlock.Text = "";
            displayTextBox.Text = "";

            _liveOutputWindow.DisplayText("");
        }

        private void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            scriptTextBlock.Text = displayTextBox.Text;

            _liveOutputWindow.DisplayText(displayTextBox.Text);
        }

        private void ScriptListView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up) return;

            // move to previous song, if not first song
            if (ScriptListView.SelectedIndex == 0 && serviceListBox.SelectedIndex > 0)
            {
                if (e.Key == Key.Up)
                {
                    serviceListBox.SelectedIndex -= 1;

                    ScriptListView.SelectedIndex = ScriptListView.Items.Count - 1;
                    ScriptListView.Focus();
                    ScriptListView.ScrollIntoView(ScriptListView.SelectedItem);

                    ((ListBoxItem)ScriptListView.SelectedItem).Focus();
                }
            }

            // move to next song, if not last song
            if (ScriptListView.SelectedIndex == ScriptListView.Items.Count - 1 && serviceListBox.SelectedIndex < serviceListBox.Items.Count - 1)
            {
                if (e.Key == Key.Down)
                {
                    serviceListBox.SelectedIndex += 1;

                    ScriptListView.SelectedIndex = 0;
                    ScriptListView.Focus();
                    ScriptListView.ScrollIntoView(ScriptListView.SelectedItem);

                    ((ListBoxItem)ScriptListView.SelectedItem).Focus();
                }
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) stopLive();
        }

        private void darkTextColorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.UseDarkText = true;

            _liveOutputWindow.UseDarkText = true;
        }

        private void darkTextColorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.UseDarkText = false;

            _liveOutputWindow.UseDarkText = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // save service list for offline backup
            Serializer.SerializeToXML<ServiceItem>(_serviceItemList);

            // save notes
            saveNotes();

            //foreach (ServiceItem serviceItem in serviceListBox.Items)
            //{
            //    song _song = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + serviceItem.Title + ".xml");

            //    _song.properties.key = serviceItem.Pad;
            //    _song.properties.loop = serviceItem.Loop;

            //    Serializer.SerializeToXML<song>(_song, Properties.Resources.SongPath + serviceItem.Title + ".xml");
            //}

            killSpotify();

            // re-activate screen saver
            SleepUtil.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

            _remoteControl.StopServer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _audioPlayer.Stop();

            _liveOutputMediaPlayer.Clock = null; //.Controller.Stop();

            _liveOutputWindow.Close();
            _liveOutputWindow = null;
        }

        private void startPad(string PadToPlay)
        {
            if (_audioPlayer != null) _audioPlayer.Close();

            if (PadToPlay == "") return;

            if (!PadToPlay.Contains(".mp3"))
                PadToPlay += ".mp3";

            _audioPlayer = new MediaPlayer();
            _audioPlayer.Open(new Uri(Properties.Resources.PadPath + PadToPlay));
            _audioPlayer.Play();

            MuteAudio = false;
        }

        private void setLoop(string LoopToPlay)
        {
            //string _loopPath = "";

            //if (LoopToPlay == "") return;

            //// set announcement loop
            ////if (serviceListBox.SelectedIndex == 0) LoopToPlay = "announcements_" + LoopToPlay;

            //// set background loop
            //if (LoopToPlay.IndexOf("_") < 0) LoopToPlay = "background_" + LoopToPlay;

            ////DoubleAnimation __ani = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(3), FillBehavior.Stop);

            ////__ani.Completed += (o, e) =>
            ////{
            ////    //_liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, null);

            //_loopPath = Properties.Resources.LoopPath + LoopToPlay;

            setVideo();

            _liveOutputMediaPlayer.Clock.Controller.Resume();

            //if (LoopToPlay.Substring(0, 9) == "countdown") MuteAudio = true;

            //    // fade in
            //    _liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(1), FillBehavior.Stop));
            //};

            //// fade out
            //_liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, __ani);
        }

        private void setVideo()
        {
            try
            {
                _liveOutputMediaPlayer.Clock = ((ServiceItem)serviceListBox.SelectedItem).VideoPlayer.Clock;
            }
            catch (Exception ex)
            {
                setVideo(new Uri(Properties.Resources.LoopPath + "background_FractalFlood.mpg"));

                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
            finally
            { }
        }

        private void setVideo(Uri videoUri)
        {
            MediaTimeline mTimeLine = new MediaTimeline(videoUri);

            if (videoUri.ToString().Contains("Loop")) mTimeLine.RepeatBehavior = RepeatBehavior.Forever;

            MediaClock mClock = mTimeLine.CreateClock();

            _liveOutputMediaPlayer.Clock = mClock;
            //_liveOutputMediaPlayer.Clock.Controller.Begin();
            //_liveOutputMediaPlayer.Clock.Controller.Pause();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            testLabel.Content = e.ViewportHeight + " / " + e.VerticalChange.ToString() + " / " + e.VerticalOffset.ToString() + " / " + e.ExtentHeight.ToString();
        }

        private void serviceListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int index = serviceListBox.SelectedIndex;

            if (e.Key == Key.Up && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (index > 0)
                    _serviceItemList.Move(index, --index);
            }
            else if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (index < serviceListBox.Items.Count)
                    _serviceItemList.Move(index, ++index);
            }
        }

        private void serviceListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PresentationSource.FromVisual(_liveOutputWindow) == null) return;

            if (serviceListBox.SelectedIndex < 0) ScriptListView.Items.Clear();
            if (serviceListBox.SelectedIndex < 0) return;

            // get song title
            ServiceItem _song = (ServiceItem)serviceListBox.SelectedItem;

            // create song object
            New.song _currentSong = Serializer.DeserializeFromXML<New.song>(Properties.Resources.SongPath + _song.Title + ".xml");

            setSong(_song.Title);
            startPad(_currentSong.key); //getPadSelection(serviceListBox.SelectedItem));
            setLoop(_currentSong.loop); //getLoopSelection(serviceListBox.SelectedItem));
        }

        private void quickVerseTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (_liveOutputMediaPlayer.Clock.Timeline.Source.LocalPath != Properties.Resources.LoopPath + "background_SimpleStarFlightBlue.mpg")
                    {
                        setVideo(new Uri(Properties.Resources.LoopPath + "background_SimpleStarFlightBlue.mpg"));

                        if (PresentationSource.FromVisual(_liveOutputWindow) != null) _liveOutputMediaPlayer.Clock.Controller.Resume();
                    }

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.esvapi.org/v2/rest/passageQuery?key=IP&passage=" + System.Uri.EscapeDataString(quickVerseTextBox.Text) + "&output-format=plain-text&include-footnotes=false&include-passage-horizontal-lines=false&include-heading-horizontal-lines=false&include-headings=false");

                    //Set values for the request back
                    req.Method = "GET";

                    using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                    {
                        // Pipes the stream to a higher level stream reader with the required encoding format. 
                        using (StreamReader readStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                        {
                            string verse;
                            string text;

                            // plain text output is:
                            // Book chapter:verse          \r
                            // [verse] text

                            // get verse
                            verse = readStream.ReadLine();
                            text = readStream.ReadToEnd();

                            _liveOutputWindow.DisplayVerse(verse);
                            _liveOutputWindow.DisplayText(verse + " - " + text);
                        }
                    }

                    quickVerseTextBox.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
            finally { }

        }

        private void muteImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MuteAudio = !MuteAudio;
        }

        private void spotifyButton_Click(object sender, RoutedEventArgs e)
        {
            //if (_spotify == null)
            //{
            //    _spotify = Process.Start(@"C:\Users\Owner\AppData\Roaming\Spotify\Spotify.exe");

            //    spotifyButton.Content = "stop spotify";
            //}
            //else
            //    

            killSpotify();
        }

        private void ScriptListItemAdd_Click(object sender, RoutedEventArgs e)
        {
            // not implemented
        }

        private void ScriptListItemDelete_Click(object sender, RoutedEventArgs e)
        {
            // not implemented
        }

        private void ScriptListItemEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed) return;

            string _path = Properties.Resources.SongPath + "\\";

            if (serviceListBox.SelectedIndex == -1) return;

            _path += ((ServiceItem)serviceListBox.SelectedItem).Title + ".xml";

            EditSlide _editSlide = new EditSlide(_path, ((System.Windows.Controls.ListViewItem)ScriptListView.SelectedItem).Name);

            _editSlide.ShowDialog();

            setSong(((ServiceItem)serviceListBox.SelectedItem).Title);
        }

        private void ServiceListItemAdd_Click(object sender, RoutedEventArgs e)
        {
            // not implemented
        }

        private void ServiceListItemDelete_Click(object sender, RoutedEventArgs e)
        {
            // not implemented
        }

        private void ServiceListItemEdit_Click(object sender, RoutedEventArgs e)
        {
            string _path = Properties.Resources.SongPath;

            if (serviceListBox.SelectedIndex == -1) return;

            _path += ((ServiceItem)serviceListBox.SelectedItem).Title.Replace("MISSING PAD: ", "") + ".xml";

            NewSong _newSong = new NewSong(_path);

            _newSong.Show();

            setSong(((ServiceItem)serviceListBox.SelectedItem).Title);
        }

        private string getNotes()
        {
            string _notes = "";

            if (!File.Exists(Properties.Resources.NotesPath)) return _notes;

            using (TextReader reader = new StreamReader(Properties.Resources.NotesPath))
            {
                _notes = reader.ReadToEnd();
            }

            return _notes;
        }

        private void saveNotes()
        {
            using (TextWriter writer = new StreamWriter(Properties.Resources.NotesPath, false))
            {
                notesTextBox.SelectAll();

                writer.Write(notesTextBox.Selection.Text);
            }
        }

        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document)
        {
            string pattern = @"[#@]*[^\W\d](\w|[-']{1,2}(?=\w))*"; //@"(?:(?<=\s)|^)(\w*[A-Za-z_]+\w*)"; // 

            TextPointer pointer = document.ContentStart;

            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);

                    MatchCollection matches = Regex.Matches(textRun, pattern, RegexOptions.Multiline);

                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;

                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);

                        yield return new TextRange(start, end);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void notesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void notesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            FlowDocument doc = new FlowDocument();

            try
            {
                // set doc text
                this.Dispatcher.Invoke((Action)delegate()
                {
                    doc = notesTextBox.Document;
                });

                TextRange textRange = new TextRange(doc.ContentStart, doc.ContentEnd);

                IEnumerable<TextRange> wordRanges = GetAllWordRanges(doc);

                foreach (TextRange wordRange in wordRanges)
                {
                    if (wordRange.Text.Contains("#"))
                    {
                        wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Green));
                        wordRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    }
                    else if (wordRange.Text.Contains("@"))
                    {
                        wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
                        wordRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    }
                    else
                    {
                        wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
                        wordRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                    }
                }
            }
            catch (Exception ex)
            { System.Diagnostics.Debug.WriteLine(ex.Message); }

            e.Result = doc;
        }

        private void notesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) return;
            if (e.Result == null) return;

            this.Dispatcher.Invoke((Action)delegate()
            {
                notesTextBox.Document = (FlowDocument)e.Result;
            });
        }

        private void pcoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_pcoWorker.CancellationPending)
            {
                DateTime _pcoLastUpdate;
                DateTime _pcoLastUpdate2;

                // do not check for updates if live output
                //if (PresentationSource.FromVisual(_liveOutputWindow) != null) continue;

                System.Threading.Thread.Sleep(5000);

                // get last update
                if (!DateTime.TryParse(PCO.pcoLastUpdate(), out _pcoLastUpdate)) continue;

                // convert global last PCO update
                DateTime.TryParse(pco_last_update, out _pcoLastUpdate2);

                // check for recent
                if (_pcoLastUpdate.ToString("MM/dd/yyyy HH:mm:ss") == _pcoLastUpdate2.ToString("MM/dd/yyyy HH:mm:ss")) continue;

                // update list
                loadServiceList();

                // enable play/stop buttons
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    playVideoButton.IsEnabled = true;
                    stopVideoButton.IsEnabled = true;
                });
            }
        }

        private void pcoWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void notesTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_notesWorker.IsBusy) _notesWorker.RunWorkerAsync();
        }
    }
}
