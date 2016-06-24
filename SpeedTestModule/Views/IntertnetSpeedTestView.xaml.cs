using Microsoft.Practices.Prism.Regions;
using SpeedTestModule.ViewModels;
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

namespace SpeedTestModule.Views
{
    /// <summary>
    /// Interaction logic for IntertnetSpeedTestView.xaml
    /// </summary>
       [Export("IntertnetSpeedTestView")]
    public partial class IntertnetSpeedTestView : UserControl, IRegionMemberLifetime
    {
        public IntertnetSpeedTestView()
        {
            InitializeComponent();
        }
        [Import]
        public SpeedTestViewModel ViewModel
        {
            get { return this.DataContext as SpeedTestViewModel; }
            set { this.DataContext = value; }
        }
        public bool KeepAlive
        {
            get { return false; }
        }
    }
}
