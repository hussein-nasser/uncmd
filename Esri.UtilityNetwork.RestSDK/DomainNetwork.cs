using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Author  : Hussein Nasser
/// Date    : May/ 29 / 2018
/// Twitter : @hnasr
/// </summary>
namespace Esri.UtilityNetwork.RestSDK
{
    public class DomainNetwork
    {
        public DomainNetwork()
        {
            Tiers = new List<Tier>();
        }
        /// <summary>
        /// Domain network name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// list all of tiers
        /// </summary>
        public List<Tier> Tiers { get; set; }

        /// <summary>
        /// Update all subnetworks in this Domain Network
        /// </summary>
        public List<Subnetwork> UpdateSubnetworks()
        {
            List<Subnetwork> badSubnetworks = new List<Subnetwork>();
            foreach (Tier t in Tiers)
            {
                badSubnetworks.AddRange(t.UpdateSubnetworks());
            }

            return badSubnetworks;
        }
    }
}
