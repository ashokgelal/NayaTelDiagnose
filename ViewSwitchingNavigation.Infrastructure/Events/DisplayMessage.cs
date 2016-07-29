using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Events
{
    public class DisplayMessage
    {
        public enum DisplayScreen {
            TOP,
            Middle,
            Both,
            Box
        }

        public String message;
        public String messageDescription;
        public String messageTittle;

        public Boolean isError;
        public DisplayScreen displayScren;

    }
}
