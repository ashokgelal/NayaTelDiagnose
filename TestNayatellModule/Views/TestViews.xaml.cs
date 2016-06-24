using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestNayatellModule.Views
{
    /// <summary>
    /// Interaction logic for TestViews.xaml
    /// </summary>
    /// 
    [Export("TestViews")]
    public partial class TestViews : UserControl
    {
        private static Uri emailsViewUri = new Uri("/DiagnoseAllView", UriKind.Relative);

        [Import]
        public IRegionManager regionManager;

        public TestViews()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.regionManager.RequestNavigate
       (
        "AllView",
        emailsViewUri,
        (NavigationResult nr) =>
        {
            var error = nr.Error;
            var result = nr.Result;

             // put a breakpoint here and checkout what NavigationResult contains
         }
       );
            this.regionManager.RequestNavigate
   (
    "AllView",
    new Uri("/NetworkResponseView", UriKind.Relative),
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
