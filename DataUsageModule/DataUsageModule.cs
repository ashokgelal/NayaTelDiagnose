using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using DataUsageModule.Views;

namespace DataUsageModule
{
    [ModuleExport(typeof(DataUsageModule))]
    public class DataUsageModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(DataUsageNavigationItemView));

            //this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(WifiInspectorView));


        }
    }
}
