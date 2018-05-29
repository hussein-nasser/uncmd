using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
     
        /// <summary>
        /// class that generates a portal token from username/password
        /// </summary>
        public class PortalTokenGenerator
        {
            string _username;
            string _password;
            string _portalurl;
            public PortalTokenGenerator(string username, string password, string portalurl)
            {
                _username = username;
                _password = password;
                _portalurl = portalurl;
            }

            public string Generate()
            {

                var responseString = "";

                try
                { 

                    var request = (HttpWebRequest)WebRequest.Create(_portalurl + "/sharing/generateToken");
                 
                    var postData = "username=" + _username;
                    postData += "&password=" + _password;
                    postData += "&request=getToken";
                    postData += "&referer=" + Dns.GetHostName();
                    postData += "&expiration=60";
                    postData += "&f=json";

                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    dynamic j = JsonConvert.DeserializeObject(responseString);

                    JValue jv = j.token;

                    if (j.token == null) return null;

                    string token = jv.Value.ToString();

                    return token;

                }
                catch (Exception ex)
                {
                    // Console.WriteLine("Failed to generate token " + responseString + ex.Message);
                    return null;
                }
            }



        }
    }
 