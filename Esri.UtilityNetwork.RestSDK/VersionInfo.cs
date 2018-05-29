using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esri.UtilityNetwork.RestSDK
{
    public class VersionInfo
    {
        /// <summary>
        /// name of the version
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// current moment of the version
        /// </summary>
        public double Moment { get; set; }

        /// <summary>
        /// The moment this version was created.
        /// </summary>
        public double CreationMoment { get; set; }

        /// <summary>
        /// whether public/private/protected
        /// </summary>
        public string Access { get; set; }

        /// <summary>
        /// Version guid
        /// </summary>
        public string Guid { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
