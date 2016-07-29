using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure;

namespace ViewSwitchingNavigation.Infrastructure.Log
{
   public class CustomLogger : ILoggerFacade
    {
        public   void Log(string message, Category category, Priority priority)
        {
            string messageToLog =
                String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{1}: {2}. Priority: {3}. Timestamp:{0:u}.",
                    DateTime.Now,
                    category.ToString().ToUpperInvariant(),
                    message,
                    priority.ToString());
            if (Constants.LOGGER_OUT_PUT)
            {
              
               // Logger.(message, category.ToString(), (int)priority);
                Debug.WriteLine(messageToLog);
            }
            //MyOtherLoggingFramework.Log(messageToLog);
        }


        public static void PrintLog(string message, Category category, Priority priority, [CallerFilePath] string file = "",
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
             string messageToLog =
                String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{1}: {2}. Priority: {3}. Timestamp:{0:u}. {4}: {5}: {6}:",
                    DateTime.Now,
                    category.ToString().ToUpperInvariant(),
                    message,
                    priority.ToString(), Path.GetFileName(file), member, line);
            if (Constants.LOGGER_OUT_PUT && category == Category.Info)
            {

                // Logger.(message, category.ToString(), (int)priority);
                Debug.WriteLine(messageToLog);
            }

        }

    }
}