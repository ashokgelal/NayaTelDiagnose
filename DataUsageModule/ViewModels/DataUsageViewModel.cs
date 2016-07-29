using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using DataUsageModule.Services;
using DataUsageModule.Model;
using System.Windows;
using System.Windows.Data;
using System.Timers;
using ViewSwitchingNavigation.Infrastructure;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Prism.Events;
using ViewSwitchingNavigation.Infrastructure.Events;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using CoreLibrary;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace DataUsageModule.ViewModels
{
    [Export]
    public class DataUsageViewModel : NDoctorTest, INavigationAware,IDisposable
    {
        protected readonly IEventAggregator eventAggregator;
        Dictionary<String, String> infiDic;
        PropertyProvider.TestType testType;
        private   IDataUsageService dataUsageService;
        private   IRegionManager regionManager;
        private   ObservableCollection<DataUsage> dataUsageCollection;
        private   ObservableCollection<DataUsage> tempDataUsageCollection;

        public ICollectionView DataUsages { get; private set; }
        private System.Timers.Timer aTimer;
        Double totalTime = 0;
        Double timeInterval = 0;
        Double  Interval = 5000;
        long time;
        PropertyProvider.WindowsMethod CurrentTestMethod;
        Boolean isEmail = false;

        [ImportingConstructor]
        public DataUsageViewModel(IDataUsageService dataUsageService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {

            this.dataUsageCollection = new ObservableCollection<DataUsage>();
            this.tempDataUsageCollection = new ObservableCollection<DataUsage>();
            this.DataUsages = new ListCollectionView(this.dataUsageCollection);

            this.dataUsageService = dataUsageService;
            this.regionManager = regionManager;
           
            this.eventAggregator = eventAggregator;
            CurrentTestMethod = PropertyProvider.WindowsMethod.DeviceUsage;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);

        }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod)
            {
                Dispose();
                TestInfo info = new TestInfo();
                info.testMethod = this.CurrentTestMethod;
                info.testType = testType;
                info.sender = this;

                this.eventAggregator.GetEvent<TestStartEvent>().Publish(info);
            }
        }
        private void Initialize()
        {

             
            this.dataUsageService.clearDataUsage();
            this.dataUsageService.GetDataUsageAsync(1);
            Thread.Sleep(1000);
            DataUsage dataUsage = this.dataUsageService.GetDataUsageAsync(1);
            dataUsage.Time = "Current";
            this.dataUsageCollection.Add(dataUsage);
            DataUsage tempDataUsage = new DataUsage();
            tempDataUsage.Download = dataUsage.Download;
            tempDataUsage.Upload = dataUsage.Upload;
            tempDataUsage.Time = dataUsage.Time;
            this.tempDataUsageCollection.Add(tempDataUsage);
            //this.tempDataUsageCollection.Insert(0, dataUsage);
            for (int i = 1; i < (totalTime / Interval) + 1; i++)
            {
                DataUsage dataUsage1 = new DataUsage();
                dataUsage1.Time = ((i * Interval) / 1000).ToString() + " sec ago";
                this.dataUsageCollection.Add(dataUsage1);
            }




            // }));
            // Create a timer with a two second interval.
            time = DateTime.UtcNow.Ticks;
            aTimer = new System.Timers.Timer(Interval);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            timeInterval = aTimer.Interval;
              isEmail = true;

        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {

            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (dataUsageCollection.Count == 0)
                    return;
                DataUsage  dataUsage = this.dataUsageService.GetDataUsageAsync((int)(Interval/1000));
                 dataUsage.Time = this.dataUsageCollection.ElementAt(0).Time;

                this.tempDataUsageCollection.Insert(0, dataUsage);
                // this.dataUsageCollection.RemoveAt(0);

                for (int i = 0; i < tempDataUsageCollection.Count; i++)
                {
                    DataUsage dataUsage1 = this.dataUsageCollection[i];
                    DataUsage tempdataUsage = tempDataUsageCollection[i];
                    String download = tempdataUsage.Download;
                    String upload = tempdataUsage.Upload;
                    dataUsage1.Download = download;
                    dataUsage1.Upload = upload;
                    this.dataUsageCollection.RemoveAt(i);
                    this.dataUsageCollection.Insert(i, dataUsage1);
                   
                }
                if (timeInterval == totalTime || timeInterval > totalTime)
                {
                    if (aTimer != null)
                        aTimer.Stop();

                    TestInfo info = new TestInfo();
                    infiDic = new Dictionary<string, string>();
                    infiDic.Add("time", "23");
                    info.infiDic = infiDic;
                    info.sender = this;
                    info.testMethod = PropertyProvider.WindowsMethod.DeviceUsage;
                    info.testType = testType;
                    this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);
                }
                //TimeSpan duration = new TimeSpan(DateTime.UtcNow.Ticks-time);
                //dataUsage.Time = duration.Seconds.ToString();  
                timeInterval += aTimer.Interval; 


            }));

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
            info.testMethod = this.CurrentTestMethod;
            info.testType = testType;
            info.sender = this;
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
            this.dataUsageService.clearDataUsage();

            if (aTimer != null) {
                aTimer.Close();
                aTimer.Dispose();
                aTimer = null;
            }
             tempDataUsageCollection.Clear();


        }

        public void Dispose()
        {
              cancelTest();
              this.dataUsageCollection.Clear();
              totalTime = 0;
              timeInterval = 0;
              Interval = 5000;
        }

        public override void StopTest()
        {

         }

        public override void StartTest()
        {
            var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(this.CurrentTestMethod, this.testType);
             
            totalTime = tuple.Item1 * 1000;
            if (tuple.Item1 < 5) {
                this.Interval = 2000;
            }else
                this.Interval = 5000;


            Initialize();
        }

        public override Boolean IsEmail()
        {
            return isEmail;

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
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Time") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Download") +
                             Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Upload") +
                              Constants.HTML_END_TR;


            foreach (var item in dataUsageCollection)
            {
               
            }
              
            foreach (var item in dataUsageCollection)
            {

                table += Constants.HTML_START_TR +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Time) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Download) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Upload) +
            Constants.HTML_END_TR;
            }
            table += Constants.HTML_END_TABLE;

            return table;
        }
        public override string getTestTittleHTMLTable()
        {
            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, getTestTittle());
            //user info like mac ip email getUserHTMLTable
            return TittleTable;
        }

        public override string getTestTittle()
        {
            return Constants.TEST_HEADER_TITTLE_DATA_USAGE;
        }
    }
}