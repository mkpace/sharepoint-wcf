using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing
{
    public enum ImportType
    {
        //[EnumMember(Name = "Import")]
        Import,     // from a pre-defined template
        //[EnumMember(Value = "Export")]
        Export,     // from an exported template
        //[EnumMember(Value = "Current")]
        Current,    // from Current export template
        //[EnumMember(Value = "Dynamic")]
        Dynamic     // takes in dynamic mapping
    }
}
