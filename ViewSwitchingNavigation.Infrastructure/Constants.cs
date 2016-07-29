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
         //public static String EMAIL_RECEIVER_SUPPORT = "jawad.hussain@mercurialminds.com";//"yasir.nawaz@mercurialminds.com";// 
        // public static String EMAIL_RECEIVER_SUPPORT = "yasir.nawaz@mercurialminds.com";// 
       //  public static String EMAIL_RECEIVER_SALE = EMAIL_RECEIVER_SUPPORT;

        public static String SUBJECT = "nDoctor  : ";

        //FPT Property
        public static String FTP_USER_NAME = "ndocter";
        public static String FTP_PASSWORD = "2N5%Bv3Vg$";
        // public static String FTP_UPLOAD_FILE = System.Environment.CurrentDirectory +
        // Path.DirectorySeparatorChar + "ftp" + Path.DirectorySeparatorChar + "5MB.zip";

        public static String FTP_UPLOAD_FILE = UtilFiles.GetAppDataDirectory() + "5MB.zip";
        public static String FTP_URL = "ftp://webdrive.nayatel.com/NDoctor_Desktop/";
        public static String FTP_DOWNLOAD_FILE_NAME = "speed_test_100MB.zip";
        public static String FTP_DOWNLOAD_URL = "ftp://webdrive.nayatel.com/NDoctor_Desktop/" + FTP_DOWNLOAD_FILE_NAME;

        //public static String FTP_DOWNLOAD_LOCATION =  System.Environment.CurrentDirectory +
        // Path.DirectorySeparatorChar +
        //"ftp\\";
        //FTP SPEED TEST THREAD
        public static int FTP_ITEREATION_DOWNLOAD = 20;
        public static int FTP_ITEREATION_UPLOAD = 5;


        /// <summary>
        /// Nayatel Conectivity Constant
        /// </summary>
        public static String CONECTIVITY_NAYATEL_URL = "http://nayatel.com/ndoc/verify_ntl_user.php";
        /// <summary>
        ///  Nayatel CONFIG Constant
        /// </summary>
        /// 
        public static String CONFIG_NAYATEL_URL = "http://www.nayatel.com/ndoc/ntl_confs.json";

        // public static String CONFIG_NAYATEL_FILE = UtilFiles.GetTemporaryDirectory() + "ntl_confs.json";
        public static String CONFIG_NAYATEL_FILE = "ntl_confs.json";

        //Vendor Json File
        public static String Vendor_MAC_FILE = "vendor_conf.json";
        //User Date Json File like email and number 
        public static String CONFIG_User_FILE = UtilFiles.GetAppDataDirectory() + "user_conf.json";


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
        public static String MESSAGES_NOT_NAYATEL_NOT_RESPONSE = MESSAGES_INTERNET_NOT_CONNECTED;
        public static String MESSAGES_NAYATEL_USER = "You are connected on internet using NAYAtel";
        //Long message description
        public static String MESSAGES_DESCRIPTION_WIFI_NOT_CONNECTED = "You are not connected on Internet. Please call NAYAtel Support (051-111-11-44-44)";
        public static String MESSAGES_DESCRIPTION_INTERNET_NOT_CONNECTED = "You are not connected on Internet. Please call NAYAtel Support (051-111-11-44-44)";
        public static String MESSAGES_DESCRIPTION_NOT_NAYATEL_USER = "You are connected on Internet but not using NAYAtel connection. Please subscribe to NAYAtel connection by calling us at UAN 111-11-44-44 or emailing us at sales@nayatel.com";
        public static String MESSAGES_DESCRIPTION_NOT_NAYATEL_NOT_RESPONSE = MESSAGES_DESCRIPTION_INTERNET_NOT_CONNECTED;//"Nayatel server not responding";
                                                                                                                         ////// User Input Validation ERROR
                                                                                                                         //Email validation messages
        public static String MESSAGES_INPUT_ERROR_EMPTY_EMAIL = "Email address is required.";
        public static String MESSAGES_INPUT_ERROR_EMPTY_PHONE_NUMBER = "Phone Number is required.";
        public static String MESSAGES_INPUT_ERROR_EMPTY_INVALID_EMAIL = "Enter a proper email address.";
        public static String MESSAGES_INPUT_ERROR_EMPTY_INVALID_PHONE_NUMBER = "Enter a proper Phone Number.";
        //Email sent aand faild messages
        public static String MESSAGES_EMAIL_SENT = "Email sent successfully";
        public static String MESSAGES_EMAIL_FAILED = "Email sent fail";
        //Network MTR validation messages
        public static String MESSAGES_INPUT_REPLACE_VALUE = "@VALUE";
        public static String MESSAGES_INPUT_INVALID_HOST_NAME = "Ping request could not find host " + MESSAGES_INPUT_REPLACE_VALUE + ". Please check the name and try again";
        public static String MESSAGES_INPUT_INVALID_ITERATION = "Iteration Interval: " + MESSAGES_INPUT_REPLACE_VALUE + " not valid";
        public static String MESSAGES_INPUT_INVALID_INTERVAL = "Interval: " + MESSAGES_INPUT_REPLACE_VALUE + " not valid";
        public static String MESSAGES_INPUT_INVALID_PACKET_SIZE = "Paket Size: " + MESSAGES_INPUT_REPLACE_VALUE + " not valid";
        //min and max
        public static String MESSAGES_INPUT_MIN_MAX_ITERATION = "Iteration Interval must be between " + MESSAGES_INPUT_REPLACE_VALUE + "";
        public static String MESSAGES_INPUT_MIN_MAX_INTERVAL = "Interval  must be between " + MESSAGES_INPUT_REPLACE_VALUE + "";
        public static String MESSAGES_INPUT_MIN_MAX_PACKET_SIZE = "Paket Size  must be between " + MESSAGES_INPUT_REPLACE_VALUE + "";


        //MTR CONSTANTS
        public static int INDIVIDUAL_PING_TIME_OUT = 2000;
        public static int DIAGNOSEALL_PING_TIME_OUT = 1000;



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
        public static String HTML_TH = String.Format(@"<th   align=""center"" bgcolor=""{1}""><font color=""WHITE"" height=""100"">{0}</font></th>", HTML_REPLACE_STRING, HTML_REPLACE_STRING_COLOR);
        public static String HTML_TD = String.Format(@"<td    align=""left"" bgcolor=""{1}""><font color=""BLACK"" size=""2""  > {0}</font></td>", HTML_REPLACE_STRING, HTML_REPLACE_STRING_COLOR);
        public static String HTML_EMAIL_TITTLE = HTML_START_TABLE + HTML_START_TR + HTML_TH.Replace(HTML_REPLACE_STRING_COLOR, HTML_HEADER_COLOR) + HTML_END_TR + HTML_END_TABLE;
        public static String HTML_EMAIL_REGARDS = String.Format(@"<br/><table style = ""width:30% "" cellpadding = ""2"" align = ""left"" ><tr><th align=""left"">Regards,</th></tr><tr><th align=""left"">NDoctor</th></tr></table>");
        // Email for availlin nayatel connection

        public static String HTML_EMAIL_CLIENT_BODY = "<p>Dear Sales,</p></br> <p>Following customer might be interested in availing NAYAtel services.Kindly coordinate the customer on the below mentioned details: </p></br><p>Email:" + PropertyProvider.getPropertyProvider().getEmailAddress() + "</p></br><p>POC:" + PropertyProvider.getPropertyProvider().getPhoneNumberAddress() + "</p>";
        public static String HTML_EMAIL_CLIENT_SUBJECT  = "NDoctor: Potential Customer";
      



        ///// Test Tittle Constant
        public static String TEST_HEADER_TITTLE_CONNECTIVITY = "Verify Connectivity";
        public static String TEST_HEADER_TITTLE_SPEED_TEST = "Speed Test";
        public static String TEST_HEADER_TITTLE_WIFI_INSPECTOR = "WiFi Inspector";
        public static String TEST_HEADER_TITTLE_ACTIVE_USERS = "Number of Active Users";
        public static String TEST_HEADER_TITTLE_DATA_USAGE = "Device Usage";
        public static String TEST_HEADER_TITTLE_NETWORK_RESPONSE = "Network Response";
        public static String TEST_HEADER_TITTLE_DIAGNOSE_ALL = "Diagnose All";
        /// <summary>
        /// Test Short Description
        /// </summary>
  ///// Test Tittle Constant
        public static String TEST_HEADER_SHORT_DESCRIPTION_CONNECTIVITY = "(Check if internet is connected)";
        public static String TEST_HEADER_SHORT_DESCRIPTION_SPEED_TEST = "(Check your internet connection speed)";
        public static String TEST_HEADER_SHORT_DESCRIPTION_WIFI_INSPECTOR = "(Optimize your Wi-Fi connection)";
        public static String TEST_HEADER_SHORT_DESCRIPTION_ACTIVE_USERS = "(Connected devices in your local network)";
        public static String TEST_HEADER_SHORT_DESCRIPTION_DATA_USAGE = "(Bandwidth used by this device)";
        public static String TEST_HEADER_SHORT_DESCRIPTION_NETWORK_RESPONSE = "(Latency/Packet loss to specific host(s))";
         //////////////////////////// Loging
        public static Boolean LOGGER_OUT_PUT = true;

        ////Speed test active user info text

        public static String MESSAGE_SPEED_TEST_ACTIVE_USER = "Please note that there are other device connected on your  WiFi at the moment. Due to the speed test results may get affected. For more detail, \n run \"Active User\" test";

        //TODO add your app id and secret here ! Crash Reporting
        public static string  SELF_NUMBER = "923445246996";
        public static string HOCKEY_SELF_APP_ID = "477e2c90473944519f00e7ac7390e8f8";
        public static string HOCKEY_SELF_APP_SECRET = "bc88a9476d08d31bb4114f819b0842c8";
        //for self
        public static string HOCKEY_NAYATEL_APP_ID = "42832892b38643a7ab5b814e0e5bf3e5";
        public static string HOCKEY_NAYATEL_APP_SECRET = "26122e80f476a1b3c4ab2a338b26fa1d";
        //
        public static string HOCKEY_APP_ID = PropertyProvider.getPropertyProvider().getPhoneNumberAddress()== SELF_NUMBER ? HOCKEY_SELF_APP_ID: HOCKEY_NAYATEL_APP_ID;
        public static string HOCKEY_APP_SECRET = PropertyProvider.getPropertyProvider().getPhoneNumberAddress() == SELF_NUMBER ? HOCKEY_SELF_APP_SECRET : HOCKEY_NAYATEL_APP_SECRET;
         
        // USER NAME

        public static string USER_EMAIL = PropertyProvider.getPropertyProvider().getEmailAddress();
        public static string USER_Number = PropertyProvider.getPropertyProvider().getPhoneNumberAddress();
        // Google Analytics
        public static string GOOGLE_ANALYITCS_APP_ID = "UA-81026565-1";
        public static string GOOGLE_ANALYITCS_ACCOUNT_ID = "81026565";
        // Your app key - this can be found on the applications page of your DeskMetrics dashboard.
        public static readonly string DMETRICS_SELF_APP_KEY = "3K031abd23";
        public static readonly string DMETRICS_NAYATEL_APP_KEY = "3Qa96884d6";
        public static string DMETRICS_APP_KEY = PropertyProvider.getPropertyProvider().getPhoneNumberAddress() == SELF_NUMBER ? DMETRICS_SELF_APP_KEY : DMETRICS_NAYATEL_APP_KEY;
 
        // Location of properties we stashed in the registry at install time
        // private static readonly string REGISTRY_KEY = @"HKEY_CURRENT_USER\SOFTWARE\HelloDeskMetrics\_dm";

        ///Events
        public static String EVENT_STOP = "Stop Test";
        public static String EVENT_START = "Start Test";
        public static String EVENT_EMAIL = "Email Sent";
        public static String EVENT_UNKNOWN = "Unknown";






    }

}
