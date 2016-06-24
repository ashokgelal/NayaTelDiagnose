using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewSwitchingNavigation.Infrastructure.Utils;

namespace ViewSwitchingNavigation.Infrastructure
{
    public class PropertyProvider
    {
        private static PropertyProvider _propertyProvider;
        private JToken confJson;
        private JToken vendorConfJson;
        private JToken userConfJson;

        public enum WindowsMethod
        {
            VerifyConnectivity,
            SpeedTest,
            WifiInspector,
            ActiveUsers,
            DeviceUsage,
            network_response
        };
        public enum NetworkResponse
        {
            MAX_TTL,
            IterationInterval,
            IterationsPerHost,
            PacketSize
            
        };
        public enum TestType
        {
            Individual,
            DiagnoseAll
        }

        private PropertyProvider()
        {

            loadTestConfJson();
        }

        public static PropertyProvider getPropertyProvider()
        {
            if (_propertyProvider == null)
            {
                _propertyProvider = new PropertyProvider();
            }
            return _propertyProvider;
        }
        public int  getMTRProperty(NetworkResponse key) {
            int result = 0;
            switch (key)
            {
                case NetworkResponse.MAX_TTL:
                    result = confJson.Value<JToken>("network_response").Value<int>("max_ttl");

                    break;
                case NetworkResponse.IterationInterval:
                    result = confJson.Value<JToken>("network_response").Value<int>("iteration_interval");

                    break;
                case NetworkResponse.IterationsPerHost:
                    result = confJson.Value<JToken>("network_response").Value<int>("iterations_per_host");

                    break;
                case NetworkResponse.PacketSize:
                    result = confJson.Value<JToken>("network_response").Value<int>("packet_size");

                    break;
                default:
                    break;
            }
            return result;
        }
        private void loadTestConfJson()
        {

            JObject configJson = HttpRequest.getConfigJSon();

            if (configJson != null) {
                System.IO.File.WriteAllText(Constants.CONFIG_NAYATEL_FILE, configJson.ToString());
            }



            using (StreamReader r = new StreamReader(Constants.CONFIG_NAYATEL_FILE))
            {
                string json = r.ReadToEnd();
                JObject o = JObject.Parse(json);

                confJson = o["windows"];

                //confJson = confJson.Value["ref_id"];
            }

            using (StreamReader r = new StreamReader(Constants.Vendor_MAC_FILE))
            {
                string json = r.ReadToEnd();
                JObject o = JObject.Parse(json);

                vendorConfJson = o;

                //confJson = confJson.Value["ref_id"];
            }
            try
            {
                using (StreamReader r = new StreamReader(Constants.CONFIG_User_FILE))
                {
                    string json = r.ReadToEnd();
                    JObject o = JObject.Parse(json);

                    userConfJson = o;

                    //confJson = confJson.Value["ref_id"];
                }
            }
            catch (Exception)
            {

             }
            

        }
        public String getVendorByMacAddress(String mac)
        {
            String vendor = vendorConfJson.Value<String>(mac);
            return vendor;
        }
        public Tuple<int, int> getTestTimeout(WindowsMethod method, TestType testType)
        {
            int ConnectedTimeout = 0;
            int notConnectedTimout = 0;
            switch (method)
            {
                case WindowsMethod.VerifyConnectivity:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("verify_connectivity").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("verify_connectivity").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("verify_connectivity").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("verify_connectivity").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }


                    }
                    break;
                case WindowsMethod.SpeedTest:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("speed_test").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("speed_test").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("speed_test").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("speed_test").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                    }
                    break;
                case WindowsMethod.WifiInspector:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("wifi_inspector").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("wifi_inspector").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("wifi_inspector").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("wifi_inspector").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                    }
                    break;
                case WindowsMethod.ActiveUsers:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("active_users").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("active_users").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("active_users").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("active_users").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                    }
                    break;
                case WindowsMethod.DeviceUsage:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("device_usage").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("device_usage").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("device_usage").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("device_usage").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                    }
                    break;
                case WindowsMethod.network_response:
                    {
                        if (testType == TestType.Individual)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("network_response").Value<JToken>("individual").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("network_response").Value<JToken>("individual").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                        else if (testType == TestType.DiagnoseAll)
                        {
                            ConnectedTimeout = confJson.Value<JToken>("network_response").Value<JToken>("diagnose_all").Value<JToken>("connected").Value<int>("timeout");
                            notConnectedTimout = confJson.Value<JToken>("network_response").Value<JToken>("diagnose_all").Value<JToken>("not_connected").Value<int>("timeout");

                        }
                    }
                    break;
                default:
                    break;
            }
            return new Tuple<int, int>(ConnectedTimeout, notConnectedTimout);
        }
        public String getEmailAddress()
        {
            if (userConfJson == null)
                return null;
            String email = userConfJson.Value<String>("email");
            return email;
        }
        public String getPhoneNumberAddress()
        {
            if (userConfJson == null)
                return null;
            String phone = userConfJson.Value<String>("phonenumber");
            return phone;
        }
        
        public void setPhoneAndEmail(String email,String cellno)
        {
            try
            {
                String json = String.Format(@" ""email"":""{0}"", ""phonenumber"":""{1}"" ", email, cellno);
                json = "{" + json + "}";

                if (json != null)
                {
                    System.IO.File.WriteAllText(Constants.CONFIG_User_FILE, json);
                }
                JObject o = JObject.Parse(json);

                userConfJson = o;
            }
            catch (Exception ex)
            {

             }
            // String phone = userConfJson.Value<String>("phonenumber");
            //return phone;
        }

    }



}
