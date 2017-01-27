using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.DataPersistence.Helpers
{
    /// <summary>
    /// Abstracted Content Type object
    /// hold primary info on the content type
    /// </summary>
    public class KPContentType
    {
        /// <summary>
        /// The CType GUID
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
    }
}
