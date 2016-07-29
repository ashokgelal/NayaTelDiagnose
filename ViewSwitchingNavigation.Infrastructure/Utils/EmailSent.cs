using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Utils
{
    public class EmailSent
    {
        public static string getUserHTMLTable()
        {
            Tuple<String, String, String> item = HttpRequest.verifyConectivityTest(Constants.CONECTIVITY_NAYATEL_URL, 5000);
           String DeviceType = PropertyProvider.getPropertyProvider().getVendorByMacAddress(UtilNetwork.GetLocalMacAddress().Substring(0, 8).Replace(":", ""));


            String HTML = Constants.HTML_START_TABLE +
             Constants.HTML_START_TR +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s Email Address") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, PropertyProvider.getPropertyProvider().getEmailAddress()) +
             Constants.HTML_END_TR + Constants.HTML_START_TR +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s Mobile Number") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, PropertyProvider.getPropertyProvider().getPhoneNumberAddress()) +
             Constants.HTML_END_TR + Constants.HTML_START_TR +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s WAN IP") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Item2) +

            Constants.HTML_END_TR + Constants.HTML_START_TR +
              Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s Local IP") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, UtilNetwork.GetLocalIPAddress()) +

                Constants.HTML_END_TR + Constants.HTML_START_TR +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s MAC Address") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, UtilNetwork.GetLocalMacAddress()) +
             Constants.HTML_END_TR + Constants.HTML_START_TR +
              Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s Device Name") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, UtilNetwork.GetLocalDeviceName()) +
             Constants.HTML_END_TR + Constants.HTML_START_TR +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Customer’s Device Type") +
             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, DeviceType) +
                         Constants.HTML_END_TR +
             Constants.HTML_END_TABLE;

            return HTML;
        }
        

        public static Boolean sentEmail(String tittle, String testHTMLBody, String To)
        {

            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, tittle);
            String HTML = Constants.HTML_START_HTML + testHTMLBody;
            HTML += Constants.HTML_EMAIL_REGARDS + Constants.HTML_END_HTML;
            String Subject = tittle;
            if (To != Constants.EMAIL_RECEIVER_SALE)
            {
                Subject += " Report : " + PropertyProvider.getPropertyProvider().getPhoneNumberAddress();


            }
            return sentEmailWithSubject(HTML, To, Subject);
        }

        public static Boolean sentEmailWithSubject(String body,String To,String Subject) {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(Constants.SMTP_HOST_NAME);
            mail.From = new MailAddress(Constants.EMAIL_SENDER);
            mail.To.Add(To);
            mail.Subject = Constants.SUBJECT + Subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpServer.Port = Constants.SMTP_PORT;
            SmtpServer.Credentials = new System.Net.NetworkCredential(Constants.EMAIL_SENDER, Constants.SMTP_PASSWORD);
            SmtpServer.EnableSsl = false;
            try
            {
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
             }
        }
    }
}
