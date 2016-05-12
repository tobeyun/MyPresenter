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
    public class ServiceItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _progress;
        private string _pad;
        private string _loop;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "error")]
        public bool Error { get; set; }

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "pad")]
        public string Pad
        {
            get { return _pad; }

            set { _pad = value; AudioPlayer.Open(new Uri(Properties.Resources.PadPath + value)); }
        }

        [XmlAttribute(AttributeName = "loop")]
        public string Loop
        {
            get { return _loop; }

            set { _loop = value; setVideo(new Uri(Properties.Resources.LoopPath + value, UriKind.Absolute)); }
        }

        [XmlIgnore]
        public List<string> Pads { get; set; }

        [XmlIgnore]
        public List<string> Loops { get; set; }

        [XmlIgnore]
        public MediaElement VideoPlayer { get; set; }

        [XmlIgnore]
        public MediaPlayer AudioPlayer { get; set; }

        [XmlIgnore]
        public int Progress
        {
            get { return _progress; }

            set
            {
                _progress = value;

                RaisePropertyChanged("Progress");
            }
        }

        public ServiceItem()
        {
            VideoPlayer = new MediaElement();
            AudioPlayer = new MediaPlayer();

            //string loopFilter = "";

            //// specific filters to prevent announcement loops from being included in song loops
            //loopFilter = "background*.mp*";

            //FileInfo[] padsDi = new DirectoryInfo(Properties.Resources.PadPath).GetFiles("*.mp3");
            //FileInfo[] loopsDi = new DirectoryInfo(Properties.Resources.LoopPath).GetFiles(loopFilter);

            //Pads = new List<string>();

            //Pads.Add("");

            //foreach (FileInfo pad in padsDi)
            //    Pads.Add(System.IO.Path.GetFileNameWithoutExtension(pad.Name));

            //Loops = new List<string>();

            //Loops.Add("");

            //foreach (FileInfo loop in loopsDi)
            //    Loops.Add(System.IO.Path.GetFileNameWithoutExtension(loop.Name.Remove(0, loopFilter.Length - 4)));
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

        private void setVideo(Uri videoUri)
        {
            MediaTimeline mTimeLine = new MediaTimeline(videoUri);

            if (videoUri.ToString().Contains("Loop")) mTimeLine.RepeatBehavior = RepeatBehavior.Forever;

            MediaClock mClock = mTimeLine.CreateClock();

            VideoPlayer.Clock = mClock;
            //VideoPlayer.Clock.Controller.Begin();
            //VideoPlayer.Clock.Controller.Pause();
        }
    }
}
