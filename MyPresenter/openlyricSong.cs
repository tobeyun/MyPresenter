namespace MyPresenter
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://openlyrics.info/namespace/2009/song", IsNullable = false)]
    public class song
    {
        public songProperties properties { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("verse", IsNullable = false)]
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    public class songProperties
    {
        public songPropertiesTitles titles { get; set; }
        public songPropertiesAuthors authors { get; set; }
        public string key { get; set; }
        public string loop { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    public class songPropertiesTitles { public string title { get; set; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    public class songPropertiesAuthors { public string author { get; set; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://openlyrics.info/namespace/2009/song")]
    public class songVerse
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        public string lines { get; set; }
    }
}
