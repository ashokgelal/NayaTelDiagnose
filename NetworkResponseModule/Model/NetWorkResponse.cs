using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkResponseModule.Model
{
   public  class NetWorkResponse
    {
        public string Count { get; set; }
        public string IPAdress { get; set; }
        public string Loss { get; set; }
        public string Rec { get; set; }
        public string Sent { get; set; }
        public string Last { get; set; }
        public string Best { get; set; }
        public string Avg { get; set; }
        public string Worst { get; set; }
        public int Index { get; set; }
    }
}
