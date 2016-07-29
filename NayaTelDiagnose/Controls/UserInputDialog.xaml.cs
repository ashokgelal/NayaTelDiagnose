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
using ViewSwitchingNavigation.Infrastructure;

namespace NayaTelDiagnose.Controls
{
    /// <summary>
    /// Interaction logic for MyDialog.xaml
    /// </summary>
    public partial class UserInputDialog : Window
    {
        public UserInputDialog()
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
                errormessage.Text = Constants.MESSAGES_INPUT_ERROR_EMPTY_EMAIL;
                EmailTextBox.Focus();
                return;
            }

           
              if (!Regex.IsMatch(EmailTextBox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = Constants.MESSAGES_INPUT_ERROR_EMPTY_INVALID_EMAIL;
                EmailTextBox.Select(0, EmailTextBox.Text.Length);
                EmailTextBox.Focus();
                 return;

            }
            if (PhoneNumberText.Length == 0)
            {
                errormessage.Text = Constants.MESSAGES_INPUT_ERROR_EMPTY_PHONE_NUMBER;
                PhoneNumberTextBox.Focus();
                return;

            }
            if (PhoneNumberText.Length < 10 || PhoneNumberText.Length >15|| !long.TryParse(PhoneNumberText, out phoneNumber)) {
                errormessage.Text = Constants.MESSAGES_INPUT_ERROR_EMPTY_INVALID_PHONE_NUMBER;
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

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            errormessage.Text = "";

        }

        private void PhoneNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            errormessage.Text = "";
        }
    }
}
