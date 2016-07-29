using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ViewSwitchingNavigation.Infrastructure;
using WifiInspectorModule.Services;
using WifiInspectorModule.Model;
using System.Collections.Generic;
using Prism.Events;
using ViewSwitchingNavigation.Infrastructure.Events;
using System.Web.UI;
using System.IO;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace WifiInspectorModule.ViewModels
{
    [Export]
    public class WifiInfoViewModel : NDoctorTest, INavigationAware, IDisposable
    {

        private readonly IWifiInfoService wiFiInfoService;
        private readonly IRegionManager regionManager;

        private readonly ObservableCollection<WifiInfo> messagesCollection;
        private readonly ObservableCollection<WifiInfo> nWiFiInspectorCollection;

        public ICollectionView Messages { get; private set; }
        public ICollectionView nWiFiInspector { get; private set; }

        protected readonly IEventAggregator eventAggregator;
        Dictionary<String, String> infiDic;
        PropertyProvider.TestType testType;
        System.Timers.Timer upTimer;
        Boolean isStop = false;
        Boolean isEmail = false;
      


        PropertyProvider.WindowsMethod CurrentTestMethod;

        [ImportingConstructor]
        public WifiInfoViewModel(IWifiInfoService wiFiInfoService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {

            this.messagesCollection = new ObservableCollection<WifiInfo>();
            this.Messages = new ListCollectionView(this.messagesCollection);

            this.nWiFiInspectorCollection = new ObservableCollection<WifiInfo>();
            this.nWiFiInspector = new ListCollectionView(this.nWiFiInspectorCollection);

            this.wiFiInfoService = wiFiInfoService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            CurrentTestMethod = PropertyProvider.WindowsMethod.WifiInspector;
            this.eventAggregator.GetEvent<TestRestartTestEvent>().Subscribe(this.restartTest, ThreadOption.UIThread);

        }
        private void restartTest(PropertyProvider.WindowsMethod currentTest)
        {
            if (currentTest == CurrentTestMethod)
            {
                Dispose();
                testStartEvent();
            }
        }

        private async Task Initialize(int timeout)
        {
            bool isStop = false;
            TestInfo info = new TestInfo();
            infiDic = new Dictionary<string, string>();
            infiDic.Add("time", "23");
            info.sender = this;
            info.testMethod = PropertyProvider.WindowsMethod.WifiInspector;
            info.testType = testType;
 
            

            //Task<bool> taskIstestwillStart = isTestWillStart();
            // var TestWillStart = await taskIstestwillStart;

            // if (!TestWillStart) {
            //    return;
            //}



            upTimer = new System.Timers.Timer(timeout);
            // Hook up the Elapsed event for the timer. 
            upTimer.Elapsed += (s, e) =>
            {
                cancelTest();
            };
            upTimer.AutoReset = false;
            upTimer.Enabled = true;


            var messages = await this.wiFiInfoService.GetWifiesAsync();
            if (!isStop)
            {
                String tMacBSSID = "";
                foreach (var item in messages)
                {
                    String NA = "N/A";
                    item.BSS = String.IsNullOrEmpty(item.BSS) ? NA : item.BSS;
                    item.SSID = String.IsNullOrEmpty(item.SSID) ? NA : item.SSID;
                    item.Channel = String.IsNullOrEmpty(item.Channel) ? NA : item.Channel;
                    item.Signal = String.IsNullOrEmpty(item.Signal) ? NA : item.Signal;
                    item.Vendor = String.IsNullOrEmpty(item.Vendor) ? NA : item.Vendor;
                    item.RSSID = String.IsNullOrEmpty(item.RSSID) ? NA : item.RSSID;
                    item.Security = String.IsNullOrEmpty(item.Security) ? NA : item.Security;
                    item.Speed = String.IsNullOrEmpty(item.Speed) ? NA : item.Speed;
                    item.Frequency = String.IsNullOrEmpty(item.Frequency) ? NA : item.Frequency;
                    
 

                    if (item.IPv4 != null)
                    {
                        item.DefualtGateway = String.IsNullOrEmpty(item.DefualtGateway) ? NA : item.DefualtGateway;
                        item.DefualtGatewayMac = String.IsNullOrEmpty(item.DefualtGatewayMac) ? NA : item.DefualtGatewayMac;
                        item.IPv4 = String.IsNullOrEmpty(item.IPv4) ? NA : item.IPv4;
                        item.MAC = String.IsNullOrEmpty(item.MAC) ? NA : item.MAC;
                        item.IPv6 = String.IsNullOrEmpty(item.IPv6) ? NA : item.IPv6;
                        item.GHZ4 = String.IsNullOrEmpty(item.GHZ4) ? NA : item.GHZ4;
                        item.GHZ5 = String.IsNullOrEmpty(item.GHZ5) ? NA : item.GHZ5;
                        item.OverlappingAPS = String.IsNullOrEmpty(item.OverlappingAPS) ? NA : item.OverlappingAPS;


                        this.messagesCollection.Add(item);
                        tMacBSSID = item.BSS;

                    }
                    else if (item.BSS != tMacBSSID)
                    {
                        this.nWiFiInspectorCollection.Add(item);

                    }
                }

                //messages.ToList().ForEach(m => this.messagesCollection.Add(m));
                infiDic.Add("email", "");
                info.infiDic = infiDic;
                if (messagesCollection.Count>0)
                {
                    this.isEmail = true;
                }
                this.eventAggregator.GetEvent<TestCompleteEvent>().Publish(info);

            }
            else
            {
                info.error = "failed test";
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
            else
            {
                this.testType = PropertyProvider.TestType.Individual;
            }
            testStartEvent();
        }
        public void testStartEvent() {
            TestInfo info = new TestInfo();
            info.sender = this;
            info.testMethod = CurrentTestMethod;
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
                upTimer.Close();
                upTimer.Dispose();
            }


        }

        public void Dispose()
        {
            this.testWillStart = false;
            cancelTest();
            messagesCollection.Clear();
            nWiFiInspectorCollection.Clear();
        }
      
        public override void StopTest()
        {
            Dispose();
         }

        public override void StartTest()
        {
            if (this.testWillStart) {
                var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(CurrentTestMethod, this.testType);
                Task task = Initialize(tuple.Item1 * 1000);
            }
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


             
            String table = ""+ Constants.HTML_START_TABLE;
            WifiInfo ConnWifi = new WifiInfo();
            foreach (var item in messagesCollection)
            {

                table +=
                //item.SSID, item.BSS, item.DefualtGateway, item.DefualtGatewayMac, item.GHZ4, item.GHZ5
                Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "SSID") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.SSID) +
                Constants.HTML_END_TR +

                Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "BSSID") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.BSS) +
                Constants.HTML_END_TR +

                Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "SSID Channel") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Channel) +
                Constants.HTML_END_TR +
                 Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Default Gateway") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.DefualtGateway) +
                Constants.HTML_END_TR +

                 Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Default Gateway MAC Address") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.DefualtGatewayMac) +
                Constants.HTML_END_TR +

                 Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Signal strenght") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Signal) +
                Constants.HTML_END_TR +

                Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Overlapping Aps") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.OverlappingAPS) +
                Constants.HTML_END_TR +


                 Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Channel with Least Congestion on 2.4Ghz") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.GHZ4) +
                Constants.HTML_END_TR +

                 Constants.HTML_START_TR +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Channel with Least Congestion on 5 Ghz") +
                Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.GHZ5) +
                Constants.HTML_END_TR;


                ConnWifi = item;
                    }

            table += Constants.HTML_END_TABLE + Constants.HTML_START_TABLE +

               Constants.HTML_START_TR +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "SSID") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "BSSID") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Vendor") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Signal Strength") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Channel") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "RSSI") +
               Constants.HTML_TH.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_HEADER_COLOR).Replace(Constants.HTML_REPLACE_STRING, "Security") +
                Constants.HTML_END_TR;


            //connected wifi info adding 

            //connected wifi info adding  
            foreach (var item in messagesCollection)
            {

                table += Constants.HTML_START_TR +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.SSID) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.BSS) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Vendor) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Signal) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Channel) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.RSSID) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Security) +
            Constants.HTML_END_TR;
            }


            foreach (var item in nWiFiInspectorCollection)
            {

                table += Constants.HTML_START_TR +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.SSID) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.BSS) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Vendor) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Signal) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Channel) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_SECONDERY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.RSSID) +
            Constants.HTML_TD.Replace(Constants.HTML_REPLACE_STRING_COLOR, Constants.HTML_PRIMARY_COLOR).Replace(Constants.HTML_REPLACE_STRING, item.Security) +
            Constants.HTML_END_TR;
            }
            table += Constants.HTML_END_TABLE;

            return table;
        }
        public override string getTestTittleHTMLTable()
        {
            //Tittle header
            String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, "Wifi Inspector");
            //user info like mac ip email getUserHTMLTable

            return TittleTable;
        }
        public override string getTestTittle()
        {
            return Constants.TEST_HEADER_TITTLE_WIFI_INSPECTOR;
        }
    }

}
 