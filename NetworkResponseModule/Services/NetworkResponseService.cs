using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkResponseModule.Model;
using nettools.Core.Commands;
using System.ComponentModel.Composition;
using System.Net;
using System.Timers;
using ViewSwitchingNavigation.Infrastructure;

namespace NetworkResponseModule.Services
{
    [Export(typeof(INetworkResponseService))]

    class NetworkResponseService : INetworkResponseService
    {
        // MTRCommand mtrCommand;
       MTRParallel mtrCommand;
        public void GetNetWorkResponse(string hostip, float hopTimeout ,int timeout, int PingTimout, int max_ttl, int iteration_interval, int iterations_per_host, int packet_size,mtrRoutes mtrRoutes, result result)
        {
            // mtrCommand = new MTRCommand();
           mtrCommand = new MTRParallel();
            //System.Timers.Timer aTimer = new System.Timers.Timer(long.Parse(timeout));
            //// Hook up the Elapsed event for the timer. 
            //aTimer.Elapsed += OnTimedEvent;

            //aTimer.AutoReset = false;
            //aTimer.Enabled = true;

            //Dictionary<string, string> arguments = new Dictionary<string, string>();
            // arguments.Add("timeout", timeout);
            //public bool Execute(string hostOrIpAddress, int timeout, int max_ttl, int iteration_interval, int iterations_per_host, int packet_size, int waiting, mtrRoutes mtrRoutesRes, mtrResult mtr)
            mtrCommand.Execute(hostip,hopTimeout, PingTimout, timeout, max_ttl, iteration_interval, iterations_per_host, packet_size,(IEnumerable<IPAddress> routes1) =>
            {

                mtrRoutes(routes1);


            }, (int loopInternalCount, int loopCount, string host, float Loss,
                      int Recv, int sent, int last,
                        int best, double avg, int worst,int index) =>
            {

                NetWorkResponse mtr = new NetWorkResponse();
                mtr.Count = loopCount.ToString();
                mtr.Avg = avg.ToString();
                mtr.Best = best.ToString();
                mtr.IPAdress = host.ToString();
                mtr.Last = last.ToString();
                mtr.Loss = Loss.ToString();
                mtr.Rec = Recv.ToString();
                mtr.Sent = sent.ToString();
                mtr.Worst = worst.ToString();
                mtr.Index = index;
                result(mtr);


            });

        }

         

      public  void  StopMtr() {
            if (mtrCommand != null)
            {
                mtrCommand.Stop();
                mtrCommand = null;
            }
        }

         

    }
    }
