
using NetworkResponseModule.ViewModels;
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

namespace NetworkResponseModule.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("NetworkResponseView")]
    public partial class NetworkResponseView : UserControl
    {

        public NetworkResponseView() {
            InitializeComponent();
        }
        [Import]
        public NetworkResponseViewModel ViewModel
        {
            get { return this.DataContext as NetworkResponseViewModel; }
            set { this.DataContext = value; }
        }
    }
}
