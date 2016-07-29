using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkResponseModule.Model;
using System.Net;

namespace NetworkResponseModule.Services
{
    public delegate void result(NetWorkResponse user);
    public delegate void mtrRoutes(IEnumerable<IPAddress> routes);

    public interface INetworkResponseService
    {
        void GetNetWorkResponse(string hostip, float hopTimeout,int timeout,  int PingTimout ,int max_ttl, int iteration_interval, int iterations_per_host, int packet_size, mtrRoutes mtrRoutes, result result);
        void StopMtr();

    }
}
