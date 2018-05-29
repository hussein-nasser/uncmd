using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    public class Subnetwork
    {
 
        /// <summary>
        /// THe name of the domain network this subnetwork belong to.
        /// </summary>
        public string DomainNetworkName { get; set; }


        /// <summary>
        /// full tier object that subnetwork belongs too..
        /// </summary>

        public Tier Tier { get; set; }

        /// <summary>
        /// Name of the tier this subnetwork belong to
        /// </summary>
        public string TierName { get; set; }

        /// <summary>
        /// is it dirty
        /// </summary>
        public bool isDirty { get; set; }

        /// <summary>
        /// Subnetwork name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// update errors
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// whether subnetwork updated successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of features in this subnetwork
        /// </summary>
        public long FeatureCount { get; set; }

        /// <summary>
        /// trace the subnetwork and return the number of features
        /// </summary>
        /// <returns></returns>
        public long Trace()
        {
            string response = "";
            UNWorkspace unworkspace = UNWorkspace.getWorkspace();
            try
            {
                    
                    unworkspace.onProgressDelegate(0, ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Trace Subnetwork " + Name + " In Tier " + TierName + " Domain " + DomainNetworkName), 0);

                    Dictionary<string, string> ps = new Dictionary<string, string>();
                    ps.Add("sessionId", unworkspace.SessionID);
                    ps.Add("gdbVersion", unworkspace.CurrentVersion.Name);
                    ps.Add("moment", unworkspace.CurrentVersion.Moment.ToString());
                    ps.Add("domainNetworkName", DomainNetworkName);
                    ps.Add("traceType", "subnetwork");         
                    ps.Add("traceLocations", "[]");
                    JObject tierdef = this.Tier.SubnetworkTraceConfiguraiton;
                    tierdef["subnetworkName"] = Name;
                    ps.Add("traceConfiguration", tierdef.ToString());
                    response = unworkspace.Request(unworkspace.UNSServiceURL + "/trace", ps);
                    ResponseMessage = response;
                    JObject subnetworktrace = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                    JArray features = subnetworktrace["traceResults"]["elements"] as JArray;
                //unworkspace.onProgressDelegate(0, ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Trace Subnetwork " + Name + " returned " + features.Count + " features"), 0);

                FeatureCount = features.Count;

                //compare the subnetwork with the gold 
                List<SubnetworkName> thesub = unworkspace.GoldSubnetworks.Where(t => t.Name == this.Name).ToList();
                if (thesub.Count> 0)
                {
                    if (thesub[0].TraceCount != FeatureCount)
                        unworkspace.onProgressDelegate(0, ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [TRACE TEST FAILED] Subnetwork " + Name + " returned different # of features. Expected [" + thesub[0].TraceCount  + "] Returned [" + features.Count + "] "), 0);
                    else
                        unworkspace.onProgressDelegate(0, ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [TRACE TEST PASSED] Subnetwork " + Name + " returned same      # of features. Expected [" + thesub[0].TraceCount + "] Returned [" + features.Count + "] "), 0);
                }
                return features.Count;
        
            }
            catch (Exception ex)
            {
                Success = false;
                ResponseMessage = response + " - " + ex.Message;
                unworkspace.onProgressDelegate(0,"[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Failed] Trace Subnetwork " + Name + " In Tier " + TierName + " Domain " + DomainNetworkName + "[" + ResponseMessage + "]",0);
                return 0;
            }
        }

        /// <summary>
        /// Update subnetwork
        /// </summary>
        public void Update()
        {
            string response = "";
            try
            {
                if (isDirty == false)
                {
                    UNWorkspace unworkspace = UNWorkspace.getWorkspace();
                    unworkspace.onProgressDelegate(0,("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Update Subnetwork " + Name + " In Tier " + TierName + " Domain " + DomainNetworkName),0);

                    Dictionary<string, string> ps = new Dictionary<string, string>();
                    ps.Add("sessionId", unworkspace.SessionID);
                    ps.Add("gdbVersion", unworkspace.CurrentVersion.Name);
                    ps.Add("moment", unworkspace.CurrentVersion.Moment.ToString());
                    ps.Add("domainNetworkName", DomainNetworkName);
                    ps.Add("tierName", TierName);
                    ps.Add("subnetworkName", Name);
                    ps.Add("allSubnetworksInTier", "false");
                    ps.Add("traceConfiguration", "{}");
                    response = unworkspace.Request(unworkspace.UNSServiceURL + "/updateSubnetwork", ps);
                    ResponseMessage = response;
                    JObject subnetworkresponse = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                    
                    if (subnetworkresponse["success"].ToString().ToLower() == "false")
                    {
                        Success = false;
                        string failedmessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Failed] Update Subnetwork " + Name + " In Tier " + TierName + " Domain " + DomainNetworkName + "[" + ResponseMessage + "]";
                        unworkspace.onProgressDelegate(0, failedmessage, 0);
                    }

                    else
                    {
                        //refresh version after each successful subnetwork to get the new moement
                        unworkspace.RefreshVersion();
                        Success = true;                  
                    }

                }
                else //clean subnetworks does not request updating.. just succeed
                    Success = true;
            }
            catch(Exception ex)
            {
                Success = false;
                ResponseMessage = response + " - " + ex.Message ;
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Failed] Update Subnetwork " + Name + " In Tier " + TierName + " Domain " + DomainNetworkName + "[" + ResponseMessage + "]");

            }

        }

    }
}



public class SubnetworkName
{

    public string DomainNetwork { get; set; }
    public string Tier { get; set; }
    public string Name { get; set; }
    public long TraceCount { get; set; }

}
