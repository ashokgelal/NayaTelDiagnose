using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using ActivateUsersModule.Views;

namespace ActivateUsersModule
{
    [ModuleExport(typeof(ActivateUsersModule))]
    public class ActivateUsersModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(ActivateUsersNavigationItemView));

            //this.regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(WifiInspectorView));


        }
    }
}
