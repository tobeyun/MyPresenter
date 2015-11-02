using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPresenter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            //getPlanItems();
            song _song = Serializer.DeserializeFromXML<song>(Properties.Resources.SongPath + "Love Come Down.xml");

            //var q = (
            //        from l in _song.lyrics
            //        select l.lines.Text).ToList();

            foreach (songVerse s in _song.lyrics)
            {
                if (s.lines.VerseText != null)
                    TextBox1.Text += s.lines.VerseText.GetLength(0) + "\n";
            }
        }

        private void getPlanItems()
        {
            //Label1.Content = TextBox1.Text;

            //DoubleAnimation daOpacity = new DoubleAnimation();
            //DoubleAnimation daTop = new DoubleAnimation();
            //TranslateTransform tt = new TranslateTransform();

            //daOpacity.From = 1.0;
            //daOpacity.To = 0.0;
            //daOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(5000));
            //daOpacity.AutoReverse = false;
            //Label1.BeginAnimation(Label.OpacityProperty, daOpacity);

            //daTop.From = 0.0;
            //daTop.To = -50.0;
            //daTop.Duration = new Duration(TimeSpan.FromMilliseconds(5000));
            //daTop.AutoReverse = false;
            //Label1.RenderTransform = tt;
            //tt.BeginAnimation(TranslateTransform.YProperty, daTop);

            //PCO pco = new PCO();

            //JSonHelper helper = new JSonHelper();
            //Plans plan = helper.ConvertJSonToObject<Plans>(pco.getPlan());
            //Items item = helper.ConvertJSonToObject<Items>(pco.getItem(plan.data[0].links.self + "/items"));

            //for (int i = 0; i < item.data.Count; i++)
            //{
            //    if (item.data[i].attributes.item_type == "song")
            //        TextBox1.Text += item.data[i].attributes.title + "\r";
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Text = Spotify.getToken();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TextBox1.Text = Spotify.getPlaylist();
        }
    }
}
