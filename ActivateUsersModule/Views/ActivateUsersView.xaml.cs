
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
using ActivateUsersModule.ViewModels;

namespace ActivateUsersModule.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("ActivateUsersView")]
    public partial class ActivateUsersView : UserControl
    {

        public ActivateUsersView() {
            InitializeComponent();
        }
        [Import]
        public ActiveViewModel ViewModel
        {
            get { return this.DataContext as ActiveViewModel; }
            set { this.DataContext = value; }
        }
    }
}
