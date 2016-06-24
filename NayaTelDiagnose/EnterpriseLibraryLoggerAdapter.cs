

using Prism.Logging;
 
namespace NayaTelDiagnose
{
    public class EnterpriseLibraryLoggerAdapter : ILoggerFacade
    {
        public EnterpriseLibraryLoggerAdapter()
        {
            //Logger.SetLogWriter(new LogWriter(new LoggingConfiguration()));
        }

        #region ILoggerFacade Members

        public void Log(string message, Category category, Priority priority)
        {
            //Logger.Write(message, category.ToString(), (int)priority);
        }

        #endregion
    }
}
