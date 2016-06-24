using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedTestModule.Model;

namespace SpeedTestModule.Services
{
    public delegate void DownloadSpeedResult(SpeedTestDownload speed);
    public delegate void UploadSpeedResult(SpeedTestUpload speed);

    public interface ISpeedTestService
    {
        void downloadSpeedFile(DownloadSpeedResult result);
        void GetUploadSpeed(UploadSpeedResult result);

        void stopSpeedtest();

    }
}
