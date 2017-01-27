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
    public interface IFileUpload
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Upload/{teamId}")]
        Message SaveFile(string teamId, Stream fileInfo);
    }

}
