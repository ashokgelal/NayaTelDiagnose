using NativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyConnectivityModule.Model;
using ViewSwitchingNavigation.Infrastructure;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace VerifyConnectivityModule.Services
{
    [Export(typeof(IVerifyConectivityService))]
    class VerifyConectivityService : IVerifyConectivityService
    {
        private async Task<VerifyConectivity> getVerifyConectivity(String url,double timeout)
        {
 
            Task<Tuple<String, String, String>> task = HttpRequest.asyncVerifyConectivityTest(url, timeout);
            var  item = await task;
            VerifyConectivity verify = new VerifyConectivity();
            verify.LocalIP = UtilNetwork.GetLocalIPAddress();
            //String status = "-1";
            //String wan_ip = "";
            //String msg = "";
            if (item.Item1.Equals("100"))
            {
                verify.WanIP = item.Item2;
                verify.MSG = item.Item3;
                verify.status = item.Item1;

            }
            else {
                verify.status = item.Item1;
            }
             
            return verify;
        }

        public Task<VerifyConectivity> getVerifyConectivityAsync(String url, double timeout)
        {
            return getVerifyConectivity(url, timeout);
        }

    }
}
