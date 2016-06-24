using System.Xml.Serialization;

namespace nettools.ThirdParty.NSpeedTest.Models
{

    [XmlRoot("server-config")]
    public class ServerConfig
    {

        [XmlAttribute("ignoreids")]
        public string IgnoreIds { get; set; }

    }

}