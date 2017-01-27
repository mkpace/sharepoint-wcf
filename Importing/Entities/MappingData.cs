using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Entities
{
    [DataContract(Name = "mappingData")]
    public class MappingData
    {
        [DataMember(Name = "libraryName")]
        public string LibraryName { get; set; }
        [DataMember(Name = "listName")]
        public string ListName { get; set; }
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }
        [DataMember(Name = "worksheet")]
        public string SheetName { get; set; }
        [DataMember(Name = "templateType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ImportType? TemplateType { get; set; }
        [DataMember(Name = "teamId")]
        public int? TeamId { get; set; }
        [DataMember(Name = "mappings")]
        public Dictionary<string, string> Mappings { get; set; }

        public Team Team { get; set; }

    }
}
