using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{

    /// <summary>
    /// singleton
    /// </summary>
    public class UNWorkspace
    {


        private static UNWorkspace _unworkspace;

        public UNWorkspace()
        {
            DomainNetworks = new List<DomainNetwork>();
            _client = new HttpClient();
            //increase timeout to 30 minutes
            _client.Timeout = new TimeSpan(0, 30, 0);
            Versions = new List<VersionInfo>();
            onProgressDelegate = onProgress;
            GoldSubnetworks = new List<SubnetworkName>();
        }
        public static UNWorkspace getWorkspace()
        {
            if (_unworkspace == null)
                _unworkspace = new UNWorkspace();
                return _unworkspace;
        }

        /// <summary>
        /// time of the last generated token..
        /// </summary>
        public DateTime LastTokenTime { get; set; }


        /// <summary>
        /// list of subnetworks and their trace count from json.
        /// </summary>
        public List<SubnetworkName> GoldSubnetworks { get; set; }
        /// <summary>
        /// Refresh the current version for the new moment
        /// </summary>
        public void RefreshVersion()
        {
            //query the verison info
            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Clear();
            string vmsdf = Request(VMSServiceURL + "/versions/" + CurrentVersion.Guid, ps);
            JObject vms = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(vmsdf);
            CurrentVersion.Moment = double.Parse(vms["modifiedDate"].ToString());
            CurrentVersion.Guid =  vms["versionGuid"].ToString().Replace("{","").Replace("}","");
            CurrentVersion.CreationMoment = double.Parse(vms["creationDate"].ToString());
            CurrentVersion.Access = vms["access"].ToString();
        }


        public List<VersionInfo> Versions { get; set; }


        public void UpdateIsConnected()
        {
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Update Is Connected ");
            UNWorkspace unworkspace = UNWorkspace.getWorkspace();
            Dictionary<string, string> ps = new Dictionary<string, string>();
        

            string response = unworkspace.Request(unworkspace.UNSServiceURL + "/updateIsConnected", ps);
            JObject subnetworkresponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            if (subnetworkresponse["success"].ToString() == "false")
            {

                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Failed] Update Is Connected");
            }

            else
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Done] Update Is Connected");
        }

        /// <summary>
        /// ability to change version, can only be called when connected
        /// </summary>
        /// <param name="version"></param>
        public void ChangeVersion(VersionInfo version)
        {

            try
            {
                //stop reading on the current version
                StopReading();
                CurrentVersion = version;
                //start reading on the current version
                StartReading();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// generate new token
        /// </summary>
        public void GenerateToken()
        {
            //generate token
            PortalTokenGenerator p = new RestSDK.PortalTokenGenerator(Username, Password, PortalURL);
            Token = p.Generate();
            LastTokenTime = DateTime.Now;
        }

        /// <summary>
        /// Get Services
        /// </summary>
        /// <param name="portalurl"></param>      
        /// <param name="username"></param>
        /// <param name="password"></param>
        public List<Service> GetServices(string portalurl, string username, string password)
        {
            List<Service> services = new List<Service>();
            try
            {
                  
                Dictionary<string, string> ps = new Dictionary<string, string>();
                Username = username;
                Password = password;
                PortalURL = portalurl;

                //generate token
                GenerateToken();

                if (this.Token == null) throw new Exception("Cannot generate token, invalid username or password.");
                //retrieve the feature service definition
                ps.Clear();
                ps.Add("types", "Feature Service");
                string fsdf = Request(portalurl + @"/sharing/rest/content/users/" + username, ps);
                JObject servicesjson = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(fsdf);
                JArray items = (JArray) servicesjson["items"];
                foreach (JObject jService in items)
                {
                    string fullserviceurl = jService["url"].ToString(); //with /featureservice
                    if (fullserviceurl == "") continue;
                    string serviceurl = fullserviceurl.Substring(0,fullserviceurl.LastIndexOf(@"/"));

                    services.Add(new Service { Title = jService["title"].ToString(), Type = jService["type"].ToString(), URL = serviceurl });
                }

                
            }

            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error " + ex.StackTrace);
                throw (ex);
            }
            return services;

        }

        public void Restart()
        {
            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Clear();
            string adminurl = ServiceURL.Replace("/rest/", "/admin/");
            Request(adminurl + ".MapServer/stop", ps);
            ps.Clear();
            Request(adminurl + ".MapServer/start", ps);
        }

        /// <summary>
        /// Connect to workspace, find the utility network layer, get the system layers
        /// </summary>
        /// <param name="portalurl">https://utilitynetwork.esri.com/portal</param>
        /// <param name="serviceurl">https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres</param>
        /// <param name="username">unadmin</param>
        /// <param name="password">unadmin</param>
        public void Connect(string portalurl, string serviceurl, string username, string password)
        {
            try
            {
                
                Versions.Clear();
                SessionID = Guid.NewGuid().ToString("B").ToUpper();
            Dictionary<string, string> ps = new Dictionary<string, string>();
            Username = username;
            Password = password;
            ServiceURL = serviceurl;
            PortalURL = portalurl;

            //generate token
            GenerateToken();
            //retrieve the feature service definition
            ps.Clear();
            string fsdf = Request(FeatureServiceURL, ps);
            _featureservicedefinition = (JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(fsdf);
                if (_featureservicedefinition["controllerDatasetLayers"] == null) throw new Exception("Service is stopped. " + serviceurl);

            string unlayer = _featureservicedefinition["controllerDatasetLayers"]["utilityNetworkLayerId"].ToString();
            //retrieve the utility network definition
            ps.Clear();
            string undf = Request(FeatureServiceURL + "/" + unlayer, ps);
            _utilitynetworkdefinition = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(undf);

            ps.Clear();
            ps.Add("layers","[" + unlayer + "]");
            string undffull = Request(FeatureServiceURL + "/queryDataElements", ps);
          _utilitynetworkdefinitionfull = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(undffull);

                //ping VMS for default version 
                ps.Clear();
            string vmsdf = Request(VMSServiceURL, ps);
            JObject vms = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(vmsdf);
                CurrentVersion = new VersionInfo { Name = vms["defaultVersionName"].ToString(), Guid = vms["defaultVersionGuid"].ToString().Replace("{", "").Replace("}", "") };
         


            //refreshversion
            RefreshVersion();


            //populate versions
            ps.Clear();
            vmsdf = Request(VMSServiceURL + "/versions", ps);
            vms = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(vmsdf);

            JArray jaVersions = (JArray) vms["versions"];
            foreach (JObject v in jaVersions)
            {
                VersionInfo vi = new VersionInfo{ Name = v["versionName"].ToString(),  Guid = v["versionGuid"].ToString().Replace("{", "").Replace("}", "") };
                Versions.Add(vi);
            }


            //populate the UN def..
            PopulateUtilityNetworkDefinition();

                //Start reading
           StartReading();


            }

            catch(Exception ex)
            {
                Console.WriteLine("Unexpected error " + ex.StackTrace);
                throw (ex);
            }


        }


        public void StartReading()
        {
            
            //start reading
            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Clear();
            ps.Add("sessionId", SessionID);
            string sreading = Request(VMSServiceURL + "/versions/" + CurrentVersion.Guid + "/startReading", ps);
            JObject startReadingresponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(sreading);
            if (startReadingresponse["success"].ToString() == "false")
            {
                throw new Exception("Failed to start reading");
            }
        }

        public void StopReading()
        {
        
            //stop reading
            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Clear();
            ps.Add("sessionId", SessionID);
            string sreading = Request(VMSServiceURL + "/versions/" + CurrentVersion.Guid + "/stopReading", ps);
            JObject startReadingresponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(sreading);
            if (startReadingresponse["success"].ToString() == "false")
            {
                throw new Exception("Failed to stop reading");
            }

        }


        /// <summary>
        /// validate the full extents of the utility network
        /// </summary>
        /// <returns></returns>
        public string ValidateNetworkTopology(string extent)
        {

            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Validate Network Topology ");
            UNWorkspace unworkspace = UNWorkspace.getWorkspace();
            Dictionary<string, string> ps = new Dictionary<string, string>();

            string fullExtent = extent;
            ps.Add("validateArea", fullExtent);
            string response = unworkspace.Request(unworkspace.UNSServiceURL + "/validateNetworkTopology", ps);
            JObject subnetworkresponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            if (subnetworkresponse["success"].ToString() == "false")
            {

                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Failed] Validate Network Topology");
            }

            else
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Done] Validate Network Topology");

            return response;

        }


        /// <summary>
        /// validate the givens extent of the utility network
        /// </summary>
        /// <returns></returns>
        public string ValidateNetworkTopology()
        {
 
            string fullExtent = _featureservicedefinition["fullExtent"].ToString();
            return ValidateNetworkTopology(fullExtent);
             

        }
        private void PopulateVersions()
        {


        }
        private void PopulateUtilityNetworkDefinition()
        {
            DomainNetworks.Clear();


            SubnetworksTable = int.Parse(_utilitynetworkdefinition["systemLayers"]["subnetworksTableId"].ToString());

            JArray domainnetworks = (JArray)_utilitynetworkdefinitionfull["layerDataElements"][0]["dataElement"]["domainNetworks"];
            foreach (JObject d in domainnetworks)
            {
                DomainNetwork domainNetwork = new DomainNetwork();
                domainNetwork.Name = d["domainNetworkName"].ToString();
                JArray tiers =  (JArray)d["tiers"];
                
                foreach (JObject t in tiers)
                {
                    Tier tier = new Tier();
                    tier.Name = t["name"].ToString();
                    tier.DomainNetworkName = domainNetwork.Name;
                    tier.SubnetworkTraceConfiguraiton = t["updateSubnetworkTraceConfiguration"] as JObject;
                     domainNetwork.Tiers.Add(tier);                 
                }

                DomainNetworks.Add(domainNetwork);
            }
        }
        

        
       /// <summary>
       /// Subnetworks table layerid
       /// </summary>
        public int SubnetworksTable { get; set; }

        /// <summary>
        /// The Utility network definition
        /// </summary>
        private Newtonsoft.Json.Linq.JObject _utilitynetworkdefinition;

        /// <summary>
        /// the full definition
        /// </summary>
        private Newtonsoft.Json.Linq.JObject _utilitynetworkdefinitionfull;

        /// <summary>
        /// THe feature service definition
        /// </summary>
        private Newtonsoft.Json.Linq.JObject _featureservicedefinition;

        /// <summary>
        /// Link to the service url e.g https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres
        /// </summary>
        public string ServiceURL { get; set; }

        /// <summary>
        /// https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres/FeatureServer
        /// </summary>
        public string FeatureServiceURL { get { return ServiceURL + "/FeatureServer"; } }

        /// <summary>
        /// https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres/FeatureServer
        /// </summary>
        public string MapServiceURL { get { return ServiceURL + "/MapServer"; } }


        /// <summary>
        /// https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres/MapServer/exts/VersionManagementServer
        /// </summary>
        public string VMSServiceURL { get { return MapServiceURL + "/exts/VersionManagementServer"; } }


        /// <summary>
        /// https://utilitynetwork.esri.com/server/rest/services/FondDuLac_Postgres/MapServer/exts/UtilityNetworkServer
        /// </summary>
        public string UNSServiceURL { get { return MapServiceURL + "/exts/UtilityNetworkServer"; } }


        /// <summary>
        /// Link to the portal url
        /// </summary>
        public string PortalURL { get; set; }

        /// <summary>
        /// Portal username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// portal password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Token used
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Return the current version
        /// </summary>
        public VersionInfo CurrentVersion { get; set; }

        /// <summary>
        /// list of all domain networks in this tier
        /// </summary>
        public List<DomainNetwork> DomainNetworks { get; set; }

        /// <summary>
        /// Update all subnetworks in this utility network
        /// </summary>
        public List<Subnetwork> UpdateSubnetworks()
        {
            PopulateSubnetworks();
            List<Subnetwork> badSubnetworks = new List<Subnetwork>();

            foreach (DomainNetwork d in DomainNetworks)
            {                
                badSubnetworks.AddRange(d.UpdateSubnetworks());
            }

            return badSubnetworks;
        }


        /// <summary>
        /// Trace all subnetworks in this utility network
        /// </summary>
        public List<Subnetwork> TraceSubnetworks()
        {
            PopulateSubnetworks();
            List<Subnetwork> badSubnetworks = new List<Subnetwork>();

            foreach (DomainNetwork d in DomainNetworks)
            {
                badSubnetworks.AddRange(d.UpdateSubnetworks());
            }

            return badSubnetworks;
        }



        #region HTTP
        HttpClient _client;

        public string Request(string methodurl, Dictionary<string,string> parameters)
        {

            //check if token is about to expire ... create new one half anoour

            if ((DateTime.Now - LastTokenTime).TotalMinutes > 30)
                GenerateToken();
            
            //always return json
            parameters.Add("f", "json");
            //get the parameters (must be populated during setup)
            var content = new FormUrlEncodedContent(parameters);
      

            var response = _client.PostAsync(methodurl + "?token=" + Token, content).Result;
            //wait for response

            string responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;

        }


        /// <summary>
        /// disconnect workspace
        /// </summary>
        public void Disconnect()
        {
            StopReading();
        }

        /// <summary>
        /// Sessionid guid
        /// </summary>
        public string SessionID { get; set; }


        private string getVal (JToken jtoken, string field)
        { 
            try
            {
                return jtoken[field.ToLower()].ToString();
            }
            catch 
            {
                return jtoken[field.ToUpper()].ToString();
            }

        }

        public int onProgress(int msgid, string msgtext, int progress)
        {
            Console.WriteLine(msgtext);
            return 1;
        }

        public Func<int, string, int, int> onProgressDelegate { get; set; }
        
        private List<Subnetwork> _subnetworks = new List<Subnetwork>();
        public List<Subnetwork> Subnetworks { get { return _subnetworks; } } 

        /// <summary>
        /// Query and populates all subnetworks
        /// </summary>
        public void PopulateSubnetworks()
        {
            _subnetworks = new List<RestSDK.Subnetwork>();
            UNWorkspace unworkspace = UNWorkspace.getWorkspace();
            Dictionary<string, string> ps = new Dictionary<string, string>();
            ps.Add("outFields", "SUBNETWORKNAME,domainnetworkname,tiername,isdirty");
            ps.Add("gdbVersion", CurrentVersion.Name);
            ps.Add("where","isdirty=1");
            ps.Add("returnDistinctValues", "true");   
            string response = unworkspace.Request(unworkspace.FeatureServiceURL + "/" + unworkspace.SubnetworksTable + "/query", ps);
            JObject jResponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response);

            JArray features = (JArray)jResponse["features"];
            
            foreach(JObject f in features)
            {
                Subnetwork s = new Subnetwork();
                s.DomainNetworkName = getVal(f["attributes"], "domainnetworkname");
                s.TierName = getVal(f["attributes"], "tiername"); // f["attributes"]["tiername"].ToString();
                s.Name = getVal(f["attributes"], "subnetworkname");  //f["attributes"]["subnetworkname"].ToString();              
                _subnetworks.Add(s);
            }


            foreach (DomainNetwork d in DomainNetworks)
                foreach(Tier t in d.Tiers)
                {
                    t.Subnetworks = _subnetworks.Where(s => s.TierName == t.Name && s.DomainNetworkName == t.DomainNetworkName).ToList<Subnetwork>();
                    foreach (Subnetwork s in t.Subnetworks) s.Tier = t;
                }

        }

        #endregion


    }
}
