using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataUsageModule.Model;

namespace DataUsageModule.Services
{
    public interface IDataUsageService
    {
        DataUsage GetDataUsageAsync(int seconds);
        void clearDataUsage();
    }
        
}
