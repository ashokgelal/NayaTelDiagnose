
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
        List<ActiveUser> newArpListIp = new List<ActiveUser>();
        List<String> ViewIpList  ;

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



            ViewIpList = new List<String>();

            isStop = false;
            
            TestTimeOut = timeout;

            barInvoker = new BackgroundWorker();
            barInvoker.WorkerSupportsCancellation = true;

            barInvoker.RunWorkerCompleted += delegate
            {
 
            };
             barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                this.activeUserService.getUsers(TestTimeOut, this.activeUserFound);

            };

            barInvoker.RunWorkerAsync();

        }
        public void activeUserFound(ActiveUser user,Boolean stop)
        {
            
           
            if (stop)
            {

                TestInfo info = new TestInfo();
                infiDic = new Dictionary<string, string>();
                infiDic.Add("time", "23");
                info.infiDic = infiDic;
                info.sender = this;
                info.testMethod = PropertyProvider.WindowsMethod.DeviceUsage;
                info.testType = testType;
                this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
              
                return;
            }
            user.Mac = String.IsNullOrEmpty(user.Mac) ? "N/A" : user.Mac;
            user.Ip = String.IsNullOrEmpty(user.Ip) ? "N/A" : user.Ip;
            user.HostName = String.IsNullOrEmpty(user.HostName) ? "N/A" : user.HostName;

            if (!isStop && user != null)
            {
                if (ViewIpList.Contains(user.Ip))
                    return;

                try
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() =>
                    {
                        activeUserCollection.Add(user);
                        ViewIpList.Add(user.Ip);
                    }));
                }
                catch (Exception)
                {

                    
                }
               
               // newArpListIp.Remove(user);


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
             
                Dispose();
            
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
            //this.Initialize(6000);

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
                if (item == null)
                    continue;
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
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, getTestTittle());
            return TittleTable;
        }

        public override string getTestTittle()
        {
            return Constants.TEST_HEADER_TITTLE_ACTIVE_USERS;
        }
    }

    
}