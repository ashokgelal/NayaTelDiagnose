using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyConnectivityModule.Model;

namespace VerifyConnectivityModule.Services
{
 
    public interface IVerifyConectivityService
    {
        Task<VerifyConectivity> getVerifyConectivityAsync(String url, double timeout);

    }
}
