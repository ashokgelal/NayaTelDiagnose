
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
using VerifyConnectivityModule.ViewModels;
using ViewSwitchingNavigation.Infrastructure;

namespace VerifyConnectivityModule.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("VerifyConnectivityView")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class VerifyConnectivityView : UserControl, IRegionMemberLifetime
    {

        public VerifyConnectivityView() {
            InitializeComponent();
        }
        [Import]
        public VerifyConectivityViewModel ViewModel
        {
            get { return this.DataContext as VerifyConectivityViewModel; }
            set { this.DataContext = value; }
        }

        public bool KeepAlive
        {
            get { return false; }
        }
    }
}
