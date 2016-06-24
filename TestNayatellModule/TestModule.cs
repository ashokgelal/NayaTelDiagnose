using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using TestNayatellModule.Views;
using System;

namespace TestNayatellModule
{
    [ModuleExport(typeof(TestModule))]
    public class TestModule : IModule   
    {
        [Import]
        public  IRegionManager regionManager;
          
        public void Initialize()
        {
            
                this.regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, typeof(TestNayatellModule.Views.TestNavigationItemView));

             
        }
         
    }
}
