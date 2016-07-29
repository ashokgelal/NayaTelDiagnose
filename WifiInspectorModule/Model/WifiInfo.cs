using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WifiInspectorModule.Model
{
   public  class WifiInfo
    {
        public string SSID { get; set; }
        public string BSS { get; set; }
        public string Channel { get; set; }
        public string DefualtGateway { get; set; }
        public string DefualtGatewayMac { get; set; }
        public string IPv4 { get; set; }
        public string IPv6 { get; set; }

        public string MAC { get; set; }
        public string Signal { get; set; }
        public string Vendor { get; set; }
        public string RSSID { get; set; }
        public string Security { get; set; }
        public string Speed { get; set; }

        public string Frequency { get; set; }
        public string GHZ4 { get; set; }
        public string GHZ5 { get; set; }
        public string OverlappingAPS { get; set; }

        public string Family { get; set; }


    }
}
