using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using WifiInspectorModule.Views;

namespace WifiInspectorModule
{
    [ModuleExport(typeof(WifiInspectorModule))]
    public class WifiInspectorModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(WifiInspectorNavigationItemView));

            //this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(WifiInspectorView));


        }
    }
}
