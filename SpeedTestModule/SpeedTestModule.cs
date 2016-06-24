using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using SpeedTestModule.Views;
using System;
using System.Windows.Media.Imaging;

namespace SpeedTestModule
{
    [ModuleExport(typeof(SpeedTestModule))]

    public class SpeedTestModule : IModule 
    {
        [Import]
        public IRegionManager regionManager;

     

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(SpeedTestNavigationItemView));

            //this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(WifiInspectorView));


        }
    }
}
