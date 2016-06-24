
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using VerifyConnectivityModule.Model;
using VerifyConnectivityModule.Services;
using ViewSwitchingNavigation.Infrastructure;
using System;
using System.Windows.Media.Imaging;
using Prism.Events;
using System.Collections.Generic;
using ViewSwitchingNavigation.Infrastructure.Events;
using System.IO;
using System.Web.UI;

namespace VerifyConnectivityModule.ViewModels
{
    [Export] 
    public class VerifyConectivityViewModel : NDoctorTest, INavigationAware  ,IDisposable
    {
 
        private readonly IVerifyConectivityService verifyConectivityService;
        private readonly IRegionManager regionManager;
        private   ObservableCollection<VerifyConectivity> verifyConectivityCollection;
        public ICollectionView VerifyConectivityView { get; private set; }
        protected readonly IEventAggregator  eventAggregator;
        Dictionary<String, String> infiDic;
        PropertyProvider.TestType testType;
        PropertyProvider.WindowsMethod CurrentTestMethod;

        System.Timers.Timer upTimer;
        bool isStopTest = false;
        Boolean isEmail = false;

        [ImportingConstructor]
        public VerifyConectivityViewModel(IVerifyConectivityService verifyConectivityService, IRegionManager regionManager , IEventAggregator eventAggregator)
        {
             
            this.verifyConectivityCollection = new ObservableCollection<VerifyConectivity>();
            this.VerifyConectivityView = new ListCollectionView(this.verifyConectivityCollection);

            this.verifyConectivityService = verifyConectivityService;
            this.regionManager = regionManager;
            //var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.VerifyConnectivity, PropertyProvider.TestType.Individual);
            this.eventAggregator = eventAggregator;
            CurrentTestMethod = PropertyProvider.WindowsMethod.VerifyConnectivity;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);

        }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod) {
                Dispose();
                TestInfo info = new TestInfo();
                info.sender = this;
                info.testMethod = currentTest;
                info.testType = testType;
                this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);
             }
                
        }

        private async Task Initialize(int timeout)
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.VerifyConnectivity, PropertyProvider.TestType.Individual);
              isStopTest = false;

            VerifyConectivity verify = null ;
            


            verify = await this.verifyConectivityService.getVerifyConectivityAsync(Constants.CONECTIVITY_NAYATEL_URL, timeout);
            isEmail = true;
            TestInfo info = new TestInfo();
            infiDic = new Dictionary<string, string>();
            info.infiDic = infiDic;
            infiDic.Add("time", "23");
            info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.VerifyConnectivity;
            info.testType = testType;
            if (verify.status.Equals("100"))
            {
                this.verifyConectivityCollection.Add(verify);
                infiDic.Add("email", getEmailBody());
                this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);

            }
            else {
                if (verify.status == "-700")
                {
                    //timeout
                    info.error = "Request time out";


                }
                else {
                    info.error = "Error"+ verify.status;

                }
                this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);

            }


        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Dispose();
            if (navigationContext.Parameters.Count() > 0)
            {
                Object testType1 = navigationContext.Parameters[TestInfo.ParmKey];
                this.testType = (PropertyProvider.TestType)testType1;
            }
            else {
                this.testType = PropertyProvider.TestType.Individual;
            }
            TestInfo info = new TestInfo();
             info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.VerifyConnectivity;
            info.testType = testType;
            this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);

            
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Dispose();
        }
        void cancelTest() {
            isStopTest = true;
            this.verifyConectivityCollection.Clear();
        }
        public void Dispose()
        {
            cancelTest();
         }

       String getEmailBody()
        {
            StringWriter stringWriter = new StringWriter();


        String body  =  String.Format(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN""      ""http://www.w3.org/TR/html4/strict.dtd"">
        <html>
            <title>{0}</title>
        <link rel=""stylesheet"" type=""text/css"" href=""style.css"">
        </head>
        <body>
< table style = ""width: 70 % "" cellpadding = ""7"" align = ""center"" >
    <tr height=""40"">
                    <th colspan=""2"" align=""left"" bgcolor=""#2b66b2""><font color=""#fff"" height=""100"">Wifi Inspector</font></th>
                </tr>                <td bgcolor=""#eaeaea"">Email Address:</td>
                    <td bgcolor=""#f4f4f4"">abc</td>
                </tr>

                <tr>
                    <td bgcolor=""#eaeaea"">Mobile Number:</td>
                    <td bgcolor=""#f4f4f4"">abc</td>
					<td bgcolor=""#f4f4f4"">abc</td>
					<td bgcolor=""#f4f4f4"">abc</td>
					<td bgcolor=""#f4f4f4"">abc</td>
					<td bgcolor=""#f4f4f4"">abc</td>
                </tr>

 <tr>
        <th align=""left"">Regards,</th>
    </tr>
    <tr>
        <th align=""left"">NAYAtel nDoctor</th>
    </tr>
</table>
</body></html>
", "");






            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                // Loop over some strings.
                foreach (var verifyConectivity in verifyConectivityCollection)
                {
                   // writer.Write();
                    // Some strings for the attributes.
                    string tr_height = "70";
                    string colspan = "2";
                    string align = "left";
                    string bgcolor = "#2b66b2";
                    string font_color = "#fff";
                    string th_height = "100";

 
                               // The important part:
                             //  writer.AddAttribute(HtmlTextWriterAttribute.Height, tr_height);
                  //  writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Begin #1

                   // writer.AddAttribute(HtmlTextWriterAttribute.Href, urlValue);
                   // writer.RenderBeginTag(HtmlTextWriterTag.A); // Begin #2


                }
            }

                    return body;
        }

        public override void StopTest()
        {
            Dispose();

        }

        public override void StartTest()
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(this.CurrentTestMethod, this.testType);
            Task task = Initialize(tuple.Item1 * 1000);

        }


        public override Boolean IsEmail()
        {
            if (testType == PropertyProvider.TestType.DiagnoseAll)
                return false;
            return true; ;

        }

        public override String emailBody()
        {
            return genrateEmailBody();
        }
        String genrateEmailBody()
        {


           
            String table = "";

            return table;
        }
        public override string getTestTittleHTMLTable()
        {
            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, "Conectivity");
            //user info like mac ip email getUserHTMLTable

            return TittleTable;
        }
    }
}