using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace ViewSwitchingNavigation.Infrastructure
{
    public class Constants
    {
        public static String EMAIL_SENDER = "ndoctor@nayatel.com";
        public static String SMTP_HOST_NAME = "mail.nayatel.com";
        public static int SMTP_PORT = 25;
        public static String SMTP_PASSWORD = "gvkDS8Cn";
        public static String EMAIL_RECEIVER_SUPPORT = "support@nayatel.com";
        public static String EMAIL_RECEIVER_SALE = "sales@nayatel.com";

        public static String SUBJECT = "NDoctor Diagnose";

        //FPT Property
        public static String FTP_USER_NAME = "ndocter";
        public static String FTP_PASSWORD = "2N5%Bv3Vg$";
        public static String FTP_UPLOAD_FILE= System.Environment.CurrentDirectory +
                                      Path.DirectorySeparatorChar + "ftp"+ Path.DirectorySeparatorChar+"5MB.zip";
         public static String FTP_URL = "ftp://webdrive.nayatel.com";
        public static String FTP_DOWNLOAD_URL = "ftp://webdrive.nayatel.com/NDoctor/5MB.zip";

        //public static String FTP_DOWNLOAD_LOCATION =  System.Environment.CurrentDirectory +
                                     // Path.DirectorySeparatorChar +
                                      //"ftp\\";
        //FTP SPEED TEST THREAD
        public static int FTP_ITEREATION = 5;


        /// <summary>
        /// Nayatel Conectivity Constant
        /// </summary>
        public static String CONECTIVITY_NAYATEL_URL = "http://nayatel.com/verify_ntl_user.php";
        /// <summary>
        ///  Nayatel CONFIG Constant
        /// </summary>
        /// 
        public static String CONFIG_NAYATEL_URL = "http://www.nayatel.com/ndoc/ntl_confs.json";

        public static String CONFIG_NAYATEL_FILE = UtilFiles.GetTemporaryDirectory() + "ntl_confs.json";

        //Vendor Json File
        public static String Vendor_MAC_FILE = "vendor_conf.json";
        //User Date Json File like email and number 
        public static String CONFIG_User_FILE = UtilFiles.GetTemporaryDirectory() + "user_conf.json";


        //public static String EMAIL_SENDER = "r.jawadhussain@gmail.com";
        //public static String SMTP_HOST_NAME = "smtp.gmail.com";
        //public static int SMTP_PORT = 587;
        //public static String SMTP_PASSWORD = "salahuddinayubi";
        //public static String EMAIL_RECEIVER = "jawad.hussain@mercurialminds.com";
        //public static String SUBJECT = "NDoctor Diagnose";

        //ERROR MESSAGE AND ALERT MESSAGES
        public static String MESSAGES_WIFI_NOT_CONNECTED = "Your Wi-Fi/Ethernet is disabled";
        public static String MESSAGES_INTERNET_NOT_CONNECTED = "You are not connected on Internet";
        public static String MESSAGES_NOT_NAYATEL_USER = "You are connected on Internet but not using NAYAtel connection";
        public static String MESSAGES_NOT_NAYATEL_NOT_RESPONSE = "Nayatel server not responding";
        public static String MESSAGES_NAYATEL_USER = "You are connected on internet using nayatel";
        //Long message description
        public static String MESSAGES_DESCRIPTION_WIFI_NOT_CONNECTED = "You are not connected on Internet. Please call NAYAtel Support (051-111-11-44-44)";
        public static String MESSAGES_DESCRIPTION_INTERNET_NOT_CONNECTED = "You are not connected on Internet. Please call NAYAtel Support (051-111-11-44-44)";
        public static String MESSAGES_DESCRIPTION_NOT_NAYATEL_USER = "You are connected on Internet but not using NAYAtel connection. Please subscribe to NAYAtel connection by calling us at UAN 111-11-44-44 or emailing us at sales@nayatel.com";
        public static String MESSAGES_DESCRIPTION_NOT_NAYATEL_NOT_RESPONSE = MESSAGES_DESCRIPTION_INTERNET_NOT_CONNECTED;//"Nayatel server not responding";
 

        //MTR CONSTANTS
        public static int INDIVIDUAL_PING_TIME_OUT = 3000;
        public static int DIAGNOSEALL_PING_TIME_OUT = 500;



        //EMAIL HTML VARIABLE CONSTANT
        public static String HTML_PRIMARY_COLOR = "#ECECEE";
        public static String HTML_SECONDERY_COLOR = "#F7F7F9";
        public static String HTML_HEADER_COLOR = "#2b66b2";

        public static String HTML_REPLACE_STRING = "VALUE";
        public static String HTML_REPLACE_STRING_COLOR = HTML_PRIMARY_COLOR;

        public static String HTML_START_HTML = String.Format(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><html><head><META http-equiv=""Content-Type"" content=""text/html; charset=utf-8""></head><body>");
        public static String HTML_END_HTML = String.Format(@"</body></html>");
        public static String HTML_START_TABLE = String.Format(@"<table style=""width:100%; table-layout: fixed;"" cellpadding=""2"" align=""center"" cellspacing=""1"" >");
        public static String HTML_END_TABLE = String.Format(@"</table>");
        public static String HTML_START_TR = String.Format(@"<tr height=""40"" >");
        public static String HTML_END_TR = String.Format(@"</tr>");
        public static String HTML_TH = String.Format(@"<th colspan=""2"" align=""center"" bgcolor=""{1}""><font color=""WHITE"" height=""100"">{0}</font></th>", HTML_REPLACE_STRING, HTML_REPLACE_STRING_COLOR);
        public static String HTML_TD = String.Format(@"<td    colspan=""2"" align=""left"" bgcolor=""{1}""><font color=""BLACK"" size=""1""  > {0}</font></td>", HTML_REPLACE_STRING, HTML_REPLACE_STRING_COLOR);
        public static String HTML_EMAIL_TITTLE = HTML_START_TABLE + HTML_START_TR + HTML_TH.Replace(HTML_REPLACE_STRING_COLOR, HTML_HEADER_COLOR) + HTML_END_TR + HTML_END_TABLE;
        public static String HTML_EMAIL_REGARDS = String.Format(@"<table style = ""width:80% "" cellpadding = ""7"" align = ""center"" ><tr><th align=""left"">Regards,</th></tr><tr><th align=""left"">NAYAtel nDoctor</th></tr></table>");

         


    }
}
