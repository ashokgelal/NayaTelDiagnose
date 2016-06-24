
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ActivateUsersModule.Model;
using ActivateUsersModule.Services;
using System.ComponentModel.Composition;
using Prism.Mvvm;
using Prism.Regions;
using System.Runtime.CompilerServices;
using System;
using System.Windows;
using System.Threading;
using ViewSwitchingNavigation.Infrastructure;
using Prism.Events;
using System.Collections.Generic;
using ViewSwitchingNavigation.Infrastructure.Events;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Net;
using System.Diagnostics;

namespace ActivateUsersModule.ViewModels
{
    [Export]
    public class ActiveViewModel : NDoctorTest , INavigationAware,IDisposable
    {
        protected readonly IEventAggregator eventAggregator;
        Dictionary<String, String> infiDic;
        PropertyProvider.TestType testType;
        private readonly IActiveUserService activeUserService;
        private readonly IRegionManager regionManager;
        private   ObservableCollection<ActiveUser> activeUserCollection;
        private ICollectionView _ActiveUsers;//{ get; private set; }
         public ICollectionView ActiveUsers
        {
            get { return _ActiveUsers; }

            set { SetProperty(ref _ActiveUsers, value); }
        }
         bool isStop = false;
        bool isFullScaneIp = false;

        BackgroundWorker barInvoker;
        PropertyProvider.WindowsMethod CurrentTestMethod;
        Dictionary<String, String> DICHostEntery = new Dictionary<string, string>();
        int progress = 0;
        public Stopwatch _Stopwatch;
        int TestTimeOut = 0;
        [ImportingConstructor]
        public ActiveViewModel(IActiveUserService activeUserService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {

            this.activeUserCollection = new ObservableCollection<ActiveUser>();
            this.ActiveUsers = new ListCollectionView(this.activeUserCollection);

            this.activeUserService = activeUserService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            CurrentTestMethod = PropertyProvider.WindowsMethod.ActiveUsers;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);

        }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod)
            {
                Dispose();
                TestInfo info = new TestInfo();
                infiDic = new Dictionary<string, string>();
                infiDic.Add("time", "23");
                info.infiDic = infiDic;
                info.sender = this;
                info.testMethod = this.CurrentTestMethod;
                info.testType = testType;
                this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);
            }
        }
        private void Initialize(int timeout)
        {
             
              isStop = false;
            isFullScaneIp = false;
            DICHostEntery = new Dictionary<string, string>();
            _Stopwatch = new Stopwatch();
            barInvoker = new BackgroundWorker();
            barInvoker.WorkerSupportsCancellation = true;
            barInvoker.WorkerReportsProgress = true;

            barInvoker.ProgressChanged += worker_ProgressChanged;
            barInvoker.DoWork += worker_DoWork;
            barInvoker.RunWorkerCompleted += worker_RunWorkerCompleted;
            barInvoker.RunWorkerAsync( );
            List<ActiveUser> notactiveuser = new List<ActiveUser>() ;

            BackgroundWorker backgroundInvoker = new BackgroundWorker();
            backgroundInvoker.WorkerSupportsCancellation = true;

            backgroundInvoker.DoWork += delegate
            {
                while (isStop != true)
                {
                    Dictionary<string, string> DICHostEnteryCopy = new Dictionary<string, string>(DICHostEntery);
                    foreach (var item in DICHostEnteryCopy)
                    { 
                        if (item.Value != "finding")
                        {
                            Thread.Sleep(500);
                             DICHostEntery[item.Key] = "finding";
                             FindDeviceNameThread(item.Key);
                        }
                    }

                    if (isFullScaneIp )
                    {
                        foreach (var item in DICHostEnteryCopy)
                        {
                            if (item.Value != "")
                            {
                                Thread.Sleep(100);
                                DICHostEntery[item.Key] = "";
                              FindDeviceNameThread(item.Key);
                            }
                        }
                        while ((_Stopwatch.Elapsed.TotalMilliseconds < TestTimeOut))
                        {
                            if (!DICHostEntery.ContainsValue(""))
                            {
                                break;
                            }
                            else {
                               
                                UpdateActiveUserInfo();
                                Thread.Sleep(500);
                            }
                            
                        }
                        UpdateActiveUserInfo();
                        //end test
                        TestInfo info = new TestInfo();
                        infiDic = new Dictionary<string, string>();
                        infiDic.Add("time", "23");
                        info.infiDic = infiDic;
                        info.sender = this;
                        info.testMethod = this.CurrentTestMethod;
                        info.testType = testType;
                        this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
                        cancelTest();
                    }

                    Thread.Sleep(200);


                }
            };
            backgroundInvoker.RunWorkerAsync();


            // ActiveUser user = (ActiveUser)e.Argument;




        }
        public void UpdateActiveUserInfo( )
        {

           
            Dictionary<string, string> DICHostEnteryCopy = new Dictionary<string, string>(DICHostEntery);
            

            for (int i = 0; i < activeUserCollection.Count(); i++)
            {
                if (isStop)
                {
                    break;
                }
                var item = activeUserCollection[i];
                if (DICHostEntery.ContainsKey(item.Ip))
                {
                    item.HostName = DICHostEntery[item.Ip];
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        activeUserCollection.RemoveAt(i);
                        activeUserCollection.Insert(i, item);
                    }));


                }

            }

        }

        async void FindDeviceNameThread(String IpAddress)
        {


            string machineName;
            try
            {
                //IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);
                var hostEntry = await Dns.GetHostEntryAsync(IpAddress);

                ///String hostEntry = Dns.GetHostEntry(ip).HostName;
                machineName = hostEntry.HostName;
                DICHostEntery[IpAddress] = machineName;
            }
            catch (Exception ex)
            {
                machineName = "UNKNOWN";
                DICHostEntery[IpAddress] = machineName;
            }


        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // if (!e.Cancelled) {
            getUserThroughARP();

              
           // }
        }
            

         void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.ActiveUsers, this.testType);
             bool isThreadStart = true;
            TestTimeOut = tuple.Item1 * 1000;
            _Stopwatch.Start();
            this.activeUserService.getUsers(TestTimeOut, ((ActiveUser user) =>
            {
                //notactiveuser.Add(user);
                if (user == null)
                {
                         if (!isStop)
                        (sender as BackgroundWorker).ReportProgress(progress++, user);
                    isThreadStart = false;

                }
                else
                {
                    if(!isStop)
                    (sender as BackgroundWorker).ReportProgress(progress++, user);

                }




            }));

            while (isThreadStart && !isStop)
            {
                Thread.Sleep(100); 
            }
             

        }

          void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // pbCalculationProgress.Value = e.ProgressPercentage;
            if (e.UserState != null)
            {
                ActiveUser user = (ActiveUser)e.UserState;
                if(!DICHostEntery.ContainsKey(user.Ip))
                    DICHostEntery.Add(user.Ip, "finding");
                 
                lock (activeUserCollection)
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        activeUserCollection.Add(user);
                    }));
                }
                 

            }
           // getUserThroughARP();
 

        }
        private delegate IPHostEntry GetHostEntryHandler(string ip);

        public string GetReverseDNS(string ip, int timeout)
        {
            try
            {
                GetHostEntryHandler callback = new GetHostEntryHandler(Dns.GetHostEntry);
                IAsyncResult result = callback.BeginInvoke(ip, null, null);
                if (result.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    return callback.EndInvoke(result).HostName;
                }
                else
                {
                    return ip;
                }
            }
            catch (Exception)
            {
                return ip;
            }
        }
        public async void getUserThroughARP()
        { 
             IEnumerable<ActiveUser> ARP_ActiveUsers = await this.activeUserService.getActiveUsersAsync();
             
            
                List<ActiveUser> newListIp = new List<ActiveUser>();

                 
                foreach (var item in ARP_ActiveUsers)
                {
                    bool isFindIp = false;
                    foreach (var item1 in activeUserCollection)
                    {
                        if (item.Ip == item1.Ip)
                        {
                            isFindIp = true;
                            break;
                        }
                    }
                    if (!isFindIp)
                    {
                        item.Mac = UtilNetwork.GetMacAddress(item.Mac);
                        newListIp.Add(item);
                   
                }
                }


                foreach (var item in newListIp)
                {
                if (!DICHostEntery.ContainsKey(item.Ip))
                    DICHostEntery.Add(item.Ip, "finding");
                if (!String.IsNullOrEmpty(item.Mac))
                {
                    item.Vendor = PropertyProvider.getPropertyProvider().getVendorByMacAddress(item.Mac.Substring(0, 8).Replace(":", ""));

                }
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    activeUserCollection.Add(item);
                }));

            }
            isFullScaneIp = true;

            //verify = await this.verifyConectivityService.getVerifyConectivityAsync(Constants.CONECTIVITY_NAYATEL_URL, timeout);


        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Dispose();

            if (navigationContext.Parameters.Count() > 0)
            {
                Object testType1 = navigationContext.Parameters[TestInfo.ParmKey];
                this.testType = (PropertyProvider.TestType)testType1;
            }
            else
            {
                this.testType = PropertyProvider.TestType.Individual;
            }

            TestInfo info = new TestInfo();
            infiDic = new Dictionary<string, string>();
            infiDic.Add("time", "23");
            info.infiDic = infiDic;
            info.sender = this;
            info.testMethod = this.CurrentTestMethod;
            info.testType = testType;
            this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            BackgroundWorker backgroundInvoker = new BackgroundWorker();
            backgroundInvoker.WorkerSupportsCancellation = true;

            backgroundInvoker.DoWork += delegate
            {
                Dispose();
            };
            backgroundInvoker.RunWorkerAsync();

        }

        void cancelTest() {
            isStop = true;
            isFullScaneIp = false;
            activeUserService.stopTest();

            if (barInvoker != null)
            barInvoker.CancelAsync();
        }
        public void Dispose()
        {
            cancelTest();
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                activeUserCollection.Clear();

            }));
         }

        public override void StopTest()
        {
            Dispose();
         }

        public override void StartTest()
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.ActiveUsers, this.testType);
            this.Initialize(tuple.Item1 * 1000);
        }


        public override Boolean IsEmail()
        {
            return true;

        }

        public override String emailBody()
        {
            return genrateEmailBody();
        }
        String genrateEmailBody()
        {
               //user info like mac ip email getUserHTMLTable
            String table = "" 


                            + Constants.HTML_START_TABLE +

                             Constants.HTML_START_TR +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "IP Address") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "MAC Address") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Device Name") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Device Type") +

                             Constants.HTML_END_TR;

            foreach (var item in activeUserCollection)
            {

                table += Constants.HTML_START_TR +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Ip) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Mac) +
          Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.HostName) +
          Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Vendor) +
            Constants.HTML_END_TR;
            }
            table += Constants.HTML_END_TABLE;

            return table;
        }

        public override string getTestTittleHTMLTable()
        {
            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, "Active Users");
            return TittleTable;
        }
    }

    
}