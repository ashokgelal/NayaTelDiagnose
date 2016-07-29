using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ViewSwitchingNavigation.Infrastructure
{
  public  interface IDiagnoseAllService
    {
         String getHeaderTitle();
        String getShortDescription(); 
        Uri getViewURI();
        Image getImageIcone();
    }
      
}
