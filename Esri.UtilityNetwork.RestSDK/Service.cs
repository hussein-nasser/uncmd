using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    public class Service
    {

        /// <summary>
        /// Name of the service
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// URL
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Service type
        /// </summary>
        public string Type { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
