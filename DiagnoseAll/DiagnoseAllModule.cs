using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using DiagnoseAll.Views;

namespace DiagnoseAll
{
    [ModuleExport(typeof(DiagnoseAllModule))]
     public class DiagnoseAllModule : IModule
        {
            [Import]
            public IRegionManager regionManager;

            public void Initialize()
            {
                 this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegionDaignoseAll, typeof(DiagnoseAllNavigationItemView));

 

            }
        }
    }