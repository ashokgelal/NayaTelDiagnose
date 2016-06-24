using System;
using System.Collections.Generic;
using System.Linq;

using NSpeedTest;
using nettools.ThirdParty.NSpeedTest.Models;

namespace nettools.Core.Commands
{

    internal class SpeedTestCommand : ICommand
    {

        private static SpeedTestClient client;
        private static Settings settings;
        private string DefaultCountry = "Austria";

        private string formatter = " {0,-7} | {1,-40} | {2,-30} | {3,-9} | {4,-9}";

        public bool Execute(Dictionary<string, string> arguments)
        {
            DefaultCountry = ArgumentParser.StripArgument(arguments, "Austria", 0, "c", "country");
            int index = ArgumentParser.StripArgument(arguments, -1, "i", "id", "index");
            bool list = ArgumentParser.StripArgument(arguments, false, "l", "list");

            Console.WriteLine("Getting speedtest.net settings and server list...");
            client = new SpeedTestClient();
            settings = client.GetSettings();

            if (list)
            {
                List<string> keywords = new List<string>();

                Console.Write("Enter some keywords (splitted by \";\") or leave blank: ");
                string[] words = Console.ReadLine().Split(';');

                foreach (string word in words)
                {
                    string tmp = word.Trim(' ');

                    if(!string.IsNullOrWhiteSpace(tmp))
                        keywords.Add(tmp.ToLower());
                }

                Console.WriteLine("\n" + formatter, "ID", "Hosted by", "Location", "Distance", "Latency");
                Console.WriteLine(formatter, new string('=', 7), new string('=', 40), new string('=', 30), new string('=', 9), new string('=', 9));

                foreach (Server server in settings.Servers.Where(t => keywords.Count != 0 && (
                        ((t.Host != null) ? keywords.Contains(t.Country.ToLower()) : false) || ((t.Host != null) ? keywords.Contains(t.Host.ToLower()) : false) ||
                        ((t.Host != null) ? keywords.Contains(t.Name.ToLower()) : false) || ((t.Host != null) ? keywords.Contains(t.Sponsor.ToLower()) : false)
                    )))
                {
                    PrintServerDetails(server);
                }

                return true;
            }

            Server bestServer = null;

            if (index == -1)
            {
                IEnumerable<Server> servers = SelectServers();
                bestServer = SelectBestServer(servers);
            }
            else
            {
                bestServer = settings.Servers.Where(t => t.Id == index).FirstOrDefault();
                if (bestServer == null)
                {
                    Console.WriteLine("\n\tNo server with this ID was found...");
                    return false;
                }
                else
                {
                    Console.WriteLine("\nServer selected by user:\n");

                    Console.WriteLine(formatter, "ID", "Hosted by", "Location", "Distance", "Latency");
                    Console.WriteLine(formatter, new string('=', 7), new string('=', 40), new string('=', 30), new string('=', 9), new string('=', 9));
                    PrintServerDetails(bestServer);

                    Console.Write("\n");
                }
            }

            Console.WriteLine("Testing speed...");
            double downloadSpeed = client.TestDownloadSpeed(bestServer, settings.Download.ThreadsPerUrl);
            PrintSpeed("Download", downloadSpeed);
            double uploadSpeed = client.TestUploadSpeed(bestServer, settings.Upload.ThreadsPerUrl);
            PrintSpeed("Upload", uploadSpeed);
            
            return true;
        }

        /*
         * SelectBestServer(), SelectServers(), PrintServerDetails() and PrintSpeed()
         * originally by https://github.com/Kwull/NSpeedTest
         */
        private Server SelectBestServer(IEnumerable<Server> servers)
        {
            Console.WriteLine("\nBest server by latency:\n");
            var bestServer = servers.OrderBy(x => x.Latency).First();
            PrintServerDetails(bestServer);
            Console.Write("\n");
            return bestServer;
        }
        
        private IEnumerable<Server> SelectServers()
        {
            Console.WriteLine("\nSelecting best server by distance...");
            IEnumerable<Server> servers = settings.Servers.Where(s => s.Country.Equals(DefaultCountry)).Take(10).ToList();

            Console.WriteLine(formatter, "ID", "Hosted by", "Location", "Distance", "Latency");
            Console.WriteLine(formatter, new string('=', 7), new string('=', 40), new string('=', 30), new string('=', 9), new string('=', 9));

            foreach (Server server in servers)
            {
                server.Latency = client.TestServerLatency(server);
                PrintServerDetails(server);
            }

            return servers;
        }
        
        private void PrintServerDetails(Server server)
        {
            Console.WriteLine(formatter, server.Id, server.Sponsor, server.Name, server.Country, (int)server.Distance / 1000, server.Latency);

            /* Console.WriteLine("#{0}, Hosted by {1} ({2}/{3}), distance: {4}km, latency: {5}ms", server.Id, server.Sponsor, server.Name,
                server.Country, (int)server.Distance / 1000, server.Latency); */
        }
        
        private void PrintSpeed(string type, double speed)
        {
            if (speed > (1024 * 1024 * 1024))
            {
                Console.WriteLine("{0} speed: {1} Tbps", type, Math.Round(speed / (1024 * 1024 * 1024), 2));
            }
            else if (speed > (1024 * 1024))
            {
                Console.WriteLine("{0} speed: {1} Gbps", type, Math.Round(speed / (1024 * 1024), 2));
            }
            else if (speed > 1024)
            {
                Console.WriteLine("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                Console.WriteLine("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }

        public string[] GetAvailableCommands()
        {
            return new string[] { "speedtest", "spdtst" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "c*0", "country|c", "i", "id", "index", "l", "list|l" };
        }

        public string GetHelp()
        {
            return "Test the speed of your network connection with SpeedTest.net";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "c")
                return "The country in which the server should run";
            else if (arg == "i")
                return "The ID of a server which should be used to test the speed";
            else if (arg == "l")
                return "List all servers";
            else
                return null;
        }

        public void Stop() { }

    }

}
