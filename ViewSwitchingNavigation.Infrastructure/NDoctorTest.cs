using Com.DeskMetrics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace ViewSwitchingNavigation.Infrastructure
{
    public abstract class NDoctorTest : BindableBase
    {
        private static readonly DeskMetricsClient dma = DeskMetricsClient.Instance;

      public  enum NDoctorEvent
        {
            TestStart,
            TestStop,
            EmailSent,
        }

        public bool testWillStart = false;
        public bool IsInterenetConnectivited = false;
        abstract public void StopTest();
        abstract public void StartTest();
        abstract public Boolean IsEmail();
        abstract public String emailBody();
        abstract public string getTestTittleHTMLTable();
        abstract public string getTestTittle();
        public void EventSent(NDoctorEvent Event ,PropertyProvider.TestType TestType)
        {
            String EventType = Constants.EVENT_UNKNOWN;
            switch (Event)
            {

                case NDoctorEvent.TestStart:
                    EventType = Constants.EVENT_START;
                    break;
                case NDoctorEvent.TestStop:
                    EventType = Constants.EVENT_STOP;

                    break;
                case NDoctorEvent.EmailSent:
                    EventType = Constants.EVENT_EMAIL;

                    break;
                default:
                    break;
            }
            String strTestType = TestType.ToString();
            Object ob = new {TestType = strTestType, EventType = EventType, Email = PropertyProvider.getPropertyProvider().getEmailAddress(), PhoneNumber = PropertyProvider.getPropertyProvider().getPhoneNumberAddress(), MACAddress = UtilNetwork.GetLocalMacAddress() };
            dma.Send(getTestTittle(), ob);

        }

        public void EventSent(NDoctorEvent Event) {

            String EventType = Constants.EVENT_UNKNOWN;
            switch (Event)
            {

                case NDoctorEvent.TestStart:
                    EventType = Constants.EVENT_START;
                    break;
                case NDoctorEvent.TestStop:
                    EventType = Constants.EVENT_STOP;

                    break;
                case NDoctorEvent.EmailSent:
                    EventType = Constants.EVENT_EMAIL;

                    break;
                default:
                    break;
            }

            Object ob = new {  EventType = EventType, Email = PropertyProvider.getPropertyProvider().getEmailAddress(), PhoneNumber = PropertyProvider.getPropertyProvider().getPhoneNumberAddress(), MACAddress = UtilNetwork.GetLocalMacAddress() };
            dma.Send(getTestTittle(), ob);


        }


    }

    public abstract class BindableBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));

            }
        }
    }
}
