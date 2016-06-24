using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ViewSwitchingNavigation.Infrastructure.PropertyProvider;

namespace ViewSwitchingNavigation.Infrastructure
{
   public class TestInfo
    {
        
        public static String ParmKey = "TESTTYPE";
        public Object sender { get; set; }
        public Dictionary<String,String> infiDic { get; set; }
        public PropertyProvider.TestType  testType { get; set; }
        public WindowsMethod testMethod { get; set; }
        public String error { get; set; }
        public Uri viewURl { get; set; }

    }
}
