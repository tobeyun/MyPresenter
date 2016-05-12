using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MyPresenter
{
    public static class Serializer
    {
        #region Functions
        public static void SerializeToXML<ServiceItem>(ObservableCollection<ServiceItem> t)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(t.GetType());

                using (TextWriter textWriter = new StreamWriter(Properties.Resources.SettingsFilePath))
                {
                    serializer.Serialize(textWriter, t);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);

                // HACK: Pull our assembly base file name from exception message
                Regex regex = new Regex(@"File or assembly name (?<baseFileName>.*).dll");
                Match match = regex.Match(ex.Message);

                string baseFileName = match.Groups["baseFileName"].Value;

                if (baseFileName == string.Empty) return;

                string outputPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".out");
                System.Diagnostics.Debug.WriteLine((new StreamReader(outputPath)).ReadToEnd());

                string csPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".0.cs");
                System.Diagnostics.Debug.WriteLine("XmlSerializer-produced source:\n" + csPath);

                return;
            }
        }

        public static ObservableCollection<ServiceItem> DeserializeFromXML<ServiceItem>()
        {
            ObservableCollection<ServiceItem> retVal = null;

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ObservableCollection<ServiceItem>), new XmlRootAttribute("ArrayOfServiceItem"));

                using (TextReader textReader = new StreamReader(Properties.Resources.SettingsFilePath))
                {
                    retVal = (ObservableCollection<ServiceItem>)deserializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);

                // HACK: Pull our assembly base file name from exception message
                Regex regex = new Regex(@"File or assembly name (?<baseFileName>.*).dll");
                Match match = regex.Match(ex.Message);

                string baseFileName = match.Groups["baseFileName"].Value;

                if (baseFileName != string.Empty)
                {
                    string outputPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".out");
                    System.Diagnostics.Debug.WriteLine((new StreamReader(outputPath)).ReadToEnd());

                    string csPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".0.cs");
                    System.Diagnostics.Debug.WriteLine("XmlSerializer-produced source:\n" + csPath);
                }
            }

            return retVal;
        }

        public static void SerializeToXML<song>(New.song t)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(t.GetType());

                using (TextWriter textWriter = new StreamWriter(Properties.Resources.SongPath + "\\" + t.title + ".xml"))
                {
                    foreach (New.songVerse v in t.lyrics)
                    {
                        t.lyrics[v.name].name = v.name.ToUpper();
                        t.lyrics[v.name].lyric = v.lyric.Replace("\r\n", "_");
                    }
                    
                    serializer.Serialize(textWriter, t);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);

                // HACK: Pull our assembly base file name from exception message
                Regex regex = new Regex(@"File or assembly name (?<baseFileName>.*).dll");
                Match match = regex.Match(ex.Message);

                string baseFileName = match.Groups["baseFileName"].Value;

                if (baseFileName == string.Empty) return;

                string outputPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".out");
                System.Diagnostics.Debug.WriteLine((new StreamReader(outputPath)).ReadToEnd());

                string csPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), baseFileName + ".0.cs");
                System.Diagnostics.Debug.WriteLine("XmlSerializer-produced source:\n" + csPath);

                return;
            }
        }

        public static New.song DeserializeFromXML<song>(string filePath)
        {
            New.song retVal = default(New.song);

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(New.song));

                using (TextReader textReader = new StreamReader(filePath))
                {
                    retVal = (New.song)deserializer.Deserialize(textReader);

                    foreach (New.songVerse v in retVal.lyrics)
                    {
                        retVal.lyrics[v.name].name = v.name.ToUpper();
                        retVal.lyrics[v.name].lyric = v.lyric.Replace("_", "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading  '" + System.IO.Path.GetFileNameWithoutExtension(filePath.Replace("MISSING PAD: ", "")) + "'.\n\n" + ex.InnerException.Message, "Song Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);

                //Console.Write(ex.Message);
            }

            return retVal;
        }
        #endregion
    }
}
