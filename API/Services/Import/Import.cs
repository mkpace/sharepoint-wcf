using System;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using Microsoft.SharePoint.Client.Services;
using Amazon.Kingpin.WCF2.Classes;
using Microsoft.SharePoint.Client;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;
using Amazon.Kingpin.WCF2.Classes.Importing.Exceptions;
using Amazon.Kingpin.WCF2.Utilities;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.Classes.Importing.Utilities;
using Amazon.Kingpin.WCF2.Classes.Importing;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Importing.Entities;
using Amazon.Kingpin.WCF2.Data.Access;
using System.IO;
using Amazon.Kingpin.WCF2.API.Interfaces;
using System.ServiceModel.Channels;
using Amazon.Kingpin.WCF2.Http;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using Newtonsoft.Json;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Importing.Utilities;

namespace Amazon.Kingpin.WCF2.API.Services.Import
{
    [ClientRequestServiceBehavior]
    [BasicHttpBindingServiceMetadataExchangeEndpointAttribute]
    [AspNetCompatibilityRequirementsAttribute(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [DataContractAttribute]
    public class Import : IImport
    {
        private WebOperationContext ctx = WebOperationContext.Current;

        /// <summary>
        /// Get list of worksheets in the specified file
        /// </summary>
        /// <param name="fileName"></param>
        public Message GetSheets(string listName, string fileName)
        {
            Message responseMsg = null;
            SPDataAccess spDataAccess = new SPDataAccess();
            WorksheetUtilities worksheetUtilities = new WorksheetUtilities(new SPDataAccess());
            List<string> worksheets = worksheetUtilities.GetWorksheetNames(listName, fileName);
            responseMsg = ctx.CreateJsonResponse<List<string>>(worksheets);
            return responseMsg;
        }

        /// <summary>
        /// Get the column definitions for the specified list
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        public Message GetItemColumnDefinitions(string listName)
        {
            Message responseMsg = null;
            ImportUtilities importUtility = new ImportUtilities();
            SPDataAccess spDataAccess = new SPDataAccess();
            List<ColumnDefinition> fields = importUtility.GetEntityColumnDefaultValues(spDataAccess, listName);
            responseMsg = ctx.CreateJsonResponse<List<ColumnDefinition>>(fields);

            return responseMsg;
        }

        /// <summary>
        /// Gets the mappings for the specified entity and worksheet
        /// </summary>
        /// <returns></returns>
        public Message GetEntityMappings()
        {
            Message responseMsg = null;
            ImportUtilities importUtility = new ImportUtilities();
            SPDataAccess spDataAccess = new SPDataAccess();
            MappedColumns mappedColumns = new MappedColumns();

            WorksheetUtilities worksheetUtilities = new WorksheetUtilities(spDataAccess);

            MappingData mappingData = this.ValidateRequest(spDataAccess, false);
            
            // get the columns from the supplied worksheet
            mappedColumns.WSColumns = worksheetUtilities.GetWorksheetColumns(mappingData);

            // checks to see if the columns pulled from sheet map to
            // a predefined template e.g. Bulk Import or Goals Export
            mappedColumns.TemplateType = ImportUtilities.GetImportTemplateType(mappedColumns.WSColumns);          

            // get the entity columns with the default values for choice columns
            mappedColumns.ColumnDefinitions = importUtility.GetEntityColumnDefaultValues(spDataAccess, mappingData.ListName);

            responseMsg = ctx.CreateJsonResponse<MappedColumns>(mappedColumns);

            return responseMsg;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// public ImportResponse ImportData(string listName, string fileName, string worksheet, string importType, string teamId)
        public ImportResponse ImportData()
        {
            // get the web context for the response
            WebOperationContext ctx = WebOperationContext.Current;

            // set default status to 200-OK
            ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;

            // initialize objects
            ImportResponse response = new ImportResponse();
            SPDataAccess spDataAccess = new SPDataAccess();
            
            // create an instance of the worksheet and initialize SheetImport
            SheetImport import = new SheetImport(new WorksheetUtilities(spDataAccess), spDataAccess);
            try
            {
                MappingData mappingData = this.ValidateRequest(spDataAccess, true);

                // import the file data using the uploaded file as input
                response = import.ImportFileData(mappingData);

                // check response status code - set to error if not 200
                if(response.StatusCode != 200)
                {
                    ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.StatusCode = 520;
                response.Message = "Error in Import.ImportData";
                response.Errors = new List<ErrorObject>();
                response.Errors.Add(new ErrorObject(ex.Message, ex.StackTrace));
            }

            return response;
        }

        /// <summary>
        /// Validate the import mapping request
        /// </summary>
        /// <param name="spDataAccess"></param>
        /// <param name="isImport"></param>
        /// <returns></returns>
        private MappingData ValidateRequest(SPDataAccess spDataAccess, bool isImport)
        {
            // get the message (body)
            Message message = OperationContext.Current.RequestContext.RequestMessage;

            ImportResponse response = new ImportResponse();
            MappingData mappingData = null;
            string jsonData = string.Empty;

            // get the convert the body text
            jsonData = JSON.GetPayload(message);

            // deserialize the JSON body
            mappingData = JsonConvert.DeserializeObject<MappingData>(jsonData);

            if (string.IsNullOrEmpty(mappingData.ListName))
                throw new Exception("List name is required (listName)");

            if (string.IsNullOrEmpty(mappingData.FileName))
                throw new Exception("File name is required (fileName)");

            if (mappingData.TemplateType == null && isImport)
                throw new Exception("Template type is required (templateType)");

            if (mappingData.TemplateType.Equals(ImportType.Import) && mappingData.TeamId < 0)
            {
                throw new Exception("TeamId is required for Import templates");
            }

            // initialize user to get their teams
            spDataAccess.InitializeCurrentUser();

            // if we have a dynamic template then we need to set the
            // team to what was passed in and add the required field to the mappings
            if(mappingData.TemplateType.Equals(ImportType.Dynamic))
            {
                // get team
                mappingData.Team = spDataAccess.CurrentUser.Teams.Find(t => t.KPID == mappingData.TeamId);
                // create the mapping value with the delimiter
                mappingData.Mappings.Add("KPTeam", string.Format("#{0}", mappingData.Team.Nick));
            }
            else
            {
                if (mappingData.TeamId.HasValue && mappingData.TeamId > -1)
                {
                    Team team = spDataAccess.CurrentUser.Teams.Find(t => t.KPID == mappingData.TeamId);

                    if (team != null)
                        mappingData.Team = team;
                    else
                        throw new Exception(string.Format("Error Validating: TeamId {0} does not exist.", mappingData.TeamId));
                }
                else
                {
                    if (spDataAccess.CurrentUser.Teams.Count > 0)
                        mappingData.Team = spDataAccess.CurrentUser.Teams[0];
                    else
                        throw new Exception(string.Format("Error Validating: User does not have any Teams assigned."));
                }
            }

            return mappingData;
        }

    }
}
