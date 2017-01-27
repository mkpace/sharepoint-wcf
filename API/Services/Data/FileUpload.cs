using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.API.Interfaces;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Diagnostics;
using Amazon.Kingpin.WCF2.Http;
using Microsoft.SharePoint.Client.Services;

namespace Amazon.Kingpin.WCF2.API.Services.Data
{
    //[BasicHttpBindingServiceMetadataExchangeEndpointAttribute]
    [AspNetCompatibilityRequirementsAttribute(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [DataContractAttribute]
    public class FileUpload : IFileUpload
    {
        protected WebOperationContext ctx = WebOperationContext.Current;

        public Message GetResponse(string data)
        {
            return this.ctx.CreateJsonResponse<string>(string.Format("Response: {0}", data));
        }

        /// <summary>
        /// Save uploaded file to SP document library
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public Message SaveFile(string teamId, Stream fileStream)
        {
            string response = string.Empty;
            int teamID = Int16.Parse(teamId);
            string fileGuid = Guid.NewGuid().ToString() + ".docx";
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            try
            {
                KPTimer timer = EventLogger.Timer;
                MultipartParser parser = new MultipartParser(fileStream);
                if (parser.Success)
                {
                    // save the file
                    SPDataAccess spDataAccess = new SPDataAccess();
                    spDataAccess.SaveFileToLibrary(teamID, fileGuid, parser.FileContents);
                    // generate response
                    responseData.Add("FileName", fileGuid);
                    responseData.Add("Elapsed", timer.ElapsedMilliseconds().ToString());
                    responseData.Add("Bytes", parser.FileContents.Length.ToString());
                }
                else
                {
                    string errMsg = string.Empty;
                    ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    responseData.Add("Error", "Error parsing filestream");
                    if (parser.FileContents == null)
                        responseData.Add("FileContents", "no file content sent in request.");
                    if(parser.ContentType == null)
                        responseData.Add("ContentType", "no file content-type found in request.");
                }
            }
            catch (Exception ex)
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                responseData.Add("Exception", ex.Message);
                if (ex.InnerException != null)
                    responseData.Add("InnerException", ex.InnerException.Message);
            }

            return this.ctx.CreateJsonResponse<Dictionary<string,string>>(responseData, serializer);
        }
    }
}
