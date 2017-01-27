using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using System.ServiceModel.Channels;
using System.IO;

namespace Amazon.Kingpin.WCF2.API.Interfaces
{
    [ServiceContract]
    public interface IData
    {
        /*****************************************
         *  ENTITY API
         ****************************************/
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entities/{entityNames}?teamids={teamIds}")]
        Message EntitiesByNameByTeamGET(string entityNames, string teamIds);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/Cache/{entityName}/{state}")]
        Message EntityCacheEnableGET(string entityName, string state);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityName}")]
        Message EntityByNameGET(string entityName);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityName}/{entityKPID}")]
        Message EntityByIdGET(string entityName, string entityKPID);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityName}/Teams/{teamIds}")]
        Message EntityByTeamGET(string entityName, string teamIds);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/Versions/{entityName}/{entityId}")]
        Message EntityVersionsGET(string entityName, string entityId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityName}")]
        Message EntityPOST(string entityName);

        [OperationContract]
        [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityName}/{entityId}")]
        Message EntityPUT(string entityName, string entityId);

        ////[OperationContract]
        ////[WebInvoke(Method = "DELETE", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Entity/{entityId}")]
        ////Message EntityDELETE(string entityId);

        /*****************************************
         *  FILE API
         ****************************************/
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Document/{teamId}/{fileName}")]
        Message FileByNameGET(string teamId, string fileName);

        /*****************************************
         *  LOOKUP API
         ****************************************/
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}")]
        Message LookupGET(string entityName);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}/{count}")]
        Message LookupMaxGET(string entityName, string count);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}")]
        Message LookupPOST(string entityName);

        [OperationContract]
        [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityName}/{entityId}")]
        Message LookupPUT(string entityName, string entityId);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/GetAllChildren/{parentID}")]
        Message LookupAllChildrenGET(string parentID);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/UserConfig/{userAlias}")]
        Message LookupUserConfigGET(string userAlias);

        //[OperationContract]
        //[WebInvoke(Method = "DELETE", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Lookup/{entityId}")]
        //Message LookupDELETE(string entityName);

        /*****************************************
         *  SECURITY API
         ****************************************/
        //[OperationContract]
        //[WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/User")]
        //Message UserProfileGET(string entityName);

    }
}
