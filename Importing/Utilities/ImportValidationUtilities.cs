using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Importing;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Importing.Entities;
using Amazon.Kingpin.WCF2.Repositories;

namespace Amazon.Kingpin.WCF2.Importing.Utilities
{
    public class ImportValidationUtilities
    {
        // match either / or , or ;
        public static Regex REGEX_DELIMITER_MATCH = new Regex(@"(/|,|;|\|)", RegexOptions.Compiled);

        public static Regex REGEX_WHITESPACE = new Regex(@"(\s)", RegexOptions.Compiled);

        public static List<string> DELIMITED_FIELDS = new List<string>() { "Country", "KPSecondaryVPs" };

        /// <summary>
        /// Validates the specified template with the corresponding mapping.
        /// TODO: should do this transparently to the user - check both template types
        /// and automatically set the correct template type.
        /// </summary>
        /// <param name="dtColumns"></param>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        public static bool ValidateTemplateMapping(DataColumnCollection dtColumns, MappingData mappingData)
        {
            string ERR_REQUIRED_COLUMNS_MISSING = "{0} Column{1} {2} missing.";
            string PLURAL = "s";
            string IS = "is";
            string ARE = "are";
            string errMsg = string.Empty;

            List<string> UPLOAD_TEMPLATE_REQUIRED_FIELDS = new List<string>() { "Team", "ID" };
            List<string> missingColumns = new List<string>();
            int columnsFound = 0;

            foreach (KeyValuePair<string, string> kvPair in mappingData.Mappings)
            {
                if (dtColumns.Contains(kvPair.Value))
                    columnsFound++;
                else
                    missingColumns.Add(kvPair.Value);
            }
            if (mappingData.TemplateType.Equals("Export") && missingColumns.Count > 0)
            {
                List<string> requiredColumns = missingColumns.FindAll(c => UPLOAD_TEMPLATE_REQUIRED_FIELDS.Contains(c));
                if (requiredColumns.Count > 0)
                {
                    if (missingColumns.Count > 1)
                        errMsg = string.Format(ERR_REQUIRED_COLUMNS_MISSING, string.Join(", ", requiredColumns), PLURAL, ARE);
                    else
                        errMsg = string.Format(ERR_REQUIRED_COLUMNS_MISSING, string.Join(", ", requiredColumns), string.Empty, IS);

                    throw new RequiredKeyFieldException(errMsg);
                }
            }
            // calculate percentage of columns found - arbitrary at 87%
            //double pctFound = Convert.ToInt16(Convert.ToDouble(columnsFound) / Convert.ToDouble(mappingData.Mappings.Count) * 100);
            // found more than 87% of the specified columns
            //return (pctFound > 87);
            return true;
        }

        /// <summary>
        /// Verifies that the current mappings list contain the key fields
        /// necessary to find unique records in each item.
        /// This method throws since we shouldn't continue if unique key mappings are missing
        /// </summary>
        /// <param name="mappings"></param>
        public static void ValidateRequiredKeyFields(Dictionary<string, string> mappings, string listPrimaryKey)
        {
            List<string> keyColumns = null;
            keyColumns = listPrimaryKey.Split(new char[] { ';' }).ToList();

            foreach (string key in keyColumns)
            {
                // throw here if we don't have any of the required keys
                if (!mappings.ContainsKey(key))
                {
                    throw new RequiredKeyFieldException(string.Format(ImportConstants.ERR_MAPPING, key));
                }
            }
        }

        /// <summary>
        /// Enforces the expected delimiters 
        /// used by KP code ';' rather than ','
        /// Currently Country is the only column that this is necessary?
        /// </summary>
        /// <param name="sheetData"></param>
        public static void EnforceDelimiters(List<KPListItem> sheetData)
        {
            foreach (KPListItem item in sheetData)
            {
                foreach (string field in DELIMITED_FIELDS)
                {
                    // check for delimited columns
                    if (item.ContainsKey(field))
                    {
                        // do we have delimiters in the string
                        if (ImportValidationUtilities.REGEX_DELIMITER_MATCH.Match(item[field].Value).Length > 0)
                        {
                            // we need to get that delimiter - we account for various ones
                            char delimiter = REGEX_DELIMITER_MATCH.Match(item[field].Value).Groups[0].Value.ToCharArray()[0];
                            // we use the semicolon ';' as the delimeter in our field data - replace with that
                            string values = item[field].Value.Replace(delimiter, ';');
                            // we'll also remove any whitespace while we're at it
                            item[field].Value = REGEX_WHITESPACE.Replace(values, string.Empty);
                        }
                        else
                        {
                            item[field].Value = REGEX_WHITESPACE.Replace(item[field].Value, string.Empty);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Verify values for Countries, Status, and State are valid since
        /// these field values don't have a central lookup list
        /// </summary>
        /// <param name="sheetData"></param>
        /// <param name="importStatus"></param>
        internal static void ValidateNonLookupFields(string listName, List<KPListItem> sheetData, ImportStatus importStatus)
        {
            string SKIPPED_COUNTRY_ADD = "Row: {0} value (\"{1}\") for field Country not found. No value set.";
            string SKIPPED_STATUS_ADD = "Row: {0} value (\"{1}\") for field Status not found. Status set to \"TBD\".";
            string SKIPPED_STATE_ADD = "Row: {0} value (\"{1}\") for field ItemState not found. State set to \"Active\".";

            List<string> foundCountries = new List<string>();
            int rowIndex = 1;
            foreach(KPListItem listItem in sheetData)
            {
                if(listItem.ContainsKey("Country") && !string.IsNullOrEmpty(listItem["Country"].Value))
                {
                    string[] countries = listItem["Country"].Value.Split(';');
                    foreach(string country in countries)
                    {
                        string foundCountry = ImportUtilities.COUNTRIES.Find(c => c == country);
                        if (string.IsNullOrEmpty(foundCountry))
                        {
                            importStatus.Messages.Add(string.Format(SKIPPED_COUNTRY_ADD, rowIndex, country));
                        }
                        else
                        {
                            foundCountries.Add(foundCountry);
                        }
                    }
                    // only add the countries that were found
                    listItem["Country"].Value = string.Join(";", foundCountries);

                }   
                
                // validate status value                
                if(listItem.ContainsKey("KPStatus") && !string.IsNullOrEmpty(listItem["KPStatus"].Value))
                {
                    string kpStatus = listItem["KPStatus"].Value;
                    string status = ImportUtilities.ENTITY_STATUS_STATES[listName].Find(s => s == kpStatus);
                    if(string.IsNullOrEmpty(status))
                    {
                        // value not matched set the value to empaty
                        listItem["KPStatus"].Value = "TBD";
                        importStatus.Messages.Add(string.Format(SKIPPED_STATUS_ADD, rowIndex, kpStatus));
                        importStatus.SkippedColumns.Add(string.Format("{0}: [Invalid]", "KPStatus"));
                    }

                }

                // validate state value
                if (listItem.ContainsKey("KPItemState") && !string.IsNullOrEmpty(listItem["KPItemState"].Value))
                {
                    string kpItemState = listItem["KPItemState"].Value;
                    string state = ImportUtilities.ITEM_STATES.Find(s => s == kpItemState);
                    if (string.IsNullOrEmpty(state))
                    {
                        // value not matched set the value to empaty
                        listItem["KPItemState"].Value = string.Empty;
                        importStatus.Messages.Add(string.Format(SKIPPED_STATE_ADD, rowIndex, kpItemState));
                        importStatus.SkippedColumns.Add(string.Format("{0}: [Invalid]", "KPItemState"));
                    }
                }
                rowIndex++;
            }
        }

        /// <summary>
        /// Check GoalSet value and CategoryL1 and CategoryL2 values 
        /// and determine if selections are valid children of each
        /// </summary>
        /// <param name="sheetData"></param>
        public static bool ValidateGoalSetCategories(DomainManager domainManager, Dictionary<string, string> originalLookup, KPListItem item, ImportStatus importErrors)
        {
            string ERR_CATL1_MISMATCH = "Row: {0}, CategoryL1 Mismatch: {1}";
            string ERR_CATL2_MISMATCH = "Row: {0}, CategoryL2 Mismatch: {1}";
            string ERR_CATL1_INVALID = "Row: {0}, CategoryL1 has an invalid value so unable to match CategoryL2: {1}";
            string ERR_MISSING_CATEGORY_VALUE = "Row {0} contains a value for Category Level 2, but not for Category Level 1.";
            string ERR_MISSING_GOALSET_VALUE = "Row {0} does not contain a value for GoalSet, but has values for Category Level 1 or 2. Please include GoalSet.";

            bool success = true;
            bool categoryL1Invalid = false;

            // keep item count to message user
            int itemCount = 1;

            // have CL1 or CL2 missing goal set
            if ((item.ContainsKey("CategoryL1") || item.ContainsKey("CategoryL2")) && !item.ContainsKey("GoalSet"))
            {
                importErrors.Messages.Add(string.Format(ERR_MISSING_GOALSET_VALUE, itemCount + 1));
                importErrors.SkippedRowCount++;
            }
            else
            {
                // have CL2 missing CL1
                if (item.ContainsKey("CategoryL2") && !item.ContainsKey("CategoryL1"))
                {
                    importErrors.Messages.Add(string.Format(ERR_MISSING_CATEGORY_VALUE, itemCount + 1));
                    importErrors.SkippedRowCount++;
                    success = false;
                }
                else
                {
                    // we do have a CL1
                    if (item.ContainsKey("CategoryL1"))
                    {
                        // get the mapped value
                        CategoryL1 CL1 = domainManager.CategoryL1Repository.Items.Find(cl1 => cl1.KPID.ToString() == item["CategoryL1"].Value);

                        // if we have one check if the CategoryL1 is in the GoalSet
                        if (CL1 == null || CL1.GoalSetId.ToString() != item["GoalSet"].Value)
                        {
                            importErrors.Messages.Add(string.Format(ERR_CATL1_MISMATCH, itemCount, originalLookup["CategoryL1"]));
                            importErrors.SkippedColumns.Add("Category L1: [No GoalSet]");
                            item["CategoryL1"].Value = string.Empty;
                            categoryL1Invalid = true;
                            success = false;
                        }
                    }

                    if (item.ContainsKey("CategoryL2"))
                    {
                        if (!categoryL1Invalid)
                        {
                            CategoryL2 CL2 = domainManager.CategoryL2Repository.Items.Find(cl2 => cl2.KPID.ToString() == item["CategoryL2"].Value);
                            // check if the CategoryL2 is in the CategoryL1
                            if (CL2 == null || CL2.CategoryL1Id.ToString() != item["CategoryL1"].Value)
                            {
                                importErrors.Messages.Add(string.Format(ERR_CATL2_MISMATCH, itemCount, originalLookup["CategoryL2"]));
                                importErrors.SkippedColumns.Add("Category L2: [Invalid]");
                                item["CategoryL2"].Value = string.Empty;
                                success = false;
                            }
                        }
                        else
                        {
                            importErrors.Messages.Add(string.Format(ERR_CATL1_INVALID, itemCount, originalLookup["CategoryL2"]));
                            importErrors.SkippedColumns.Add("Category L2: [No Category L1]");
                            item["CategoryL2"].Value = string.Empty;
                            success = false;
                        }
                    }

                    itemCount++;
                }
            }
            return success;
        }

        /// <summary>
        /// Remove any field names that should not be mapped
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsValidFieldName(string field)
        {
            bool isValid = true;
            List<string> excludedFieldNames = new List<string>() { 
                "KPGUID", "KPAttachment", "KPMovedTo", "KPMovedFrom", "OrderIndex", 
                "KPOwner", "KPOwners", "ParentProgramInitiative", "AEReduction", 
                "TTMImpact", "DraftMode", "AssociatedProject"
            };
            if (excludedFieldNames.Contains(field) || field.Contains("Permissions") || field.Contains("Related") || field.Contains("Effort"))
                isValid = false;

            return isValid;
        }

    }
}
