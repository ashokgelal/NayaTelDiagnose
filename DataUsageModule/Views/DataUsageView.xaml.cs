
using DataUsageModule.ViewModels;
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

namespace DataUsageModule.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("DataUsageView")]
    public partial class DataUsageView : UserControl
    {

        public DataUsageView() {
            InitializeComponent();
        }

        [Import]
        public DataUsageViewModel ViewModel
        {
            get { return this.DataContext as DataUsageViewModel; }
            set { this.DataContext = value; }
        }
    }
}
