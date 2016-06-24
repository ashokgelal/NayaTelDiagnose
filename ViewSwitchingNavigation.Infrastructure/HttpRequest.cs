using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure
{
  public  class HttpRequest
    {

      public  static async Task<Tuple<String, String,String>> asyncVerifyConectivityTest(string url ,double timeout)
        {
            String status = "-1";
            String wan_ip = "";
            String msg = "";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.Timeout = TimeSpan.FromMilliseconds(timeout);

                //  client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("accept-encoding"," gzip, deflate");
                //client.DefaultRequestHeaders.Add("accept-language" ,"en - US, en; q = 0.8");
                client.DefaultRequestHeaders.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");

                var cts = new CancellationTokenSource();
                try
                {
                    HttpResponseMessage response = null;
                    try
                    {
                          response = await client.GetAsync(url);

                    }
                    catch (Exception ex)
                    {

                        status = "-700";
                        return new Tuple<String, String, String>(status, wan_ip, msg);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            String json = await response.Content.ReadAsStringAsync();
                            try
                            {
                                JObject o = JObject.Parse(json);
                                if (o != null)
                                {
                                    status = o["status"].ToString();
                                    wan_ip = o["wan_ip"].ToString();
                                    msg = o["msg"].ToString();
                                }
                                return new Tuple<String, String, String>(status, wan_ip, msg);

                            }
                            catch (Exception ex)
                            {

                                status = "-800";
                                return new Tuple<String, String, String>(status, wan_ip, msg);
                            }
                            



                        }
                    }
                    else
                    {
                        status = response.StatusCode.ToString();
                        return new Tuple<String, String, String>(status, wan_ip, msg);


                    }//end else

                }
                catch (WebException ex)
                {
                    // handle web exception
                     status = "-500";
                    return new Tuple<String, String, String>(status, wan_ip, msg);

                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        // a real cancellation, triggered by the caller
                        status = "-600";
                        return new Tuple<String, String, String>(status, wan_ip, msg);

                    }
                    else
                    {
                        status = "-700";
                        return new Tuple<String, String, String>(status, wan_ip, msg);

                        // a web request timeout (possibly other things!?)
                    }
                }//catch

            }//end using

        }

        public static  Tuple<String, String, String>  verifyConectivityTest(string url, double timeout)
        {
            String status = "-1";
            String wan_ip = "";
            String msg = "";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.Timeout = TimeSpan.FromMilliseconds(timeout);

                //  client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("accept-encoding"," gzip, deflate");
                //client.DefaultRequestHeaders.Add("accept-language" ,"en - US, en; q = 0.8");
                client.DefaultRequestHeaders.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");

                var cts = new CancellationTokenSource();
                try
                {

                    HttpResponseMessage response = null;
                    try
                    {
                        response = client.GetAsync(url).Result;
                    }
                    catch (Exception ex)
                    {

                        status = "-700";
                        return new Tuple<String, String, String>(status, wan_ip, msg);
                    }

                if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            // by calling.Result you are performing a synchronous call
                             var responseContent = response.Content;

                            // by calling .Result you are synchronously reading the result
                            string responseString = responseContent.ReadAsStringAsync().Result;
                            String json = responseString;
                            try
                            {
                                JObject o = JObject.Parse(json);
                                if (o != null)
                                {
                                    status = o["status"].ToString();
                                    wan_ip = o["wan_ip"].ToString();
                                    msg = o["msg"].ToString();
                                }
                                return new Tuple<String, String, String>(status, wan_ip, msg);

                            }
                            catch (Exception ex)
                            {

                                status = "-800";
                                return new Tuple<String, String, String>(status, wan_ip, msg);
                            }




                        }
                    }
                    else
                    {
                        status = response.StatusCode.ToString();
                        return new Tuple<String, String, String>(status, wan_ip, msg);


                    }//end else

                }
                catch (WebException ex)
                {
                    // handle web exception
                    status = "-500";
                    return new Tuple<String, String, String>(status, wan_ip, msg);

                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        // a real cancellation, triggered by the caller
                        status = "-600";
                        return new Tuple<String, String, String>(status, wan_ip, msg);

                    }
                    else
                    {
                        status = "-700";
                        return new Tuple<String, String, String>(status, wan_ip, msg);

                        // a web request timeout (possibly other things!?)
                    }
                }//catch

            }//end using

        }
        public static JObject getConfigJSon()
        {
            JObject json = null;
            String status;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");

                var cts = new CancellationTokenSource();
                try
                {

                    HttpResponseMessage response = null;
                    try
                    {
                        response = client.GetAsync(Constants.CONFIG_NAYATEL_URL).Result;
                    }
                    catch (Exception ex)
                    {

                        status = "-700";
                        return null;
                     }

                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            // by calling.Result you are performing a synchronous call
                            var responseContent = response.Content;

                            // by calling .Result you are synchronously reading the result
                            string responseString = responseContent.ReadAsStringAsync().Result;
                            String res = responseString;
                            try
                            {
                                json = JObject.Parse(res);
                                 
                                return json;

                            }
                            catch (Exception ex)
                            {

                                status = "-800";
                             }




                        }
                    }
                    else
                    {
                        status = response.StatusCode.ToString();
 

                    }//end else

                }
                catch (WebException ex)
                {
                    // handle web exception
                    status = "-500";
                    return null;

                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        // a real cancellation, triggered by the caller
                        status = "-600";
 
                    }
                    else
                    {
                        status = "-700";
 
                        // a web request timeout (possibly other things!?)
                    }
                    return null;

                }//catch
                return json;

            }//end using
        }

    }
}
