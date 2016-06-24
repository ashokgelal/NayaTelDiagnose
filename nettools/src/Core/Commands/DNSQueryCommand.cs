using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using nettools.ThirdParty.Bdev.Net.Dns;
using System.Diagnostics;

namespace nettools.Core.Commands
{

    internal class DNSQueryCommand : ICommand
    {

        public bool Execute(Dictionary<string, string> arguments)
        {
            string hostname = ArgumentParser.StripArgument(arguments, "", 0, "h", "host", "hostname", "d", "domain");
            string dnsserver = ArgumentParser.StripArgument(arguments, "8.8.8.8", "ds", "dns", "dnssrv", "dnsserver");

            if (string.IsNullOrEmpty(hostname) || string.IsNullOrWhiteSpace(hostname))
            {
                Console.WriteLine("Please enter a (valid) hostname or an ip-address");
                return false;
            }

            if (string.IsNullOrEmpty(dnsserver) || string.IsNullOrWhiteSpace(dnsserver))
            {
                Console.WriteLine("Please enter a (valid) DNS-Server");
                return false;
            }

            IPAddress dns_root = IPAddress.Parse(dnsserver);
            Stopwatch timer = new Stopwatch();

            Console.WriteLine("Starting querying records from " + dns_root.ToString() + "...");
            timer.Start();

            /* foreach (DnsType dnsType in new DnsType[] { DnsType.ANAME, DnsType.MX, DnsType.NS, DnsType.SOA })
            {
                responses.Add(QueryDNSServer(dns_root, DnsClass.IN, dnsType, hostname));
            } */

            List<Response> responses = new List<Response>();

            responses.Add(QueryDNSServer(dns_root, DnsClass.IN, DnsType.ANAME, hostname));
            responses.Add(QueryDNSServer(dns_root, DnsClass.IN, DnsType.NS, hostname));
            responses.Add(QueryDNSServer(dns_root, DnsClass.IN, DnsType.TXT, hostname));
            responses.Add(QueryDNSServer(dns_root, DnsClass.IN, DnsType.MX, hostname));
            responses.Add(QueryDNSServer(dns_root, DnsClass.IN, DnsType.SOA, hostname));
            
            timer.Stop();
            Console.WriteLine("Finished after " + timer.ElapsedMilliseconds + "ms!\n");
            timer = null;
            
            string formatter = " {0,-30} | {1,-7} | {2,-3} | {3,-6} | {4,-60}";
            string noTableFormatter = " {0,-30}   {1,-7}   {2,-3}   {3,-6}   {4,-60}";

            int queryCount = 0;
            int answerCount = 0;
            int addCount = 0;
            int authCount = 0;

            responses.Any(t => (queryCount += t.Questions.Length) == -1);
            responses.Any(t => (answerCount += t.Answers.Length) == -1);
            responses.Any(t => (addCount += t.AdditionalRecords.Length) == -1);
            responses.Any(t => (authCount += (t.AuthoritativeAnswer ? 1 : 0)) == -1);

            Console.WriteLine(";<<>> nettools 0.1 <<>> @" + dns_root.ToString() + " ANY " + hostname);
            Console.WriteLine(";flags:  ; QUERY: " + queryCount + ", ANSWER: " + answerCount + ", AUTHORITY: " + authCount + ", ADDITIONAL: " + addCount);

            Console.WriteLine("");

            Console.WriteLine(";; QUESTION SECTION:");
            Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));

            foreach (Response response in responses)
            {
                if (response.Questions.Length > 0)
                {
                    foreach (Question quest in response.Questions)
                    {
                        Console.WriteLine(formatter, quest.Domain, "", quest.Class, quest.Type, "");
                        Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));
                    }
                }
            }
            
            Console.WriteLine("");
            Console.WriteLine(";; ANSWER SECTION:");
            Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));

            foreach (Response response in responses)
            {
                if (response.Answers.Length > 0)
                {
                    foreach (Answer answer in response.Answers)
                    {
                        if (answer.Record == null)
                        {
                            Console.WriteLine(formatter, answer.Domain.Trim('\n'), answer.Ttl, answer.Class, answer.Type, "No record for this answer");
                        }
                        else
                        {
                            string[] recordLines = answer.Record.ToString().Split('\n');
                            Console.WriteLine(formatter, answer.Domain.Trim('\n'), answer.Ttl, answer.Class, answer.Type, recordLines[0]);

                            for (int i = 1; i < recordLines.Length; i++)
                                Console.WriteLine(noTableFormatter, "", "", "", "", recordLines[i]);
                        }
                        Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));
                    }
                }
            }


            Console.WriteLine("");
            Console.WriteLine(";; ADDITIONAL SECTION:");
            Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));

            foreach (Response response in responses)
            {
                if (response.AdditionalRecords.Length > 0)
                {
                    foreach (AdditionalRecord additionalRecord in response.AdditionalRecords)
                    {
                        Console.WriteLine(formatter, additionalRecord.Domain, additionalRecord.Ttl, additionalRecord.Class, additionalRecord.Type, additionalRecord.Record);
                        Console.WriteLine(formatter, new string('-', 30), new string('-', 7), new string('-', 3), new string('-', 6), new string('-', 60));
                    }
                }
            }

            return true;
        }

        private Response QueryDNSServer(IPAddress dnsServer, DnsClass dnsClass, DnsType dnsType, string domain)
        {
            Request request = new Request();
            
            request.AddQuestion(new Question(domain, dnsType, dnsClass));

            return Resolver.Lookup(request, dnsServer);
        }
       
        public string[] GetAvailableCommands()
        {
            return new string[] { "dnsquery", "dns", "querydns", "getdns" };
        }

        public string[] GetAvailableArguments()
        {
            return new string[] { "h*0", "host|h", "hostname|h", "d|h", "domain|h", "ds", "dns|ds", "dnssrv|ds", "dnsserver|ds" };
        }

        public string GetHelp()
        {
            return "Print informations about the given IP";
        }

        public string GetArgumentHelp(string arg)
        {
            if (arg == "h")
                return "The domain which should be scanned";
            else if (arg == "d")
                return "The DNS-Server which should be queried";
            else
                return null;
        }

        public void Stop() { }

    }

}
