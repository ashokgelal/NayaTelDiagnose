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
        public bool testWillStart = false;
        public bool IsInterenetConnectivited = false;
         abstract public void StopTest();
        abstract public void StartTest();
        abstract public Boolean IsEmail();
        abstract public String emailBody();
         abstract public string getTestTittleHTMLTable();
       

 
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
