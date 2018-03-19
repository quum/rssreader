using System.Collections.Generic;
using System.Xml.Serialization;
namespace RSSLib
{


    [XmlRoot(ElementName = "head")]
    public class Head
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "outline")]
    public class Outline
    {
        [XmlAttribute(AttributeName = "text")]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "xmlUrl")]
        public string XmlUrl { get; set; }
        [XmlAttribute(AttributeName = "htmlUrl")]
        public string HtmlUrl { get; set; }
    }

    [XmlRoot(ElementName = "body")]
    public class Body
    {
        [XmlElement(ElementName = "outline")]
        public List<Outline> Outline { get; set; }
    }

    [XmlRoot(ElementName = "opml")]
    public class Opml
    {
        [XmlElement(ElementName = "head")]
        public Head Head { get; set; }
        [XmlElement(ElementName = "body")]
        public Body Body { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

}