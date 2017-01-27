using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.API.Services.Data;

namespace Amazon.Kingpin.WCF2.API.Interfaces
{
    [ServiceContract]
    public interface IFile
    {
        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{teamId}")]
        //Message Upload(string teamId, Stream fileInfo);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{teamId}/{fileName}")]
        Message Download(string teamId, string fileName);

    }

}
