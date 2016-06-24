
using WifiInspectorModule.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using System.Windows.Controls;


namespace WifiInspectorModule.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("WifiInspectorView")]
    public partial class WifiInspectorView : UserControl
    {

        public WifiInspectorView() {
            InitializeComponent();
        }
        [Import]
        public WifiInfoViewModel ViewModel
        {
            get { return this.DataContext as WifiInfoViewModel; }
            set { this.DataContext = value; }
        }

        
    }
}
