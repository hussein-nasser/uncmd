using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    public class Tier
    {

        public Tier()
        {
            Subnetworks = new List<Subnetwork>();
        }

        /// <summary>
        /// Domain network name
        /// </summary>
        public string DomainNetworkName { get; set; }

        /// <summary>
        /// Tier name
        /// </summary>
        public string Name { get; set; }
          
        /// <summary>
        /// All Subnetworks in the tier
        /// </summary>
        public List<Subnetwork> Subnetworks { get; set; }

        /// <summary>
        /// The trace configuraiton of this tier
        /// </summary>
        public JObject SubnetworkTraceConfiguraiton { get; set; }
        /// <summary>
        /// Update all subnetworks in this tier
        /// </summary>
        public List<Subnetwork> UpdateSubnetworks()
        {
            List<Subnetwork> badSubnetworks = new List<Subnetwork>();
            int count = Subnetworks.Count;
            double i = 0;
            foreach (Subnetwork s in Subnetworks)
            {            
                //update dirty subnetworks 
                if (s.isDirty == false)
                {
                    double prg = (i / count) * 100;
// UNWorkspace.getWorkspace().onProgressDelegate(0, "Update Subnetwork " + s.Name + " In Tier " + this.Name, (int) prg);
                    s.Update();
                   // s.Trace();
                }
                if (s.Success == false)
                    badSubnetworks.Add(s);

                i++;
            }

            return badSubnetworks;
             
        }

     

    }
}
