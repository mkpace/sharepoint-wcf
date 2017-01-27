using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Amazon.Kingpin.WCF2.Classes.Importing;

namespace Amazon.Kingpin.WCF2.Classes
{
    [DataContract(Name = "mappedColumns")]
    public class MappedColumns
    {
        /// <summary>
        /// sharepoint columns
        /// </summary>
        [DataMember(Name = "columnDefinitions")]
        public List<ColumnDefinition> ColumnDefinitions { get; set; }

        /// <summary>
        /// worksheet columns
        /// </summary>
        [DataMember(Name = "wsColumns")]
        public List<string> WSColumns { get; set; }

        [DataMember(Name = "statusMessage")]
        public string StatusMessage { get; set; }

        [DataMember(Name = "templateType")]
        public ImportType? TemplateType { get; set; }

        /// <summary>
        /// <summary>
        /// Default Ctor: needed for serialization / DataContract
        /// </summary>
        /// </summary>
        public MappedColumns() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spColumns"></param>
        public MappedColumns(List<string> wsColumns) 
        {
            this.ColumnDefinitions = new List<ColumnDefinition>();
            this.WSColumns = wsColumns;
        }
    }
}
