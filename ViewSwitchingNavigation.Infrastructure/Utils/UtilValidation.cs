using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Utils
{
   public   class UtilValidation
    {
        public static bool isNumber(String value) {
            int number;
            if (int.TryParse(value, out number)) {
                return true;
            }

            return false;
        }
    }
}
