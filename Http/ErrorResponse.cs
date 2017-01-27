using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Importing.Exceptions;
using Amazon.Kingpin.WCF2.Importing.Entities;

namespace Amazon.Kingpin.WCF2.Classes.Reporting
{
    [DataContractAttribute(Name = "exceptionResponse")]
    public class ErrorResponse
    {
        [DataMember(Name = "httpAction")]
        public string HttpAction { get; set; }
        [DataMember(Name = "exception")]
        public string Exception { get; set; }
        [DataMember(Name = "innerException")]
        public string InnerException { get; set; }

        public ErrorResponse(Exception ex, string httpAction)
        {
            this.HttpAction = httpAction;
            this.Exception = ex.Message;
            this.InnerException = (ex.InnerException != null) ? ex.InnerException.Message : null;
        }
    }

    [DataContractAttribute(Name = "importResponse")]
    public class ImportResponse
    {
        [DataMember(Name = "statusCode")]
        public int StatusCode { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "importStatus")]
        public ImportStatus ImportStatus { get; set; }
        [DataMember(Name = "errors")]
        public List<ErrorObject> Errors { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }

        public ImportResponse()
        {
            this.StatusCode = 200;
            this.Message = "OK";
            this.ImportStatus = new ImportStatus();
            this.Errors = new List<ErrorObject>();
        }
    }
}
