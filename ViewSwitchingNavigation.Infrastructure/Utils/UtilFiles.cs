using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Utils
{
    public class UtilFiles
    {
        static String tempFolder = "NayatelNDoctor";

        public static string GetTemporaryDirectory()
        {
            string tempDir = Path.GetTempPath()+tempFolder;
             Directory.CreateDirectory(tempDir);

            return tempDir + Path.DirectorySeparatorChar;
        }
         
    }
}
