using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure;

namespace NayaTelDiagnose
{
    class MainWindowsViewsManager
    {

        [Import(typeof(IDiagnoseAllService))]
        public IDiagnoseAllService MainWindowsInfo;

        public MainWindowsViewsManager() {
        }
    }
}
