
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using SpeedTestModule.Model;
using SpeedTestModule.Services;
using Prism.Mvvm;
using Prism.Regions;
using System.ComponentModel.Composition;
using System.Threading;
using System;
using System.Windows;
using ViewSwitchingNavigation.Infrastructure;
using Prism.Events;
using System.Collections.Generic;
using ViewSwitchingNavigation.Infrastructure.Events;
using CoreLibrary;
using System.Net;
using System.Net.NetworkInformation;
using ViewSwitchingNavigation.Infrastructure.Utils;
using System.Diagnostics;

namespace SpeedTestModule.ViewModels
{

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SpeedTestViewModel : NDoctorTest, INavigationAware, IDisposable
    {
        protected readonly IEventAggregator eventAggregator;
        private readonly ISpeedTestService speedTestService;
        private readonly IRegionManager regionManager;
        private ObservableCollection<SpeedTestDownload> speedTestDownloadCollection;
        private ObservableCollection<SpeedTestUpload> speedTestUploadCollection;

        public ICollectionView SpeedTestDownloadCollectionView { get; private set; }
        public ICollectionView SpeedTestUploadCollectionView { get; private set; }
        BackgroundWorker downloadBarInvoker;
        BackgroundWorker UploadInvoker;
        Boolean isStop = false;
        Dictionary<String, String> infiDic;
        PropertyProvider.TestType testType;
        System.Timers.Timer upTimer;
        System.Timers.Timer downTimer;
        PropertyProvider.WindowsMethod CurrentTestMethod;
        Boolean isActiveUserFound = false;
        private bool _ActiveUserMessageBoxHide;
        public bool ActiveUserMessageBoxHide
        {
            get { return _ActiveUserMessageBoxHide; }

            set { SetProperty(ref _ActiveUserMessageBoxHide, value); }
        }

        private String _textBlockMiddleMessage;
        public String textBlockMiddleMessage
        {
            get { return _textBlockMiddleMessage; }

            set { SetProperty(ref _textBlockMiddleMessage, value); }
        }
        
        [ImportingConstructor]
        public SpeedTestViewModel(ISpeedTestService speedTestService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {

            this.speedTestDownloadCollection = new ObservableCollection<SpeedTestDownload>();
            this.speedTestUploadCollection = new ObservableCollection<SpeedTestUpload>();

            this.SpeedTestDownloadCollectionView = new ListCollectionView(this.speedTestDownloadCollection);
            this.SpeedTestUploadCollectionView = new ListCollectionView(this.speedTestUploadCollection);
            this.speedTestService = speedTestService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            CurrentTestMethod = PropertyProvider.WindowsMethod.SpeedTest;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);
            textBlockMiddleMessage = Constants.MESSAGE_SPEED_TEST_ACTIVE_USER;
        }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod)
            {
                Dispose();
                isStop = false;
                TestInfo info = new TestInfo();
                info.sender = this;
                info.testMethod =this.CurrentTestMethod;
                info.testType = testType;
                this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);
            }
        }

        private void Initialize(int timeout)
        {
            ActiveUserOnNetwork NetworkUser = new ActiveUserOnNetwork();
            Dictionary<IPAddress, PhysicalAddress> all = NetworkUser.GetAllDevicesOnLAN();
            IPAddress localIp = IPAddress.Parse(UtilNetwork.GetLocalIPAddress());
            all.Remove(localIp);
            List<IPAddress> GatwayList = UtilNetwork.GetDefualtWayIpAddress();
            foreach (var item in GatwayList)
            {
                all.Remove(item);

            }

            if (all.Count > 0)
            {
                isActiveUserFound = true;
            }
            this.ActiveUserMessageBoxHide = isActiveUserFound;
            downTimer = new System.Timers.Timer(timeout);
            downTimer.Elapsed += (s, e) =>
            {
                downloadBarInvoker.CancelAsync();
                cancelTest();
               updateUploadViews();
            };
            downTimer.AutoReset = false;
            downTimer.Enabled = true;


            downloadBarInvoker = new BackgroundWorker();
            downloadBarInvoker.WorkerSupportsCancellation = true;
            downloadBarInvoker.RunWorkerCompleted += delegate
            {
                
                    //updateUploadViews();
                
            };
                downloadBarInvoker.DoWork += delegate
            {
                isStop = false;
                
                this.speedTestService.downloadSpeedFile((SpeedTestDownload speed) =>
                {

                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                       
                        if (speedTestDownloadCollection.Count > 0)
                        {
                            speedTestDownloadCollection.RemoveAt(0);
                            speedTestDownloadCollection.Add(speed);
                        }
                        else
                        {
                            speedTestDownloadCollection.Add(speed);

                        }
                         
                        if (speed.IsCompleted && !isStop)
                        {
                            if (downTimer != null) {
                                downTimer.Stop();
                                downTimer.Close();
                                downTimer = null;
                                updateUploadViews();
                            }
                            

 

                        }


                    }));

                });

            };
            downloadBarInvoker.RunWorkerAsync();
        }

        public void updateUploadViews()
        {

            TestInfo info = new TestInfo();
            infiDic = new Dictionary<string, string>();
            infiDic.Add("time", "23");
            info.infiDic = infiDic;
            info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.SpeedTest;
            info.testType = testType;

            UploadInvoker = new BackgroundWorker();
            UploadInvoker.WorkerSupportsCancellation = true;

            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(PropertyProvider.WindowsMethod.SpeedTest, PropertyProvider.TestType.Individual);
             upTimer = new System.Timers.Timer(tuple.Item1 * 1000);
             upTimer.Elapsed += (s, e) =>
            {
                cancelTest();
                this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);

            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;
            upTimer.Start();
            UploadInvoker.RunWorkerCompleted += delegate
            {

              };
            UploadInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                isStop = false;

                this.speedTestService.GetUploadSpeed((SpeedTestUpload speed) =>
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                       

                        if (speedTestUploadCollection.Count > 0)
                        {
                            speedTestUploadCollection.RemoveAt(0);
                            speedTestUploadCollection.Add(speed);
                        }
                        else
                        {
                            speedTestUploadCollection.Add(speed);

                        }
                        if (speed.IsCompleted && !isStop) 
                        {
                            if (upTimer != null) {
                                upTimer.Stop();
                                upTimer.Close();
                                upTimer.Dispose();
                            }
                            
                             
                            this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);

                        }
                         

                    }));

                });

            };
            UploadInvoker.RunWorkerAsync();

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

        public void cancelTest()
        {
            isStop = true;
            if (upTimer != null)
            {
                upTimer.Stop();
                upTimer.Close();
                upTimer.Dispose();
            }
            speedTestService.stopSpeedtest();

            if (downloadBarInvoker != null)
            {
                downloadBarInvoker.CancelAsync();


            }
            if (UploadInvoker != null)
            {
                if (UploadInvoker.WorkerSupportsCancellation)
                {
                    UploadInvoker.CancelAsync();
                }
            }


            

           
        }

        public void Dispose()
        {
            cancelTest();
            if (upTimer != null ) {
                upTimer.Stop();
                upTimer.Close();
                upTimer.Dispose();
                upTimer = null;
            }
            if (downTimer != null)
            {
                downTimer.Stop();
                downTimer.Close();
                downTimer.Dispose();
                downTimer = null;
            }

            speedTestDownloadCollection.Clear();
            speedTestUploadCollection.Clear();
        }

        public override void StopTest()
        {
            Dispose();
         }

        public override void StartTest()
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(this.CurrentTestMethod, this.testType);
            Initialize(tuple.Item1 * 1000);
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
              String table = ""

                  
                            +Constants.HTML_START_TABLE +

                             Constants.HTML_START_TR +
                             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Active User") +
                             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, isActiveUserFound?"YES":"NO") +
                             Constants.HTML_END_TR+Constants.HTML_START_TR +
                              Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Upload Speed") +
                             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, speedTestUploadCollection.ElementAt(0).Upload) +

                             Constants.HTML_END_TR + Constants.HTML_START_TR +
                               Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Download Speed") +
                             Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, speedTestDownloadCollection.ElementAt(0).Download) +

                             Constants.HTML_END_TR;

            
                      
            table += Constants.HTML_END_TABLE;

            return table;
        }
        public override string getTestTittleHTMLTable()
        {
            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, "Speed Test");
            //user info like mac ip email getUserHTMLTable

            return TittleTable;
        }
        public override string getTestTittle()
        {
            return Constants.TEST_HEADER_TITTLE_SPEED_TEST;
        }
    }
}