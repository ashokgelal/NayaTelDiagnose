using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace NayaTelDiagnose
{
    /// <summary>
    /// Interaction logic for MessageDialogWindow.xaml
    /// </summary>
    public partial class MessageDialogWindow : Window
    {
        public MessageDialogWindow()
        {
            InitializeComponent();
        }
        public MessageDialogWindow(String Message,   String tittle1)
        {
            InitializeComponent();
            showMessage(Message,tittle1);
        }
        private string MesssageText;
        private string TitleText;

        public Boolean showMessage(String Message ,String tittle1) {
            this.MesssageText = Message;
            this.TitleText = tittle1;
            this.textBlockTittle.Text = TitleText;
            this.textBlockMiddleMessage.Text = this.MesssageText;

             return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Closing -= Window_Closing;
            //e.Cancel = true;
            //var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.5));
            //anim.Completed += (s, _) => this.Close();
            //this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
