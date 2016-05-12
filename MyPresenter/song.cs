using System;
using System.Collections;

namespace MyPresenter.New
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ai4.co/namespace/2016/song")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://ai4.co/namespace/2016/song", IsNullable = false)]
    public class song
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("verse", IsNullable = false)]
        public Lyrics lyrics { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string author { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string key { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string loop { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime modifiedDate { get; set; }

        public song()
        {
            lyrics = new Lyrics();
            version = new decimal();
            title = String.Empty;
            author = String.Empty;
            key = String.Empty;
            loop = String.Empty;
            modifiedDate = new DateTime();
        }

        public void Save()
        {
            Serializer.SerializeToXML<New.song>(this);
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ai4.co/namespace/2016/song")]
    public class songVerse
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        public string lyric { get; set; }
    }

    public class Lyrics : ICollection
    {
        public string CollectionName;

        private ArrayList lyricArray = new ArrayList();
        
        public New.songVerse this[int index]
        {
            get { return (New.songVerse)lyricArray[index]; }
        }

        public New.songVerse this[string name]
        {
            get
            {
                New.songVerse _verse = new New.songVerse();

                for (int i = 0; i < lyricArray.Count; i++)
                {
                    if (((New.songVerse)lyricArray[i]).name == name)
                        _verse = (New.songVerse)lyricArray[i];
                }

                return _verse;
            }
        }

        public void CopyTo(Array a, int index)
        {
            lyricArray.CopyTo(a, index);
        }
        public int Count
        {
            get { return lyricArray.Count; }
        }
        public object SyncRoot
        {
            get { return this; }
        }
        public bool IsSynchronized
        {
            get { return false; }
        }
        public IEnumerator GetEnumerator()
        {
            return lyricArray.GetEnumerator();
        }

        public void Add(New.songVerse newVerse)
        {
            lyricArray.Add(newVerse);
        }

        public void Insert(int index, object value)
        {
            lyricArray.Insert(index, value);
        }

        public void Remove(int index)
        {
            lyricArray.RemoveAt(index);
        }
    }
}
