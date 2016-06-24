using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NayaTelDiagnose.Controls
{
    /// <summary>
    /// Interaction logic for MyDialog.xaml
    /// </summary>
    public partial class MyDialog : Window
    {
        public MyDialog()
        {
            InitializeComponent();
        }

        public string EmailAddressText
        {
            get { return EmailTextBox.Text; }
            set { EmailTextBox.Text = value; }
        }
        public string PhoneNumberText
        {
            get { return PhoneNumberTextBox.Text; }
            set { PhoneNumberTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            long phoneNumber;
            if (EmailAddressText.Length == 0)
            {
                errormessage.Text = "Enter an email.";
                EmailTextBox.Focus();
                return;
            }

            if (PhoneNumberText.Length == 0)
            {
                errormessage.Text = "Enter an Cell No.";
                PhoneNumberTextBox.Focus();
                return;

            }
              if (!Regex.IsMatch(EmailTextBox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = "Enter a valid email.";
                EmailTextBox.Select(0, EmailTextBox.Text.Length);
                EmailTextBox.Focus();
                DialogResult = false;
                return;

            }
              if (PhoneNumberText.Length < 10 || PhoneNumberText.Length >15|| !long.TryParse(PhoneNumberText, out phoneNumber)) {
                errormessage.Text = "Enter a valid Cell No.";
                PhoneNumberTextBox.Select(0, PhoneNumberTextBox.Text.Length);
                PhoneNumberTextBox.Focus();
                return;

            }

            else {
                DialogResult = true;

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
