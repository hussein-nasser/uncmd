using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    public class ServerAdminUtil
    {
        public string Token { get; set; }


        HttpClient _client;
        public string ServerURL { get; set; }

        public ServerAdminUtil(string username, string password, string portalurl, string serverurl)
        {
            PortalTokenGenerator p = new PortalTokenGenerator(username, password, portalurl);
            ServerURL = serverurl;
            Token = p.Generate();
            _client = new HttpClient();

        }
         
        public string RegisterSDEDataSource(string datasourcename, string connectionString)
        {
            //https://utilitynetwork.esri.com/server/admin

            Dictionary<string, string> ps = new Dictionary<string, string>();

            ps.Add("item", "{'type':'egdb','info':{'dataStoreConnectionType':'shared','isManaged':false,'connectionString':'" + connectionString + "'},'path':'/enterpriseDatabases/" + datasourcename + "'}");
             
           return  Request(ServerURL  + "/admin/data/registerItem" , ps);

        }

        public string Request(string methodurl, Dictionary<string, string> parameters)
        {
        
           
            //always return json
            parameters.Add("f", "json");
            //get the parameters (must be populated during setup)
            var content = new FormUrlEncodedContent(parameters);
             
            var response = _client.PostAsync(methodurl + "?token=" + Token, content).Result;
            //wait for response

            string responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;

        }

    }
}
