using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{

    [DataContract(Name = "KPItemVersion")]
    public class KPItemVersion
    {
        [DataMember(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
        [DataMember(Name = "Modified")]
        public string Modified { get; set; }
        [DataMember(Name = "Number")]
        public int Number { get; set; }
        [DataMember(Name = "Fields")]
        [JsonConverter(typeof(DataContractJsonSerializer))]
        public Dictionary<string, string> Fields { get; set; }

        public KPItemVersion()
        {
            this.Fields = new Dictionary<string, string>();
        }
    }
}
