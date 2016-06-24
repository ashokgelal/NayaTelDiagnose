
using Microsoft.Practices.Prism.Regions;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewSwitchingNavigation.Infrastructure;

namespace DiagnoseAll.Views
{
    /// <summary>
    /// Interaction logic for WifiInspectorView.xaml
    /// </summary>
    /// 
    [Export("DiagnoseAllView")]
    public partial class DiagnoseAllView : UserControl
    {

        public DiagnoseAllView() {
            InitializeComponent();
        }

        private void PanelVerifyConnectivity_TouchUp(object sender, TouchEventArgs e)
        {
            
        }

        private void PanelVerifyConnectivity_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1000;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            
            if (panel.Height > 70)
            {
                panel.Height = 70;
            }
            else {
                panel.Height = 400;

            }
            Uri ImageUri = (panel.Height > 70 == true ? new Uri("/DiagnoseAll;component/Resources/arrow_up.png", UriKind.Relative) : new Uri("/DiagnoseAll;component/Resources/arrow_down.png", UriKind.Relative));
             
            foreach (Image image in FindVisualChildren<Image>(panel))
            {
                // do something with tb here
                if (image.Name.Contains("imageArrow"))
                {
                    image.Source = new BitmapImage(ImageUri);

                }
            }
            //panel.BeginAnimation(ScaleTransform.CenterXProperty, da);
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
