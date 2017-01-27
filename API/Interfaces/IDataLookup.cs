using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace Amazon.Kingpin.WCF2.API.Interfaces
{
    [ServiceContract]
    public interface IDataLookup
    {
        /*****************************************
         *  LOOKUP API
         ****************************************/
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}")]
        Message LookupGET(string entityName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}")]
        Message LookupPOST(string entityName);

        [OperationContract]
        [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}/{entityId}")]
        Message LookupPUT(string entityName, string entityId);

        /*****************************************
         *  DIAGNOSTICS/LOGGING API
         ****************************************/
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Logging/{entityName}")]
        Message DiagnosticsGET(string entityName);
    }
}
