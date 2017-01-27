using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Exceptions
{
    [DataContractAttribute(Name = "errorObject")]
    public class ErrorObject
    {
        [DataMember(Name = "message")]
        public string errorMsg { get; set; }
        [DataMember(Name = "stackTrace")]
        public string stackTrace { get; set; }

        public ErrorObject(string errorMsg, string stackTrace)
        {
            this.errorMsg = errorMsg;
            this.stackTrace = stackTrace.Trim();
        }
    }
}
