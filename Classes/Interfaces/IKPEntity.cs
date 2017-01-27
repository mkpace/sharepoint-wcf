using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Base;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    /// <summary>
    /// Interface for all BaseEntity and BaseItems
    /// </summary>
    public interface IKPEntity : IKPItem
    {
        /// <summary>
        /// TeamId from Teams list
        /// </summary>
        int? KPTeamId { get; set; }
    }
}
