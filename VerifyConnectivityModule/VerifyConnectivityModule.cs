using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using VerifyConnectivityModule.Views;

namespace VerifyConnectivityModule
{
    [ModuleExport(typeof(VerifyConnectivityModule))]
    public class VerifyConnectivityModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(VerifyConnectivityNavigationItemView));

            //this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(WifiInspectorView));


        }
    }
}
