using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ViewSwitchingNavigation.Infrastructure;

namespace SpeedTestModule
{
    [Export(typeof(IDiagnoseAllService))]
    [PartCreationPolicy(CreationPolicy.Shared)]

  public  class MainWindowsInfo 
    {
         
        public string getHeaderTitle()
        {
            return "Speed Test";
        }

        public Image getImageIcone()
        {
            return null;
        }

        public Uri getViewURI()
        {
            return new Uri("/SpeedTestView", UriKind.Relative);
        }

        
    }
}
