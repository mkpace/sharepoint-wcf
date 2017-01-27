using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace Amazon.Kingpin.WCF2.API.Interfaces
{
    [ServiceContract]
    public interface IDataEntity
    {
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{entityName}")]
        Message EntityGET(string entityName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{entityName}")]
        Message EntityPOST(string entityName);

        [OperationContract]
        [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{entityName}")]
        Message EntityPUT(string entityName);

        [OperationContract]
        [WebInvoke(Method = "DELETE", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{entityName}")]
        Message EntityDELETE(string entityName);
    }
}
