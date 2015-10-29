using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPresenter
{
    /// <summary>
    /// Interaction logic for LiveOutputWindow.xaml
    /// </summary>
    public partial class LiveOutputWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _useDarkText;

        public LiveOutputWindow()
        {
            InitializeComponent();

            scriptTextBlock.DataContext = this;
        }

        public bool UseDarkText
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

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LiveWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            this.Hide();

            if (VideoPlayer.Clock == null) VideoPlayer.Stop();
            if (VideoPlayer.Clock != null) VideoPlayer.Clock.Controller.Stop();
        }

        public void DisplayText(string textToDisplay)
        {
            scriptTextBlock.Text = "";

            if (textToDisplay != "") scriptTextBlock.Text = textToDisplay;
        }

        public void DisplayVerse(string textToDisplay)
        {
            //verseTextBlock.Text = "";

            //if (textToDisplay != "") verseTextBlock.Text += textToDisplay + "\r";
        }

        private int getBrightness()
        {
            //Bitmap originalImage;
            //Bitmap adjustedImage;
            //float brightness = 1.0f; // no change in brightness
            //float contrast = 2.0f; // twice the contrast
            //float gamma = 1.0f; // no change in gamma

            //float adjustedBrightness = brightness - 1.0f;
            //// create matrix that will brighten and contrast the image
            //float[][] ptsArray ={
            //    new float[] {contrast, 0, 0, 0, 0}, // scale red
            //    new float[] {0, contrast, 0, 0, 0}, // scale green
            //    new float[] {0, 0, contrast, 0, 0}, // scale blue
            //    new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
            //    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            //imageAttributes = new ImageAttributes();
            //imageAttributes.ClearColorMatrix();
            //imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            //imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            //Graphics g = Graphics.FromImage(adjustedImage);
            //g.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
            //    , 0, 0, originalImage.Width, originalImage.Height,
            //    GraphicsUnit.Pixel, imageAttributes);

            return 60;
        }
    }
}
