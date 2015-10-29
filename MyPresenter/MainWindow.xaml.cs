using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyPresenter
{    
    public class ServiceItem
    {
        [XmlAttribute(AttributeName="type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName="title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "error")]
        public bool Error { get; set; }

        [XmlAttribute(AttributeName="path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName="pad")]
        public string Pad { get; set; }

        [XmlAttribute(AttributeName = "loop")]
        public string Loop { get; set; }

        [XmlIgnore]
        public List<string> Pads { get; set; }

        [XmlIgnore]
        public List<string> Loops { get; set; }

        public ServiceItem()
        {
            FileInfo[] padsDi = new DirectoryInfo(Properties.Resources.PadPath).GetFiles("*.mp3");
            FileInfo[] loopsDi = new DirectoryInfo(Properties.Resources.LoopPath).GetFiles("background*.mpg");

            Pads = new List<string>();

            Pads.Add("");

            foreach (FileInfo pad in padsDi)
                Pads.Add(System.IO.Path.GetFileNameWithoutExtension(pad.Name));

            Loops = new List<string>();

            Loops.Add("");

            foreach (FileInfo loop in loopsDi)
                Loops.Add(System.IO.Path.GetFileNameWithoutExtension(loop.Name.Remove(0, 11)));
        }

        public ServiceItem(bool error)
        {
            Error = error;
        }

        public ServiceItem(bool error, string title)
        {
            Error = error;
            Title = title;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://openlyrics.info/namespace/2009/song", IsNullable=false)]
    public class song
    {
        public songProperties properties { get; set; }
    
        [System.Xml.Serialization.XmlArrayItemAttribute("verse", IsNullable=false)]
        public songVerse[] lyrics { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string createdIn { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string modifiedIn { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime modifiedDate { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    public class songProperties
    {
        public songPropertiesTitles titles { get; set; }
        public songPropertiesAuthors authors { get; set; }
        public string key { get; set; }
        public string loop { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    public class songPropertiesTitles { public string title { get; set; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    public class songPropertiesAuthors { public string author { get; set; } }

    /// <remarks/>
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    //public class songPropertiesKey { public string key { get; set; } }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    public class songVerse
    {
        public songVerseLines lines { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://openlyrics.info/namespace/2009/song")]
    public class songVerseLines
    {
        //[System.Xml.Serialization.XmlElementAttribute("br")]
        //public object[] br { get; set; }
    
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text { get; set; }
    }

    public static class Serializer
    {
        #region Functions
        public static void SerializeToXML<ServiceItem>(ObservableCollection<ServiceItem> t)
        {
            XmlSerializer serializer = new XmlSerializer(t.GetType());

            using (TextWriter textWriter = new StreamWriter(Properties.Resources.SettingsFilePath))
            {
                serializer.Serialize(textWriter, t);
            }
        }

        public static ObservableCollection<ServiceItem> DeserializeFromXML<ServiceItem>()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ObservableCollection<ServiceItem>), new XmlRootAttribute("ArrayOfServiceItem"));
            ObservableCollection<ServiceItem> retVal = null;

            try
            {
                using (TextReader textReader = new StreamReader(Properties.Resources.SettingsFilePath))
                {
                    retVal = (ObservableCollection<ServiceItem>)deserializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return retVal;
        }
        
        public static void SerializeToXML<song>(song t, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(t.GetType());

            using (TextWriter textWriter = new StreamWriter(filePath))
            {
                serializer.Serialize(textWriter, t);
            }
        }

        public static song DeserializeFromXML<song>(string filePath)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(song));
            song retVal = default(song);

            try
            {
                using (TextReader textReader = new StreamReader(filePath))
                {
                    retVal = (song)deserializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading  '" + System.IO.Path.GetFileNameWithoutExtension(filePath) + "'.\n\n" + ex.InnerException.Message, "Song Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                //Console.Write(ex.Message);
            }

            return retVal;
        }
        #endregion
    }

    public class LineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string total = "";

            foreach (string val in values)
                total += "\n" + val;

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
        public event PropertyChangedEventHandler PropertyChanged;

        private LiveOutputWindow _liveOutputWindow;
        private MediaElement _liveOutputMediaPlayer;
        private MediaPlayer _audioPlayer;
        private bool _useDarkText;
        private bool _muteAudio;
        private FileSystemWatcher _watcher;
        private ObservableCollection<ServiceItem> _serviceItemList;
        private string pco_last_update;

        public MainWindow()
        {
            InitializeComponent();

            scriptTextBlock.DataContext = this;

            _serviceItemList = new ObservableCollection<ServiceItem>();
        }

        private bool UseDarkText
        {
            get
            {
                return _useDarkText;
            }
            set
            {
                _useDarkText = value;

                RaisePropertyChanged("UseDarkText");
            }
        }

        private bool MuteAudio
        {
            get
            {
                return _muteAudio;
            }
            set
            {
                _liveOutputMediaPlayer.IsMuted = value;

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
            loadSongs();
            loadBackgroundLoops();
            loadBackgroundAudio();
            loadBumperVideos();
            loadBackgroundImages();
            loadLiveOutputWindow(Properties.Resources.PlaceholderImagePath);
            //loadServiceList();
            
            // set up directory watchers
            _watcher = new FileSystemWatcher(Properties.Resources.LibraryPath, "*.*");
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.Renamed += new RenamedEventHandler(FileSystemRenamed);
            _watcher.Created += new FileSystemEventHandler(FileSystemChanged);
            _watcher.Deleted += new FileSystemEventHandler(FileSystemChanged);

            // set up low res timer
            DispatcherTimer fiveSecondTimer = new System.Windows.Threading.DispatcherTimer();
            fiveSecondTimer.Tick += fiveSecondTimer_Tick;
            fiveSecondTimer.Interval = new TimeSpan(0, 0, 5);
            fiveSecondTimer.Start();

            // set up high res timer
            DispatcherTimer oneSecondTimer = new System.Windows.Threading.DispatcherTimer();
            oneSecondTimer.Tick += oneSecondTimer_Tick;
            oneSecondTimer.Interval = new TimeSpan(0, 0, 1);
            oneSecondTimer.Start();

            // set service data context
            serviceListBox.ItemsSource = _serviceItemList;
            serviceListBox.SelectedIndex = -1;

            // set window caption
#if DEBUG
           this.Title = "MyPresenter - DEBUG";
#else
           this.Title = "MyPresenter - " + DateTime.Now.ToString("MMM d, yyyy");
#endif

            //WebBrowser1.Source = new Uri("https://play.spotify.com/user/spotify/playlist/6Higf6awk4pfVmjvlaCn7b?play=true&utm_source=open.spotify.com&utm_medium=open"); //Properties.Resources.SpotifyPath);
        }

        // Define the event handlers. 
        private void FileSystemChanged(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.SongPath) loadSongs();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.VideoPath) loadBumperVideos();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.PadPath) loadBackgroundAudio();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.LoopPath) loadBackgroundLoops();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.ImagePath) loadBackgroundImages();
        }

        private void FileSystemRenamed(object source, RenamedEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Properties.Resources.SongPath) loadSongs();
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
            { }
        }

        void _liveOutputMediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void loadServiceList()
        {
            _serviceItemList.Clear();

            try
            {
                pcoItems.Items items = PCO.getItems();
                var rand = new Random();
                var files = Directory.GetFiles(Properties.Resources.LoopPath, "background*.mpg");

                for (int i = 0; i < items.data.Count; i++)
                {
                    if (items.data[i].attributes.item_type == "song")
                    {
                        if (!File.Exists(Properties.Resources.SongPath + items.data[i].attributes.title + ".xml"))
                        {
                            System.Windows.MessageBox.Show("Lyrics for '" + items.data[i].attributes.title + "' are missing.", "Song Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                            _serviceItemList.Add(new ServiceItem(true, "MISSING: " + items.data[i].attributes.title));

                            continue;
                        }

                        song _song = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + items.data[i].attributes.title + ".xml");
                        string _loop = _song.properties.loop;

                        if (_loop == "") _loop =  files[rand.Next(files.Length)];

                        if (_song != null)
                            _serviceItemList.Add(new ServiceItem() { Title = _song.properties.titles.title, Pad = _song.properties.key, Loop = System.IO.Path.GetFileNameWithoutExtension(_loop.Replace("background_", "")) });
                        else // error loading file
                            _serviceItemList.Add(new ServiceItem(true, "FILE: " + items.data[i].attributes.title));
                    }
                }

                pco_last_update = PCO.pcoLastUpdate();
            }
            catch (Exception ex)
            {
                if (!File.Exists(Properties.Resources.SettingsFilePath)) return;
                if (new FileInfo(Properties.Resources.SettingsFilePath).Length == 0) return;

                _serviceItemList = Serializer.DeserializeFromXML<ServiceItem>();

                serviceListBox.SelectedIndex = -1;

                //System.Windows.MessageBox.Show("Error importing from PCO.", "PCO Import Error - OFFLINE", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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

            // set countdown display
            if (DateTime.TryParseExact(startTimeTextBox.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out startTime))
            {
                ts = new TimeSpan(startTime.Ticks - DateTime.Now.Ticks);

                countdownTimeLabel.Content = string.Format("{0:%h}h {0:%m}m {0:%s}s", ts);
            }
            else
                countdownTimeLabel.Content = "";

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
            { }

            // AUTO START
            try
            {
                if (DateTime.Now.CompareTo(startTime.AddMinutes(-5)) >= 0)
                {
                    string _startTime = startTimeTextBox.Text;

                    startTimeTextBox.Text = "";

                    MuteAudio = true;  

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

        private void fiveSecondTimer_Tick(object sender, EventArgs e)
        {
            DateTime _pcoLastUpdate;

            // do not check for updates if live output
            if (PresentationSource.FromVisual(_liveOutputWindow) != null) return;

            // check PCO for plan updates
            if (DateTime.TryParse(PCO.pcoLastUpdate(), out _pcoLastUpdate))
                if (_pcoLastUpdate != Convert.ToDateTime(pco_last_update))
                    loadServiceList();
        }

        private void goLive(DateTime startTime)
        {
            //long video = _liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks;
            long offset = startTime.Ticks - DateTime.Now.Ticks;

            if (_liveOutputMediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MuteAudio = true;

                if (offset > _liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks) offset = 0;
                
                _liveOutputMediaPlayer.Clock.Controller.Seek(new TimeSpan(_liveOutputMediaPlayer.NaturalDuration.TimeSpan.Ticks - offset), TimeSeekOrigin.BeginTime);
                _liveOutputMediaPlayer.Clock.Controller.Resume();
            }
            
            if (!_liveOutputWindow.IsVisible) _liveOutputWindow.Show();

            playVideoButton.IsEnabled = false;
        }

        private void stopLive()
        {
            _liveOutputMediaPlayer.Clock = null;

            _audioPlayer.Stop();

            _liveOutputWindow.Close();

            VideoDisplay.Fill = null;

            playVideoButton.IsEnabled = true;

            BumperVideoComboBox.SelectedIndex = 0;
            BackgroundAudioComboBox.SelectedIndex = 0;
            BackgroundVideoComboBox.SelectedIndex = 0;
            backgroundImageComboBox.SelectedIndex = 0;
        }

        private void playVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresentationSource.FromVisual(_liveOutputWindow) == null)
            {
                if (BackgroundVideoComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.LoopPath + BackgroundVideoComboBox.SelectedItem.ToString());
                else if (BumperVideoComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.VideoPath + BumperVideoComboBox.SelectedItem.ToString());
                else if (backgroundImageComboBox.SelectedIndex > 0)
                    loadLiveOutputWindow(Properties.Resources.ImagePath + backgroundImageComboBox.SelectedItem.ToString());
                else
                    loadLiveOutputWindow(Properties.Resources.PlaceholderImagePath);
            }

            _liveOutputMediaPlayer.IsMuted = MuteAudio;
            _liveOutputMediaPlayer.Clock.Controller.Resume();

            playVideoButton.IsEnabled = false;

            _liveOutputWindow.Show();
        }

        private void pauseVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _liveOutputMediaPlayer.Clock.Controller.Pause();

            playVideoButton.IsEnabled = true;
        }

        private void stopVideoButton_Click(object sender, RoutedEventArgs e)
        {
            stopLive();
        }

        private void loadSongs()
        {
            if (this.Dispatcher.CheckAccess())
            {
                DirectoryInfo di = new DirectoryInfo(Properties.Resources.SongPath);
                var directories = di.GetFiles("*.xml");

                SongList.Items.Clear();

                //foreach (FileInfo d in directories)
                //    System.IO.File.Move(d.FullName, d.FullName.Remove(d.FullName.IndexOf(")") - 1, 2));

                foreach (FileInfo d in directories)
                    SongList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(d.Name));
            }
            else
                 this.Dispatcher.Invoke(new Action(loadSongs));
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

        private void setVideo(Uri videoUri)
        {
            MediaTimeline mTimeLine = new MediaTimeline(videoUri);

            if (videoUri.ToString().Contains("Loop")) mTimeLine.RepeatBehavior = RepeatBehavior.Forever;
            
            MediaClock mClock = mTimeLine.CreateClock();

            _liveOutputMediaPlayer.Clock = mClock;
            //_liveOutputMediaPlayer.Clock.Controller.Begin();
            //_liveOutputMediaPlayer.Clock.Controller.Pause();
        }

        private void setSong(string songName)
        {
            //_currentSong = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + "\\" + songName + ".xml");

            //var q = (
            //        from l in _currentSong.lyrics
            //        select l.lines.Text).ToList();

            XmlReaderSettings settings = new XmlReaderSettings();

            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;

            using (XmlReader reader = XmlReader.Create(Properties.Resources.SongPath + songName + ".xml", settings))
            {
                //_currentSong = new ObservableCollection<string>;

                ScriptListView.Items.Clear();

                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element) continue;

                    reader.ReadToFollowing("lines");

                    string _dis = reader.ReadInnerXml();

                    _dis = _dis.Replace("<br />", "\n");
                    _dis = _dis.Replace("<br xmlns=\"http://openlyrics.info/namespace/2009/song\" />", "\n");
                    _dis = _dis.Replace("<lines xmlns=\"http://openlyrics.info/namespace/2009/song\" />", "\n");
                    _dis = _dis.Replace("<lines xmlns=\"http://openlyrics.info/namespace/2009/song\">", "");
                    _dis = _dis.Replace("</lines>", "\n");

                    ScriptListView.Items.Add(_dis);
                }
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

            setVideo(new Uri(Properties.Resources.ImagePath + backgroundImageComboBox.SelectedValue.ToString()));

            if (PresentationSource.FromVisual(_liveOutputWindow) != null) _liveOutputMediaPlayer.Clock.Controller.Resume();
        }

        private void ScriptListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ScriptListView.SelectedIndex == -1 || ScriptListView.SelectedIndex == 0) return;

            //ScriptListView.ScrollIntoView(ScriptListView.SelectedItem);

            scriptTextBlock.Text = "";

            // do not continue if nothing is selected
            if (ScriptListView.SelectedIndex < 0) return;
            
            // show text in preview window
            scriptTextBlock.Text = ScriptListView.SelectedItem.ToString();

            // display text in output window
            _liveOutputWindow.DisplayText(ScriptListView.SelectedItem.ToString());
        }

        private void SongList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            song _song = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + SongList.SelectedItem.ToString() + ".xml");

            _serviceItemList.Add(new ServiceItem() { Title = _song.properties.titles.title, Pad = _song.properties.key });

            SongList.SelectedIndex = -1;

            textBox1.Focus();
            textBox1.SelectAll();
        }

        private void serviceListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            object item = serviceListBox.SelectedItem;
            int index = serviceListBox.SelectedIndex;

            //if (Keyboard.IsKeyDown(Key.F5))
            //{
            //    int breakCount = 1;

            //    for (int i = 0; i < serviceListBox.Items.Count; i++)
            //        if (serviceListBox.Items[i].ToString().Contains("Break")) breakCount++;

            //    serviceListBox.Items.Insert(index + 1, "Break " + breakCount.ToString() + " - 10:00");
            //}

            if (e.Key == Key.OemMinus)
                _serviceItemList.Remove((ServiceItem)serviceListBox.SelectedItem);
        }

        private void serviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresentationSource.FromVisual(_liveOutputWindow) == null) return;

            if (serviceListBox.SelectedIndex < 0) ScriptListView.Items.Clear();
            if (serviceListBox.SelectedIndex < 0) return;

            _liveOutputWindow.DisplayText("");

            ServiceItem _song = (ServiceItem)serviceListBox.SelectedItem;

            setSong(_song.Title);

            startPad(getPadSelection(serviceListBox.SelectedItem));
            setLoop(getLoopSelection(serviceListBox.SelectedItem));
            
            //if (serviceListBox.SelectedItem.ToString().Contains("Break"))
            //    gotoBreak(new Uri(Properties.Resources.ImagePath + "\\LivingRoomLive.jpg", UriKind.Absolute));
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

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text == "" || textBox1.Text == "Search")
                loadSongs();
            else
            {
                SongList.Items.Clear();

                DirectoryInfo di = new DirectoryInfo(Properties.Resources.SongPath);
                var directories = di.GetFiles("*.xml");

                foreach (FileInfo d in directories)
                {
                    if (d.FullName.Contains(textBox1.Text) && !FindListViewItem(SongList))
                        SongList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(d.FullName));
                }
            }
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

        private void textBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text == "Search") textBox1.Text = "";
        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text == "") textBox1.Text = "Search";
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

                    //ScriptListView.SelectedIndex = ScriptListView.Items.Count - 1;
                    //ScriptListView.Focus();
                    //ScriptListView.ScrollIntoView(ScriptListView.SelectedItem);
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

            foreach (ServiceItem serviceItem in serviceListBox.Items)
            {
                song _song = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + serviceItem.Title + ".xml");

                _song.properties.key = serviceItem.Pad;
                _song.properties.loop = serviceItem.Loop;

                Serializer.SerializeToXML<song>(_song, Properties.Resources.SongPath + serviceItem.Title + ".xml");
            }
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

            _audioPlayer = new MediaPlayer();
            _audioPlayer.Open(new Uri(Properties.Resources.PadPath + PadToPlay + ".mp3"));
            _audioPlayer.Play();
        }

        private void setLoop(string LoopToPlay)
        {
            if (LoopToPlay == "") return;

            if (LoopToPlay.IndexOf("_") < 0) LoopToPlay = "background_" + LoopToPlay;

            //DoubleAnimation __ani = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(3), FillBehavior.Stop);

            //__ani.Completed += (o, e) =>
            //{
            //    //_liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, null);

                setVideo(new Uri(Properties.Resources.LoopPath + LoopToPlay + ".mpg", UriKind.Absolute));

                _liveOutputMediaPlayer.Clock.Controller.Resume();

            //    // fade in
            //    _liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(1), FillBehavior.Stop));
            //};

            //// fade out
            //_liveOutputWindow.BeginAnimation(MediaElement.OpacityProperty, __ani);
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

            ServiceItem _song = (ServiceItem)serviceListBox.SelectedItem;

            setSong(_song.Title);

            startPad(getPadSelection(serviceListBox.SelectedItem));
            setLoop(getLoopSelection(serviceListBox.SelectedItem));
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
                //
            }
            finally { }

        }

        private void muteImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MuteAudio = !MuteAudio;
        }
    }
}
