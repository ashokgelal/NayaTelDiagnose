

using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;
using ViewSwitchingNavigation.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using NayaTelDiagnose.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Prism.Events;
using ViewSwitchingNavigation.Infrastructure.Utils;
using ViewSwitchingNavigation.Infrastructure.Events;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Prism.Mef.Regions;
using Com.DeskMetrics;
using Prism.Logging;
using ViewSwitchingNavigation.Infrastructure.Log;
using Microsoft.HockeyApp;

namespace NayaTelDiagnose
{
    [Export]
    public partial class Shell : Window, IPartImportsSatisfiedNotification
    {
        private static readonly DeskMetricsClient dma = DeskMetricsClient.Instance;

        private const string DaignoseAllModuleName = "DiagnoseAllModule";
        private static Uri DiagnoseAllViewUri = new Uri("/DiagnoseAllView", UriKind.Relative);
 

        [Import(AllowRecomposition = false)]
        public IModuleManager ModuleManager;


        [Import]
        public IRegionManager regionManager;
        [Import]
        IEventAggregator eventAggregator;
        Boolean subScribeEvent = false;
       // Dictionary<string, String> tittleDictionary = new System.Collections.Generic.Dictionary<string, String>();
        Queue<DaignosePaneControl> qTest = new Queue<DaignosePaneControl>();
        //Courent runing test control like networkresponse ,speedtest
        DaignosePaneControl CurrentControlPanel;
        //its for email check which test was completed  it is for email test base on individual test or diagnose all test
        PropertyProvider.TestType currentTestType;
        PropertyProvider.WindowsMethod currentTestMethod; //curent test runing
        Dictionary<string, String> emailDiagnoseAllBodydic = new System.Collections.Generic.Dictionary<string, String>();
        String errorEmialMesssage;
        // for email 
        NDoctorTest CurrentIndividualTest;
        List<NDoctorTest> CurrentDaignoseAllTest;

        public Shell()
        {
            InitializeComponent();
            //tittleDictionary.Add("/DiagnoseAllView", Constants.TEST_HEADER_TITTLE_DIAGNOSE_ALL);
            //tittleDictionary.Add("/ActivateUsersView", Constants.TEST_HEADER_TITTLE_ACTIVE_USERS);
            //tittleDictionary.Add("/DataUsageView", Constants.TEST_HEADER_TITTLE_DATA_USAGE);
            //tittleDictionary.Add("/NetworkResponseView", Constants.TEST_HEADER_TITTLE_NETWORK_RESPONSE);
            //tittleDictionary.Add("/IntertnetSpeedTestView", Constants.TEST_HEADER_TITTLE_SPEED_TEST);
            //tittleDictionary.Add("/VerifyConnectivityView", Constants.TEST_HEADER_TITTLE_CONNECTIVITY);
            //tittleDictionary.Add("/WifiInspectorView", Constants.TEST_HEADER_TITTLE_WIFI_INSPECTOR);
            //GoogleAnalyticsApi.TrackEvent( "category","action","label");
           Task task = testConectiviteyBeforeStart();

            BackgroundWorker backgroundWorker = new BackgroundWorker();


            backgroundWorker.DoWork += delegate
            {
                
                UtilFiles.deleteAllFillOnFtp();
               

            };
            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Sets the ViewModel.
        /// </summary>
        /// <remarks>
        /// This set-only property is annotated with the <see cref="ImportAttribute"/> so it is injected by MEF with
        /// the appropriate view model.
        /// </remarks>
        [Import]
        [SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Needs to be a property to be composed by MEF")]
        ShellViewModel ViewModel
        {
            set
            {
                this.DataContext = value;

            }


        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (!subScribeEvent)
            {
                this.eventAggregator.GetEvent<TestCompleteEvent>().Subscribe(this.testCompeleted, ThreadOption.UIThread);
                this.eventAggregator.GetEvent<TestStartEvent>().Subscribe(this.testWillStarted, ThreadOption.UIThread);
                this.eventAggregator.GetEvent<DisplayMessageEvent>().Subscribe(this.testMessageEvent, ThreadOption.UIThread);
                // Start sending events.
                // This will trigger the automatic 'launch' event, and a 'heart_beat' event every 12 hours.
                dma.Start(DMASessionType.Application, Constants.DMETRICS_APP_KEY);
                #region HOCKEYAPP SAMPLE CODE

                //main configuration of HockeySDK
                HockeyClient.Current.Configure(Constants.HOCKEY_APP_ID)
                    // .UseCustomResourceManager(NayaTelDiagnose.App.ResourceManager)
                    //register your own resourcemanager to override HockeySDK i18n strings
                    //.RegisterCustomUnhandledExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unhandled exception
                    //.RegisterCustomUnobserveredTaskExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unobserved task exception
                    //.RegisterCustomDispatcherUnhandledExceptionLogic((args) => { }) // define a callback that is called after dispatcher unhandled exception
                    //.SetApiDomain("https://your.hockeyapp.server")
                    .SetContactInfo(Constants.USER_EMAIL, Constants.USER_Number);
                #endregion
                subScribeEvent = true;
            }
            this.ModuleManager.LoadModuleCompleted +=
                (s, e) =>
                {

                    if (e.ModuleInfo.ModuleName == DaignoseAllModuleName)
                    {
                    }
                };
            try
            {

                IRegion mainContentRegion = this.regionManager.Regions[RegionNames.MainContentRegion];
                if (mainContentRegion != null && mainContentRegion.NavigationService != null)
                {
                    mainContentRegion.NavigationService.Navigating += this.MainContentRegion_Navigating;
                    mainContentRegion.NavigationService.Navigated += this.MainContentRegion_Navigated;
                }

            }
            catch (Exception)
            {


            }

        }
        public void NavigationRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {


        }
        public void MainContentRegion_Navigating(object sender, RegionNavigationEventArgs e)
        {
            currentTestType = PropertyProvider.TestType.Individual;
            this.StackDaignoseAll.Visibility = Visibility.Collapsed;
            this.borderBlockMiddleMessage.Visibility = Visibility.Collapsed;

            clearDaignoseAll();
        }
        public void MainContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            

            MefRegionNavigationService service = sender as Prism.Mef.Regions.MefRegionNavigationService;
            FrameworkElement elemaent = service.Region.ActiveViews.First() as FrameworkElement;
            if (typeof(IDisposable).IsAssignableFrom(elemaent.DataContext.GetType()))
                if (currentTestType == PropertyProvider.TestType.Individual && (typeof(NDoctorTest).IsAssignableFrom(elemaent.DataContext.GetType())))
            {
                NDoctorTest NDoctorTest =  elemaent.DataContext  as NDoctorTest;
               textBlockHeaderTitle.Text = NDoctorTest.getTestTittle();
            } 

                //textBlockHeaderTitle.Text = tittleDictionary[e.Uri.ToString()];
            if (EmailButton.Visibility == Visibility.Hidden)
            {
                EmailButton.Visibility = Visibility.Visible;

            }
        }
        void clearDaignoseAll()
        { 
            this.EmailButton.Visibility = Visibility.Collapsed;
            Resfreshbtn.Visibility = Visibility.Collapsed;
            this.imageIndicator.Visibility = Visibility.Collapsed;
            CurrentDaignoseAllTest = new List<NDoctorTest>();
            emailDiagnoseAllBodydic.Clear();
            if (this.StackDaignoseAll.Children.Count > 0)
            {
                qTest.Clear();

                for (int i = 0; i < regionManager.Regions.Count(); i++)
                {

                    if (regionManager.Regions.ElementAt(i).Name.Contains("DaignoseAllContentRegion"))
                    {


                        var views = regionManager.Regions.ElementAt(i).Views;
                        foreach (object item in views)
                        {
                            //regionManager.Regions.ElementAt(i).Remove(item);


                            FrameworkElement elemaent = item as FrameworkElement;
                            if (typeof(IDisposable).IsAssignableFrom(elemaent.DataContext.GetType()))
                            {
                                IDisposable dispose = elemaent.DataContext as IDisposable;
                                dispose.Dispose();
                            }
                        }
                        regionManager.Regions.Remove(regionManager.Regions.ElementAt(i).Name);

                        // IDisposable disposableViewModel = regionManager.Regions.ElementAt(i).ActiveViews.First().DataContext as IDisposable;

                    }
                }

                this.StackDaignoseAll.Children.Clear();
                CurrentControlPanel.PanelHeader.Children.Clear();
                CurrentControlPanel = null;
                this.StackDaignoseAll.Visibility = Visibility.Collapsed;

            }

            this.StackMainContentRegion.Visibility = Visibility.Visible;

        }
        private void NavigateToDaignoseAllRadioButton_Click(object sender, RoutedEventArgs e)
        {

            clearDaignoseAll();
            this.StackMainContentRegion.Visibility = Visibility.Collapsed;
            this.StackDaignoseAll.Visibility = Visibility.Visible;
            IRegion region = regionManager.Regions[RegionNames.MainNavigationRegion];
             textBlockHeaderTitle.Text = Constants.TEST_HEADER_TITTLE_DIAGNOSE_ALL;
            
            foreach (object view in region.Views)
            {
                if (typeof(IDiagnoseAllService).IsAssignableFrom(view.GetType()))
                {
                    IDiagnoseAllService diagnoseAll = view as IDiagnoseAllService;
                    diagnoseAll.getViewURI();
                    DaignosePaneControl control = new DaignosePaneControl();
                    control.textBlockTittle.Text = diagnoseAll.getHeaderTitle();
                    control.navigateImage.Children.Add(diagnoseAll.getImageIcone());
                    control.ViewUri = diagnoseAll.getViewURI();
                    control.textBlockMessage.Text = "";
                    control.ShortTextBlock.Text = diagnoseAll.getShortDescription();
                    qTest.Enqueue(control);
                    this.StackDaignoseAll.Children.Add(control);
                }

            }
            StartDaignoseAll(qTest.Dequeue());
        }

        public void StartDaignoseAll(DaignosePaneControl control)
        {

            CurrentControlPanel = control;
             String RegionName = "DaignoseAllContentRegion" + control.ViewUri;
            control.imageIndicator.Visibility = Visibility.Visible;
            //      < ContentControl Name = "DaignoseAllRegion" prism: RegionManager.RegionName = "DaignoseAllContentRegion" />
            if (regionManager.Regions.ContainsRegionWithName(RegionName))
            {

                regionManager.Regions.Remove(RegionName);

            }
            RegionManager.SetRegionName(control.DaignoseAllRegion, RegionName);
            RegionManager.SetRegionManager(control.DaignoseAllRegion, regionManager);
            RegionManager.UpdateRegions();
            NavigationParameters parm = new NavigationParameters();
            parm.Add(TestInfo.ParmKey, PropertyProvider.TestType.DiagnoseAll);
            this.regionManager.RequestNavigate
        (
        RegionName, control.ViewUri,
         (NavigationResult nr) =>
         {
             var error = nr.Error;
             var result = nr.Result;
             if (nr.Result.Value)
             {
                 control.MouseLeftButtonUp += new MouseButtonEventHandler(this.Panel_MouseLeftButtonUp);
                 Panel_MouseLeftButtonUp(control, null);
             }
             // put a breakpoint here and checkout what NavigationResult contains
         }, parm
        );

        }

        private void Panel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DaignosePaneControl control = (DaignosePaneControl)sender;
            ContentControl content = control.DaignoseAllRegion;
            StackPanel panel = control.PanelHeader;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1000;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));

            if (content.ActualHeight > 0)
            {
                content.Height = 0;
                content.Margin = new Thickness(0);

                control.textBlockMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                content.Height = Double.NaN;
                content.Margin = new Thickness(2);
               if (control.textBlockMessage.Text.Count()>0)
                control.textBlockMessage.Visibility = Visibility.Visible;
             }

            Uri ImageUri = ((content.ActualHeight > 0) ? new Uri("/DiagnoseAll;component/Resources/arrow_down.png", UriKind.Relative) : new Uri("/DiagnoseAll;component/Resources/arrow_up.png", UriKind.Relative));

            control.imageArrow.Source = new BitmapImage(ImageUri);


            //panel.BeginAnimation(ScaleTransform.CenterXProperty, da);
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        private void testCompeleted(TestInfo info)
        {


            CustomLogger.PrintLog("TestCompleted Type:"+info.testType +" testMethod:" + info.testMethod, Category.Info, Priority.Low);


            if (info.testType == PropertyProvider.TestType.Individual && (typeof(NDoctorTest).IsAssignableFrom(info.sender.GetType())))
            {
                CurrentIndividualTest = info.sender as NDoctorTest;
                //sent event analytics
                CurrentIndividualTest.EventSent(NDoctorTest.NDoctorEvent.TestStop, info.testType);
                if (CurrentIndividualTest.IsEmail()) {
                    this.EmailButton.Visibility = Visibility.Visible;
                } else
                    this.EmailButton.Visibility = Visibility.Collapsed;

            }
            if (info.testType == PropertyProvider.TestType.DiagnoseAll && (typeof(NDoctorTest).IsAssignableFrom(info.sender.GetType())))
            {
                NDoctorTest nDoctor = info.sender as NDoctorTest;
                CurrentDaignoseAllTest.Add(nDoctor);
                //sent event analytics
                nDoctor.EventSent(NDoctorTest.NDoctorEvent.TestStop, info.testType);

            }
            imageIndicator.Visibility = Visibility.Collapsed;
             
            if (CurrentControlPanel != null)
            {
                CurrentControlPanel.imageIndicator.Visibility = Visibility.Collapsed;

            }
            if (qTest.Count > 0)
            {
                StartDaignoseAll(qTest.Dequeue());

            }
            else
            {
                currentTestType = info.testType;
                Resfreshbtn.Visibility = Visibility.Visible;
                if (info.testType == PropertyProvider.TestType.DiagnoseAll)
                {
                    this.EmailButton.Visibility = Visibility.Visible;

                }
            }

            }
        private async void testWillStarted(TestInfo info)
        {
            CustomLogger.PrintLog("Test going to Start :Type:" + info.testType + " testMethod:" + info.testMethod , Category.Info, Priority.Low);

            this.currentTestType = info.testType;
            currentTestMethod = info.testMethod;
            Resfreshbtn.Visibility = Visibility.Collapsed;
            this.EmailButton.Visibility = Visibility.Collapsed;
            this.textBlockMiddleMessage.Visibility = Visibility.Collapsed;
            this.borderBlockMiddleMessage.Visibility = Visibility.Collapsed;

            if (info.sender != null)
            {
                if (typeof(NDoctorTest).IsAssignableFrom(info.sender.GetType()))
                {
                    NDoctorTest NDoctorTest = info.sender as NDoctorTest;
                    NDoctorTest.testWillStart = true;

                    NDoctorTest.testWillStart = await testConectiviteyBeforeStart();
                    if (NDoctorTest.testWillStart)
                    {
                        this.imageIndicator.Visibility = Visibility.Visible;
                        if (info.testType == PropertyProvider.TestType.Individual)
                        {
                           
                            this.StackMainContentRegion.Visibility = Visibility.Visible;
                            //Event sent on Dmatrics for individual test
                        }
                        NDoctorTest.StartTest();
                        NDoctorTest.EventSent(NDoctorTest.NDoctorEvent.TestStart, info.testType);

                    }
                    else
                    {
                        this.StackMainContentRegion.Visibility = Visibility.Collapsed;

                        if (info.testType == PropertyProvider.TestType.Individual)
                        {
                            this.imageIndicator.Visibility = Visibility.Collapsed;
                            Resfreshbtn.Visibility = Visibility.Visible;

                        }
                        else if (info.testType == PropertyProvider.TestType.DiagnoseAll)
                        {
                            qTest.Clear();
                            this.imageIndicator.Visibility = Visibility.Collapsed;
                            Resfreshbtn.Visibility = Visibility.Visible;
                        }

                    }

                }
            }

        }
        private void testMessageEvent(DisplayMessage message)
        {

            DisplayMessageOnScreen(message);


        }

        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            


            // EmailSent.sentEmail(getEmailBody("Individual test", emailIndividualTestBody));
            String HTML = "";
            String Subject = "";
 
            if (currentTestType == PropertyProvider.TestType.DiagnoseAll)
            {
                String TittleTable = Constants.HTML_EMAIL_TITTLE.Replace(Constants.HTML_REPLACE_STRING, Constants.TEST_HEADER_TITTLE_DIAGNOSE_ALL);

                HTML += TittleTable+ EmailSent.getUserHTMLTable();
                Subject = Constants.TEST_HEADER_TITTLE_DIAGNOSE_ALL;
                foreach (var item in CurrentDaignoseAllTest)
                {
                    if (item.IsEmail())
                    {
                        HTML += item.getTestTittleHTMLTable()+item.emailBody();
                    }

                }


            }
            else if (CurrentIndividualTest != null && currentTestType == PropertyProvider.TestType.Individual)
            {
                HTML += CurrentIndividualTest .getTestTittleHTMLTable() + EmailSent.getUserHTMLTable();
                HTML += CurrentIndividualTest.emailBody();
                Subject = CurrentIndividualTest.getTestTittle();
            }

            


                BackgroundWorker backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                backgroundWorker.DoWork += delegate {

                    String to = Constants.EMAIL_RECEIVER_SUPPORT;
                    if (!String.IsNullOrEmpty(errorEmialMesssage))
                    {
                        HTML +=  EmailSent.getUserHTMLTable();
                        HTML += errorEmialMesssage;
                        if (errorEmialMesssage == Constants.MESSAGES_DESCRIPTION_NOT_NAYATEL_USER) {
                            to = Constants.EMAIL_RECEIVER_SALE;
                            Subject = Constants.HTML_EMAIL_CLIENT_SUBJECT;
                            HTML = Constants.HTML_EMAIL_CLIENT_BODY;

                        }

                    }


                    Boolean isSent =  EmailSent.sentEmail(Subject, HTML,to);
                    DisplayMessage message = new DisplayMessage();
                    message.messageTittle = Subject;
                    message.displayScren = DisplayMessage.DisplayScreen.Box;
                    
                    if (isSent)
                         message.message = Constants.MESSAGES_EMAIL_SENT; 
                    else
                        message.message = Constants.MESSAGES_EMAIL_FAILED;
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        DisplayMessageOnScreen(message);
                    }));
                };

                backgroundWorker.RunWorkerCompleted += delegate
                {
                    this.imageIndicator.Visibility = Visibility.Collapsed;
                    this.EmailButton.IsEnabled = true;
                    this.Resfreshbtn.Visibility = Visibility.Visible;
                    enableNavigationButton();
                };
                this.imageIndicator.Visibility = Visibility.Visible;
                 this.Resfreshbtn.Visibility = Visibility.Collapsed;

            this.EmailButton.IsEnabled = false;
                disableNavigationButton();
                backgroundWorker.RunWorkerAsync();



             
            }



        private void Resfreshbtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentTestType == PropertyProvider.TestType.Individual)
            {
                this.eventAggregator.GetEvent<TestRestartTestEvent>().Publish(currentTestMethod);

            }
            else
            {
                NavigateToDaignoseAllRadioButton_Click(null, null);

            }

        }

        async Task<bool> testConectiviteyBeforeStart()
        {
           
            this.EmailButton.Visibility = Visibility.Collapsed;
            errorEmialMesssage = null;
            this.imageIndicator.Visibility = Visibility.Visible;
            disableNavigationButton();
            bool validTest = true;
            DisplayMessage message = new DisplayMessage();
            if (!UtilNetwork.isConectedWifi())
            {
                message.message = Constants.MESSAGES_WIFI_NOT_CONNECTED;
                message.messageDescription = Constants.MESSAGES_DESCRIPTION_WIFI_NOT_CONNECTED;
                message.displayScren = DisplayMessage.DisplayScreen.Both;

                validTest = false;
            }
            else
            {

                var tuple = PropertyProvider.getPropertyProvider().getTestTimeout(currentTestMethod, this.currentTestType);
                int timeout = tuple.Item2 * 1000;
                Task<Tuple<String, String, String>> task = HttpRequest.asyncVerifyConectivityTest(Constants.CONECTIVITY_NAYATEL_URL, timeout);
                var item = await task;
                if (!(item.Item1 == "100"))
                {
                    if (item.Item1.Equals("-100")|| item.Item1.Equals("-800"))
                    {
                        message.message = Constants.MESSAGES_NOT_NAYATEL_USER;
                        message.messageDescription = Constants.MESSAGES_DESCRIPTION_NOT_NAYATEL_USER;

                        message.displayScren = DisplayMessage.DisplayScreen.Both;
                        this.EmailButton.Visibility = Visibility.Visible;
                        errorEmialMesssage = Constants.MESSAGES_DESCRIPTION_NOT_NAYATEL_USER;
                        validTest = false;
                    }
                    else
                    {
                        validTest = false;

                        bool isInternet = UtilNetwork.isConnectedInternet(timeout);
                        if (!isInternet)
                        {
                            message.message = Constants.MESSAGES_INTERNET_NOT_CONNECTED;
                            message.messageDescription = Constants.MESSAGES_DESCRIPTION_INTERNET_NOT_CONNECTED;

                            message.displayScren = DisplayMessage.DisplayScreen.Both;
                        }
                        else
                        {
                            this.EmailButton.Visibility = Visibility.Visible;
                            errorEmialMesssage = Constants.MESSAGES_NOT_NAYATEL_USER;
                            message.message = Constants.MESSAGES_NOT_NAYATEL_USER;
                            if (item.Item1.Equals("-700") || item.Item1.Equals("NotFound"))
                            //{
                                message.messageDescription = Constants.MESSAGES_DESCRIPTION_NOT_NAYATEL_NOT_RESPONSE;
                                message.message = Constants.MESSAGES_NOT_NAYATEL_NOT_RESPONSE;

                           // }
                            message.displayScren = DisplayMessage.DisplayScreen.Both;

                        }
                    }


                }
                else if ((item.Item1 == "100"))
                {

                    message.message = Constants.MESSAGES_NAYATEL_USER;
                    message.displayScren = DisplayMessage.DisplayScreen.TOP;
                }
            }
            if (message.message != null)
            {
                DisplayMessageOnScreen(message);

            }

            enableNavigationButton();
            this.imageIndicator.Visibility = Visibility.Collapsed;

            return validTest;

        }
        void enableNavigationButton()
        {
            if (regionManager == null)
                return;
            IRegion region = regionManager.Regions[RegionNames.MainNavigationRegion];
            foreach (object view in region.Views)
            {
                foreach (RadioButton btn in FindVisualChildren<RadioButton>((UserControl)view))
                {
                    btn.IsEnabled = true;
                }
            }
            this.NavigateToDaignoseAllRadioButton.IsEnabled = true;

        }

        void disableNavigationButton()
        {
            if (regionManager == null)
                return;
            IRegion region = regionManager.Regions[RegionNames.MainNavigationRegion];
            foreach (object view in region.Views)
            {
                foreach (RadioButton btn in FindVisualChildren<RadioButton>((UserControl)view))
                {
                    btn.IsEnabled = false;
                }
            }
            this.NavigateToDaignoseAllRadioButton.IsEnabled = false;


        }

        void DisplayMessageOnScreen(DisplayMessage message)
        {

            if (this.currentTestType == PropertyProvider.TestType.DiagnoseAll)
            {
                 if (message.message != Constants.MESSAGES_NAYATEL_USER)
                {
                    if (DisplayMessage.DisplayScreen.Box == message.displayScren)
                    {

                        var dialog = new MessageDialogWindow(message.message, message.messageTittle);
                        // dialog.MesssageText = message.message;
                        if (dialog.ShowDialog() == true)
                        {
                            dialog.Close();
                        }

                    }
                    else {
                        this.textBlockHeaderMessage.Text = "(" + message.message + ")";
                        this.textBlockHeaderMessage.Visibility = Visibility.Visible;

                        CurrentControlPanel.textBlockMessage.Visibility = Visibility.Visible;
                    CurrentControlPanel.textBlockMessage.Text = message.message;
                    CurrentControlPanel.imageIndicator.Visibility = Visibility.Collapsed;


                    }
                       


                }
            }
            else
                switch (message.displayScren)
                {
                    case DisplayMessage.DisplayScreen.TOP:
                        this.textBlockHeaderMessage.Text = "(" + message.message + ")";
                        this.textBlockHeaderMessage.Visibility = Visibility.Visible;
                        break;
                    case DisplayMessage.DisplayScreen.Middle:
                        this.textBlockMiddleMessage.Text = message.messageDescription;
                        this.textBlockMiddleMessage.Visibility = Visibility.Visible;
                        this.borderBlockMiddleMessage.Visibility = Visibility.Visible;
                        break;
                    case DisplayMessage.DisplayScreen.Both:
                        this.textBlockMiddleMessage.Text = message.messageDescription;
                        this.textBlockMiddleMessage.Visibility = Visibility.Visible;
                        this.textBlockHeaderMessage.Text = "(" + message.message + ")";
                        this.textBlockHeaderMessage.Visibility = Visibility.Visible;
                        this.borderBlockMiddleMessage.Visibility = Visibility.Visible;

                        break;
                    case DisplayMessage.DisplayScreen.Box:
                        var dialog = new MessageDialogWindow(message.message,message.messageTittle);
                       // dialog.MesssageText = message.message;
                        if (dialog.ShowDialog() == true)
                        {
                            dialog.Close();
                        }
                        break;
                    default:
                        break;
                }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();


            backgroundWorker.DoWork += delegate
            {
                UtilFiles.deleteDW_FTPFileFromAppData();
                UtilFiles.create5MBFile();
             };
                backgroundWorker.RunWorkerAsync();


                String email = PropertyProvider.getPropertyProvider().getEmailAddress();

            if (email != null)
                return;

           



            var dialog = new UserInputDialog();
            if (dialog.ShowDialog() == true)
            {
                 PropertyProvider.getPropertyProvider().setPhoneAndEmail(dialog.EmailAddressText, dialog.PhoneNumberText);

            }
            else {
                base.OnClosed(e);

                Application.Current.Shutdown();
                

            }
        }

        private void NavigateToDaignoseAllRadioButton_MouseEnter(object sender, MouseEventArgs e)
        {
            NavigateToDaignoseAllRadioButton.Background = (Brush)Application.Current.MainWindow.FindResource("PrimaryBrush_CHECKED_BG");

        }



        private void NavigateToDaignoseAllRadioButton_MouseLeave(object sender, MouseEventArgs e)
        {
            NavigateToDaignoseAllRadioButton.Background = (Brush)Application.Current.MainWindow.FindResource("DiagnoseBrush_BG");

        }
    }
}
