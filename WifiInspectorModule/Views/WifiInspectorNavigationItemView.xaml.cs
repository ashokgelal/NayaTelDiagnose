
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;
using ViewSwitchingNavigation.Infrastructure;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Media;

namespace WifiInspectorModule.Views
{
    [Export]
    [ViewSortHint("03")]
    public partial class WifiInspectorNavigationItemView : UserControl, IPartImportsSatisfiedNotification, IDiagnoseAllService

    {
        private static Uri ViewUri = new Uri("/WifiInspectorView", UriKind.Relative);

        [Import]
        public IRegionManager regionManager;

        public WifiInspectorNavigationItemView()
        {
            InitializeComponent();
        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            IRegion mainContentRegion = this.regionManager.Regions[RegionNames.MainContentRegion];
            if (mainContentRegion != null && mainContentRegion.NavigationService != null)
            {
                mainContentRegion.NavigationService.Navigated += this.MainContentRegion_Navigated;
            }
        }

        public void MainContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            this.UpdateNavigationButtonState(e.Uri);

            
            
        }

        private void UpdateNavigationButtonState(Uri uri)
        {
           
                this.navigateRadioButton.IsChecked = (uri == ViewUri);
                Uri ImageUri = (this.navigateRadioButton.IsChecked == true ? new Uri("../Images/active_radio.png", UriKind.Relative) : new Uri("../Images/in_active_radio.png", UriKind.Relative));
                this.navigateImage.Source = new BitmapImage(ImageUri);
           
        }



        private void navigateRadioButton_Click(object sender, RoutedEventArgs e)
        {
            this.regionManager.RequestNavigate
                   (
                    RegionNames.MainContentRegion,
                    ViewUri,
                    (NavigationResult nr) =>
                    {
                        var error = nr.Error;
                        var result = nr.Result;

                        // put a breakpoint here and checkout what NavigationResult contains
                    }
                   );
        }

        private void navigateRadioButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShortTextBlock.Foreground = (Brush)Application.Current.MainWindow.FindResource("BrushShortDiscriptionIn");

            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.WorkerSupportsCancellation = true;
            barInvoker.DoWork += delegate
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            Uri ImageUri = new Uri("../Images/active_radio.png", UriKind.Relative);

            this.navigateImage.Source = new BitmapImage(ImageUri);
        }));
            };

            barInvoker.RunWorkerAsync();
        }
        private void navigateRadioButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShortTextBlock.Foreground = (Brush)Application.Current.MainWindow.FindResource("BrushShortDiscriptionOut");

            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.WorkerSupportsCancellation = true;
            barInvoker.DoWork += delegate
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
               Uri ImageUri = (this.navigateRadioButton.IsChecked == true ? new Uri("../Images/active_radio.png", UriKind.Relative) : new Uri("../Images/In_active_radio.png", UriKind.Relative));
             this.navigateImage.Source = new BitmapImage(ImageUri);
             }));
            };

            barInvoker.RunWorkerAsync();
        }


        public string getHeaderTitle()
        {
            return Constants.TEST_HEADER_TITTLE_WIFI_INSPECTOR;
        }
        public string getShortDescription()
        {
            return Constants.TEST_HEADER_SHORT_DESCRIPTION_WIFI_INSPECTOR;
        }

        public Image getImageIcone()
        {
            Uri ImageUri = new Uri("/WifiInspectorModule;component/Resources/wif_inspector.png", UriKind.Relative);
            Image image = new Image();
            image.Source = new BitmapImage(ImageUri);
            return image;
        }

        public Uri getViewURI()
        {
            return ViewUri;
        }
    }
}