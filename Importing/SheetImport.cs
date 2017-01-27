using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ServiceModel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.SharePoint;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.Classes.Importing;
using Amazon.Kingpin.WCF2.Classes.Importing.Utilities;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;
using Amazon.Kingpin.WCF2.Classes.Importing.Exceptions;
using System.ServiceModel.Web;
using Amazon.Kingpin.WCF2.Utilities;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Repositories;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Importing.Entities;
using Amazon.Kingpin.WCF2.Importing.Utilities;

namespace Amazon.Kingpin.WCF2.Classes
{
    public class SheetImport
    {
        private string DOCUMENT_UPLOAD_LIBRARY = "Shared Documents";

        // match html tags
        private Regex REGEX_HTML = new Regex("<.*?>", RegexOptions.Compiled);

        // TODO(mpace): need to make this dynamic
        private string listName;
        private string libraryName;
        //private int teamId;

        // TODO: remove this
        //private SPDataProvider spDataProvider = new SPDataProvider();
        private SPDataAccess spDataAccess;
        private DomainManager domainManager;

        private WorksheetUtilities worksheetUtilities;

        private Dictionary<string, SPList> lookupLists = new Dictionary<string, SPList>();

        /// <summary>
        /// Mapping keys used by templates. 
        /// Templates supported are enumerated as ImportType
        /// </summary>
        private string primaryKeysList;

        /// <summary>
        /// The name of the fields the values we care about are stored.
        /// The key is the lookup list name and the value is the field that stores the value
        /// TODO: remove the value before the colon - not needed 
        /// </summary>
        private Dictionary<string, string> LOOKUP_COLUMNS = new Dictionary<string, string>()
        {
            {"KPTeam","KPTeams:Nick"},
            {"GoalSet","KPGoalSets:Title"},
            {"KPPrimaryVP","KPVPs:Nick"},
            {"KPSecondaryVPs","KPVPs:Nick"},
            {"CategoryL1","KPCategoryL1:Title"},
            {"CategoryL2","KPCategoryL2:Title"},
        };

        /// <summary>
        /// the primary key that is used with the export 
        /// template so we don't write multiple records/items
        /// TODO(mpace): need to make this configurable        
        /// </summary>
        private Dictionary<string, string> EXPORT_KEYS_LIST = new Dictionary<string, string>() 
        {
            {"KPProjects","KPID"},
            {"KPGoals","KPID"},
            {"KeyInsightsInnovations","KPID"},
            {"AccomplishmentsMisses","KPID"},
            {"AuditItems","KPID"}
        };

        /// <summary>
        // the constructed primary key that is used so we don't write multiple records/items
        // the semi-colon separated list of fields that make up the key for each unique record.
        /// Template keys list supports importing new items so must compare a different
        /// set of keys than the export since export only supports updating
        // TODO(mpace): need to make this configurable
        /// </summary>
        private Dictionary<string, string> TEMPLATE_KEYS_LIST = new Dictionary<string, string>() 
        {
            {"KPProjects","KPExternalID"},
            {"KPGoals","Title;KPTeam"},
            {"KeyInsightsInnovations","KPDescription"},
            {"AccomplishmentsMisses","Title;KPTeam"},
            {"AuditItems","Title;KPTeam"}
        };

        /// <summary>
        /// Used for the dynamic templates - we don't necessarily have a mapping to KPID
        /// from randomly imported worksheets so we use a combination of fields to identify the 'unique' item
        /// </summary>
        private Dictionary<string, string> DYNAMIC_TEMPLATE_KEYS_LIST = new Dictionary<string, string>() 
        {
            {"KPProjects","Title;KPTeam"},
            {"KPGoals","Title;KPTeam"},
            {"KeyInsightsInnovations","KPDescription"},
            {"AccomplishmentsMisses","Title;KPTeam"},
            {"AuditItems","Title;KPTeam"}
        };

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="worksheetUtilities"></param>
        /// <param name="spDataAccess"></param>
        public SheetImport(WorksheetUtilities worksheetUtilities, SPDataAccess spDataAccess)
        {
            this.worksheetUtilities = worksheetUtilities;
            this.spDataAccess = spDataAccess;
            this.domainManager = new DomainManager();
            this.domainManager.SetCurrentUser();
            // sets default 'storage' library
            // uploaded documents are saved here by default
            this.libraryName = DOCUMENT_UPLOAD_LIBRARY;
        }

        /// <summary>
        /// Default Ctor
        /// Targets a single list for the entire import
        /// </summary>
        /// <param name="listName"></param>
        public SheetImport(string listName)
        {
            this.domainManager = new DomainManager();
            this.domainManager.SetCurrentUser();
            // sets default list name
            this.listName = listName;
            // sets default 'storage' library
            // uploaded documents are saved here by default
            this.libraryName = DOCUMENT_UPLOAD_LIBRARY;
        }

        /// <summary>
        /// Import data from either an exported file (from WCF) or a file supplied by the client
        /// **Client supplied files will need to include the subteam that is being targeted for import
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="teamId">Path to team site</param>
        /// <param name="fromExport"></param>
        /// <returns></returns>
        public ImportResponse ImportFileData(MappingData mappingData)
        {
            // library for uploads
            mappingData.LibraryName = DOCUMENT_UPLOAD_LIBRARY;

            this.listName = mappingData.ListName;

            this.worksheetUtilities.LoadFile(mappingData);

            // handle each template differently
            switch (mappingData.TemplateType)
            {
                    // goals export template
                case ImportType.Export:
                    mappingData.Mappings = TemplateMapping.Export;
                    this.primaryKeysList = this.EXPORT_KEYS_LIST[mappingData.ListName];
                    break;
                    // goals bulk import template
                case ImportType.Import:
                    mappingData.Mappings = TemplateMapping.Import;
                    this.primaryKeysList = this.TEMPLATE_KEYS_LIST[mappingData.ListName];
                    break;
                    // Current export template
                case ImportType.Current:
                    mappingData.Mappings = TemplateMapping.Current;
                    this.primaryKeysList = this.TEMPLATE_KEYS_LIST[mappingData.ListName];
                    break;
                    // generic worksheet from another source
                case ImportType.Dynamic:
                    this.primaryKeysList = this.DYNAMIC_TEMPLATE_KEYS_LIST[mappingData.ListName];
                    break;
            }

            return this.ImportData(mappingData);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        public ImportResponse ImportData(MappingData mappingData)
        {
            ImportStatus importStatus = new ImportStatus();

            Dictionary<string, string> listMappings = new Dictionary<string, string>();

            // initialize response
            ImportResponse response = new ImportResponse();
            response.Message = string.Empty;

            // extract property values
            Dictionary<string, string> mappings = mappingData.Mappings;
            string fileName = mappingData.FileName;
            string sheetName = mappingData.SheetName;

            int rowCount = 0;

            List<string> listColumns = new List<string>();
            List<int> duplicateRows = new List<int>();

            List<KPListItem> kpListItems = new List<KPListItem>();

            // datatable contains column definitions and rows
            DataTable dt = this.worksheetUtilities.GetDataTable(mappingData);

            // initialize lookup list collections
            this.domainManager.InitLookups();

            // initialize entity list
            // get entities by team and all sub-teams
            // if team is supplied then use this for all rows in sheet
            if(mappingData.TemplateType == ImportType.Export)
            {
                // these imports are for multiple teams so we need to get the team data
                List<int> teamIds = GetTeamIds(dt, mappingData);
                kpListItems = this.domainManager.GetUserEntities(mappingData.ListName, teamIds);
            }
            else
            {
                kpListItems = this.domainManager.GetUserEntities(mappingData.ListName, mappingData.TeamId.Value);
            }

            // get list columns from mapping
            //List<string> kpListColumns = kpListItems[0].Keys.ToList();
            List<string> kpListColumns = mappings.Keys.ToList();

            // ensure that we have a valid sheet template by comparing the sheet columns to our mapped data
            if (!ImportValidationUtilities.ValidateTemplateMapping(dt.Columns, mappingData))
            {
                // TODO: move this to front-end
                response.Message = "Oops. Looks like you've uploaded the wrong template.";
                response.StatusCode = 520;
                response.ImportStatus.Messages.Add("<div>If you want to update your data, please use the 'Export' feature.</div>");
                response.ImportStatus.Messages.Add(string.Format("<div>If you want to load new data, please use this <a href='{0}/Common/KPTemplates/2015_STeam_Goals_Template.xlsx'>Bulk Template</a> to bulk load your data.<br/>", this.spDataAccess.HostUrl));
                response.ImportStatus.Status = "error";
                return response;
            }

            // validate that the data in the sheet template contains the 
            // key fields required for import and duplicate checking
            // validate the mapping contains the required key fields (throws RequiredKeyFieldException)
            // TODO: this should be part of the ValidateTemplateMapping
            try
            {
                ImportValidationUtilities.ValidateRequiredKeyFields(mappings, this.primaryKeysList);
            }
            catch (RequiredKeyFieldException ex)
            {
                response.ImportStatus.Status = "error";
                response.Errors.Add(new ErrorObject(ex.Message, ex.StackTrace));
                response.Message = "Error: Required field exception";
                response.StatusCode = 520;
                return response;
            }

            // clean-up data from spreadsheet and convert to a KPListItem object
            // this will represent an entity object (e.g. Goal, Project, etc.)
            List<KPListItem> sheetData = this.ConvertSheetData(dt, kpListColumns, mappingData, importStatus);

            // checks for delimiters in values for Country & SecondaryVPs
            ImportValidationUtilities.EnforceDelimiters(sheetData);

            // validate the country and status values
            ImportValidationUtilities.ValidateNonLookupFields(mappingData.ListName, sheetData, importStatus);

            // copy original lookups for GoalSets
            List<Dictionary<string,string>> originalLookups = new List<Dictionary<string,string>>();

            // get the lookup values for the lookup fields
            this.TransformLookupFieldValues(originalLookups, sheetData, importStatus);

            // add each item
            foreach (KPListItem sheetItem in sheetData)
            {
                // initialize flag to indicate if item was actually updated
                bool isUpdated = false;

                // get the team object from the worksheet for updating or creating a new item
                Team userTeam = domainManager.User.Teams.Find(t => t.KPID == Int32.Parse(sheetItem["KPTeam"].Value));

                // find the item if it already exists
                int itemKPID = this.CheckForExistingItem(sheetItem, kpListItems, this.primaryKeysList);

                // if we successfully found all matching Category L1 & L2 for the specified Goal Set then continue else we'll skip this row
                if (ImportValidationUtilities.ValidateGoalSetCategories(this.domainManager, originalLookups[rowCount], sheetItem, importStatus))
                {
                    // skip this if no team found
                    if (userTeam != null)
                    {
                        // is this a create or update?
                        if (itemKPID > -1)
                        {
                            // update the item and check flag for results
                            this.spDataAccess.UpdateEntityItem(userTeam.SiteUrl, mappingData.ListName, sheetItem, itemKPID, out isUpdated);
                            if (isUpdated)
                            {
                                importStatus.UpdatedRowCount++;
                                importStatus.Messages.Add(string.Format("Row {0}: Updated, new field values found.", rowCount + 1));
                            }
                            else
                                importStatus.SkippedRow(string.Format("Row {0}: Not updated, no changes found.", rowCount + 1));
                        }
                        else
                        {
                            this.spDataAccess.AddNewEntityItem(userTeam.SiteUrl, mappingData.ListName, sheetItem);
                            importStatus.CreatedRowCount++;
                        }
                    }
                    else
                    {
                        string errMsg = string.Format("Could not add item. Team '{0}' not found for current user.", userTeam.Nick);
                        importStatus.Messages.Add(errMsg);
                        importStatus.SkippedRow(errMsg);
                    }
                }
                else
                {
                    importStatus.SkippedRowCount++;
                }

                rowCount++;
            }

            importStatus.TotalRowCount = rowCount;

            response.Message = ImportConstants.SUCCESS_IMPORT;

            response.ImportStatus = importStatus;


            // catch the following errors
            // skipped rows (what reason)?
            // skipped columns (we only care about these if the values are mismatched - GoalSet/CL1/CL2/VP) 
            // Duplicate data (keys match existing data)

            // message user
            // rows created
            // rows skipped - see above

            //    catch (UpdateOnlyTemplateException ex)
            //    {
            //        string ERR_ITEM_NOT_FOUND = "Error: Item not found to update (KPID:{0})";
            //        string err = string.Format(ERR_ITEM_NOT_FOUND, ex.Message);
            //        // add import exception errors for exceptions updating column values
            //        //importErrors.Add(new ErrorObject(err, string.Empty));
            //        //response.StatusCode = 206;  // error
            //    }
            //    catch (ColumnUpdateException ex)
            //    {
            //        // add import exception errors for exceptions updating column values
            //        //importErrors.Add(new ErrorObject(ex.Message, ex.StackTrace));
            //    }
            //    catch (Exception ex)
            //    {
            //        // add validation errors
            //        //importErrors.Add(new ErrorObject(string.Format(ERR_ROW_MSG, rowIndex), ex.StackTrace));
            //        // error importing rows
            //        //response.StatusCode = 206;  // error
            //    }
            //}


            //if (response.StatusCode < 400)
            //{
            //    // TODO: refactor this - total mess
            //    switch (mappingData.ImportType)
            //    {
            //        case ImportType.Export:
            //            // successfully updated data from sheet
            //            if (rowIndex >= importCount)
            //            {
            //                response.Message = string.Format(ImportConstants.SUCCESS_IMPORT);
            //                response.Message += string.Format("Updated {0} of {1} rows from sheet.<br/>", importCount, rowIndex);
            //            }
            //            if (rowIndex > importCount)
            //            {
            //                // show skipped rows
            //                if (duplicateRows.Count > 0)
            //                    response.Message += string.Format(ImportConstants.SUCCESS_SKIPPED_ROWS, duplicateRows.Count, string.Join(",", duplicateRows.ToArray()));
            //            }
            //            if (_versionConflict.Count > 0)
            //            {
            //                response.Message += string.Format("{0} Version Conflicts found:<br/>", _versionConflict.Count);
            //                response.Message += string.Join("<br/>", _versionConflict.ToArray());
            //                response.Message += "<br/>Please export the data and try again.<br/>";
            //            }
            //            if (_mismatchedLookups.Count > 0)
            //            {
            //                response.Message += string.Format("{0} Mismatched Lookup Values found:<br/>{1}", _mismatchedLookups.Count, string.Join("<br/>", _mismatchedLookups.ToArray()));
            //                response.StatusCode = 205;  // warning
            //            }
            //            break;
            //        case ImportType.Dynamic:
            //        case ImportType.Template:
            //            // successfully imported data from sheet
            //            if (rowIndex >= importCount)
            //            {
            //                response.Message = string.Format(ImportConstants.SUCCESS_BULK_IMPORT);
            //                response.Message += string.Format("Created {0} of {1} rows from sheet.<br/>", importCount, rowIndex);
            //            }
            //            if (columnsMissing.Count > 0)
            //            {
            //                response.Message += "Bulk import successful ... but columns not found";
            //                response.Errors = columnsMissing;
            //                response.StatusCode = 205;  // warning
            //            }
            //            if (_mismatchedLookups.Count > 0)
            //            {
            //                response.Message += string.Format("{0} Mismatched Lookup Values found:<br/>{1}", _mismatchedLookups.Count, string.Join("<br/>", _mismatchedLookups.ToArray()));
            //                response.StatusCode = 205;  // warning
            //            }
            //            if (_incorrectLookups.Count > 0)
            //            {
            //                // show incorrect values mapped
            //                response.Message += string.Format("{0} Incorrect Lookup Values found:<br/>{1}", _incorrectLookups.Count, string.Join("<br/>", _incorrectLookups.ToArray()));
            //                response.Message += "<br/>Incorrect values were removed.";
            //                response.Message += "<br/>Please refer to Kingpin for the correct values to update.";
            //                response.StatusCode = 205;  // warning
            //            }
            //            break;
            //    }
            //}
            //else
            //{
            //    // an exception was thrown.
            //    response.Errors = importErrors;
            //}

            return response;
        }

        /// <summary>
        /// Get the list of team id's from the data table rows
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        private List<int> GetTeamIds(DataTable dt, MappingData mappingData)
        {
            // the column will be KPTeam
            string mappedColumn = mappingData.Mappings["KPTeam"];
            // the list of id's to return
            List<int> teamIds = new List<int>();
            foreach(DataRow row in dt.Rows)
            {
                string teamName = row[mappedColumn].ToString();
                Team team = this.domainManager.User.Teams.Find(t => t.Nick == teamName);
                // only want to add them once
                if (team != null && !teamIds.Contains(team.KPID))
                {
                    teamIds.Add(team.KPID);
                }
            }
            return teamIds;
        }

        /// <summary>
        /// Checks for existing items by checking the assigned primary key values
        /// </summary>
        /// <param name="sheetItem"></param>
        /// <param name="kpListItems"></param>
        /// <param name="primaryKeyValue"></param>
        /// <returns></returns>
        private int CheckForExistingItem(KPListItem sheetItem, List<KPListItem> kpListItems, string primaryKeyValue)
        {
            bool foundMatch = false;
            int itemKPID = -1;
            string[] primaryKeys = primaryKeyValue.Split(';');
            
            // check sheet for value if it has one
            if (sheetItem.ContainsKey("KPID") && !string.IsNullOrEmpty(sheetItem["KPID"].Value))
            {
                // we have a value, but it may be bogus
                // so we set the foundMatch flag to double check
                itemKPID = Int32.Parse(sheetItem["KPID"].Value);
                primaryKeys = new string[] { "KPID" };
            }

            if (sheetItem.ContainsKey("KPExternalID") && !string.IsNullOrEmpty(sheetItem["KPExternalID"].Value))
            {
                primaryKeys = new string[] { "KPExternalID" };                
            }

            if(primaryKeys.Length > 1)
            {
                foreach (KPListItem kpListItem in kpListItems)
                {
                    if ((sheetItem[primaryKeys[0]].Value == kpListItem[primaryKeys[0]].Value) && (sheetItem[primaryKeys[1]].Value == kpListItem[primaryKeys[1]].Value))
                    {
                        itemKPID = int.Parse(kpListItem["KPID"].Value);
                        foundMatch = true;
                    }
                }
            } 
            else
            {
                // check if we have the primary key to lookup
                if (sheetItem.ContainsKey(primaryKeys[0]))
                {
                    foreach (KPListItem kpListItem in kpListItems)
                    {
                        if (sheetItem[primaryKeys[0]].Value == kpListItem[primaryKeys[0]].Value)
                        {
                            itemKPID = int.Parse(kpListItem["KPID"].Value);
                            foundMatch = true;
                        }
                    }
                }
            }

            // check if we found a match
            if (foundMatch)
                return itemKPID;
            else
                return -1;
        }

        /// <summary>
        /// Converts the imported SheetData into KPListItem objects for importing into SP
        /// The KPListItem is used so we don't need to use the strongly typed objects which 
        /// is problematic since we want to be able to dynamically support any of the entities.
        /// TODO: consider adding a generic type T to allow strongly-typed objects to be used.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="kpListColumns"></param>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        private List<KPListItem> ConvertSheetData(DataTable dt, List<string> kpListColumns, MappingData mappingData, ImportStatus importStatus)
        {
            string MAPPING_DEFAULT_DELIMITER = "#";
            // need a site url regardless to get an entity item to get its fields
            // we can use the assigned team, but for 'Export' templates we don't 
            // have a team so we'll need to use any team the user has access to
            List<KPListItem> listItems = new List<KPListItem>();

            // get the list column names and data type
            Dictionary<string, string> fieldInfo = this.spDataAccess.GetEntityFields(mappingData.Team.SiteUrl, mappingData.ListName);
            int rowCount = 0;

            try 
            {
                // enumerate the rows of the data table
                foreach (DataRow row in dt.Rows)
                {
                    // create a new item - which represents a row/item/entity
                    KPListItem item = new KPListItem();
                    // convert each row into a KPListItem
                    foreach (KeyValuePair<string, string> kvPair in mappingData.Mappings)
                    {
                        string value = string.Empty;
                        // mapping.Key is the list column name
                        string listColumn = kvPair.Key;
                        // mapping.Value is the sheet column name
                        string sheetColumn = kvPair.Value;
                        // get the type of field
                        string fieldType = fieldInfo[listColumn];

                        // does the mapped column contain a default value?
                        // #-prefix flags default values
                        if (kvPair.Value.Contains(MAPPING_DEFAULT_DELIMITER))
                        {
                            // use the mapped value - remove the token
                            value = sheetColumn.Remove(0, 1);
                        }
                        else
                        {
                            // check the worksheet row[column] for the mapped column
                            value = row[sheetColumn].ToString();
                        }
                        // create the field with value and data type
                        if (!string.IsNullOrEmpty(value))
                        {
                            value = KPUtilities.ConvertToSPType(value, fieldType);

                            // if value is null it's an invalid date
                            if (value == null)
                                importStatus.SkippedColumns.Add(string.Format("{0}: [Invalid Date]", sheetColumn));

                            item.Add(listColumn, new KPItem(value, fieldType));
                        }

                    }
                    // ensure we have columns - but if we have '1' then it's KPTeam which doesn't matter
                    // because KPTeam is included in all items by default - so effectively we have no row data
                    if(importStatus.SkippedColumns.Count < mappingData.Mappings.Count && item.Count != 1)
                    {
                        listItems.Add(item);
                        rowCount++;
                    }
                    else
                    {
                        // we have an empty row - ignore this row, but add to skippedRows
                        if (item.Count != 1)
                        {
                            importStatus.SkippedRowCount++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string errMsg = "Error converting sheet data (ConvertSheetData). Exception: {0}; StackTrace: {1}";
                importStatus.Messages.Add(string.Format(errMsg, ex.Message, ex.StackTrace));
            }

            return listItems;
        }

        #region PRIVATE HELPER METHODS
        /// <summary>
        /// Transforms the string value to the KPID value (lookup)
        /// using the LookupColumns definitions and getting the values
        /// from the Lookup Lists in the domain manager
        /// </summary>
        /// <param name="listItems"></param>
        private void TransformLookupFieldValues(List<Dictionary<string, string>> originalLookups, List<KPListItem> listItems, ImportStatus importErrors)
        {
            string INVALID_LOOKUP = "You have an invalid lookup value: \"{0}\" for Field Name: \"{1}\" skipped this field.";
            foreach (KPListItem listItem in listItems)
            {
                // new lookup dictionary object
                Dictionary<string, string> originalLookup = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> lookupColumn in this.LOOKUP_COLUMNS)
                {
                    string value = string.Empty;
                    string fieldName = lookupColumn.Key;
                    if (listItem.Keys.Contains(fieldName))
                    {
                        // get lookup column value
                        string[] values = lookupColumn.Value.Split(':');
                        string fieldValue = listItem[fieldName].Value;
                        string listName = values[0];
                        string lookupField = values[1];

                        // contains the lookup field name and original value
                        originalLookup.Add(lookupColumn.Key, fieldValue);

                        // KPVPs is a delimited list of VPs
                        if (listName == "KPVPs")
                        {
                            List<string> multiValues = new List<string>();
                            // we know we have semicolons because we replaced those earlier
                            string[] vpValues = fieldValue.Split(';');
                            // go through each item and get its lookup value
                            foreach (string delimitedValue in vpValues)
                            {
                                string lookup = this.domainManager.GetLookupValue(fieldName, delimitedValue, lookupField);
                                if(!string.IsNullOrEmpty(lookup))
                                    multiValues.Add(lookup);
                                else
                                {
                                    importErrors.Status = "warning";
                                    importErrors.Messages.Add(string.Format(INVALID_LOOKUP, delimitedValue, fieldName));
                                    importErrors.SkippedColumns.Add(string.Format("{0}: [Invalid]", fieldName));
                                }
                            }
                            // now join them back with the semicolon delimiter
                            value = string.Join(";", multiValues);
                        }
                        else
                        {
                            value = this.domainManager.GetLookupValue(fieldName, fieldValue, lookupField);

                            if (string.IsNullOrEmpty(value))
                            {
                                importErrors.Status = "warning";
                                importErrors.Messages.Add(string.Format(INVALID_LOOKUP, fieldValue, fieldName));
                                importErrors.SkippedColumns.Add(string.Format("{0}: [Invalid]", fieldName));
                            }
                        }

                        listItem[fieldName].Value = value;
                    }
                }
                // add the dictionary of lookup fieldnames and values 
                // for error reporting in EnforceGoalSetCategories
                originalLookups.Add(originalLookup);
            }
        }

        #endregion

        /// <summary>
        /// TODO(mpace): move this to classes/entities
        /// This object is serialized into JSON for the
        /// UI to display the list of dropdowns allowing 
        /// the user to map list columns to spreadsheet columns
        /// </summary>
        internal class ColumnMapping
        {
            public string FieldName { get; set; }
            public List<string> SheetColumns { get; set; }
            public List<string> Default { get; set; }
            public ColumnMapping()
            {
                this.SheetColumns = new List<string>();
            }
        }
    }
}
