
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using System.Linq;

namespace TestNayatellModule.Views
{
    [Export]
    [ViewSortHint("01")]
    public partial class TestNavigationItemView : UserControl, IPartImportsSatisfiedNotification
    {
        private static Uri emailsViewUri = new Uri("/TestViews", UriKind.Relative);

        [Import]
        public IRegionManager regionManager;

        public TestNavigationItemView()
        {
            InitializeComponent();
        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            IRegion mainContentRegion = this.regionManager.Regions[RegionNames.MainContentRegion];
            IRegion mainNavigationRegion = this.regionManager.Regions[RegionNames.MainNavigationRegion];

            if (mainContentRegion != null && mainContentRegion.NavigationService != null)
            {
                mainContentRegion.NavigationService.Navigated += this.MainContentRegion_Navigated;
            }
            if (mainNavigationRegion != null && mainNavigationRegion.NavigationService != null)
            {
                 
            }
        }

        public void MainContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            this.UpdateNavigationButtonState(e.Uri);
              
        }

        private void UpdateNavigationButtonState(Uri uri)
        {
             IRegion mainNavigationRegion = this.regionManager.Regions[RegionNames.MainNavigationRegion];

             
            if (mainNavigationRegion != null && mainNavigationRegion.NavigationService != null)
            {
                 
            }
        }

        private void NavigateToEmailRadioButton_Click(object sender, RoutedEventArgs e)
        {
             
            this.regionManager.RequestNavigate
        (
         RegionNames.MainContentRegion,
         emailsViewUri,
         (NavigationResult nr) =>
         {
             var error = nr.Error;
             var result = nr.Result;

                 // put a breakpoint here and checkout what NavigationResult contains
             }
        );
         
        }
    }
}