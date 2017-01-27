using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes;
using Amazon.Kingpin.WCF2.Classes.Importing;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Importing.Utilities;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Importing.Utilities
{
    internal class ImportUtilities
    {
        private Dictionary<string, Dictionary<string, List<string>>> columnMappingCache = new Dictionary<string, Dictionary<string, List<string>>>();

        // item state lookup
        internal static List<string> ITEM_STATES = new List<string>() { "Active", "Inactive", "Deleted" };

        // countries lookup
        internal static List<string> COUNTRIES = new List<string>() { "WW", "US", "CA", "UK", "DE", "FR", "IT", "ES", "JP", "CN", "IN", "BR", "MX", "AU", "NL" };

        // choice fields that don't have values set
        internal static List<string> CHOICE_FIELDS = new List<string>() { 
            "KPStatus", "KPItemState", "KPPrimaryVP", "KPSecondaryVPs", "KPStatus", 
            "KPTeam", "Country", "GoalSet", "CategoryL1", "CategoryL2"
        };

        // status states for KPGoals/KPProjects - different
        internal static Dictionary<string, List<string>> ENTITY_STATUS_STATES = new Dictionary<string, List<string>>();

        static ImportUtilities()
        {
            // goals
            ENTITY_STATUS_STATES.Add("KPProjects", new List<string>() { 
                        "Business Proposal","Exploring","Scoping","Scheduling",
                        "Not Started","TBD","Green","Yellow","Red","Cancelled",
                        "Completed","Completed Late"
                    });
            // projects
            ENTITY_STATUS_STATES.Add("KPGoals", new List<string>() { 
		                "Green","Yellow","Red","DNM",
                        "Not Started","Completed","Completed Late"
                    });
        }

        /// <summary>
        /// Convert the value if it is a DateTime
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //internal static string ConvertCellValueDateTime(string value)
        //{
        //    string returnValue;

        //    // need to parse different inputs
        //    DateTime date;
        //    if (string.IsNullOrEmpty(value))
        //        returnValue = value;
        //    if (!DateTime.TryParse(value, out date))
        //    {
        //        double dateVal;
        //        if (double.TryParse(value, out dateVal))
        //            date = DateTime.FromOADate(dateVal);
        //    }
        //    if(date == new DateTime())
        //        return string.Empty;
        //    else
        //        return date.ToString("MM/dd/yyyy hh:mm:ss tt");
        //}

        /// <summary>
        /// Sets the team object and id on mapping data
        /// </summary>
        /// <param name="spDataAccess"></param>
        /// <param name="mappingData"></param>
        internal static void SetTeam(SPDataAccess spDataAccess, MappingData mappingData)
        {
            string teamName = mappingData.Mappings["KPTeam"];
            Team team = spDataAccess.Teams.Find(t => t.Nick == teamName);
            mappingData.TeamId = team.KPID;
            mappingData.Team = team;
        }

        /// <summary>
        /// Check fields to see if they map to an 
        /// existing predefined import template
        /// </summary>
        /// <param name="list"></param>
        internal static ImportType? GetImportTemplateType(List<string> worksheetFields)
        {
            ImportType? templateType = null;
            string templateName = string.Empty;
            Dictionary<string, Dictionary<string, string>> importTemplates = new Dictionary<string, Dictionary<string, string>>();
            importTemplates.Add("Import", TemplateMapping.Import);
            importTemplates.Add("Export", TemplateMapping.Export);
            importTemplates.Add("Current", TemplateMapping.Current);

            foreach(KeyValuePair<string, Dictionary<string,string>> template in importTemplates)
            {
                Dictionary<string,string> templateKeys = template.Value;
                int fieldCount = 0;

                // remove KPID since we add this to track the team for each item
                if (template.Key.Equals("Import") || template.Key.Equals("Current"))
                    templateKeys.Remove("KPTeam");

                foreach(string field in worksheetFields)
                {
                    if(templateKeys.ContainsValue(field))
                    {
                        fieldCount++;
                    }
                }
                if (templateKeys.Count == fieldCount)
                    templateName = template.Key;
            }

            if (!string.IsNullOrEmpty(templateName))
                templateType = (ImportType)Enum.Parse(typeof(ImportType), templateName);

            return templateType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public List<ColumnDefinition> GetEntityColumnDefaultValues(SPDataAccess spDataAccess, string listName)
        {

            List<ColumnDefinition> columnDefinitions = new List<ColumnDefinition>();

            spDataAccess.InitializeCurrentUser();
            string siteUrl = spDataAccess.CurrentUser.Teams[1].SiteUrl;
            Dictionary<string, string> fields = spDataAccess.GetEntityFields(siteUrl, listName);

            foreach (KeyValuePair<string, string> kvp in fields)
            {
                List<string> defaultValues = new List<string>();
                defaultValues = this.GetPropertyDefaultValues(spDataAccess, listName, kvp.Key);

                if (ImportValidationUtilities.IsValidFieldName(kvp.Key))
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition(kvp.Key);
                    columnDefinition.DefaultValues = defaultValues;
                    columnDefinitions.Add(columnDefinition);
                }
            }

            // order by field name alpha so it's easier to read
            ColumnDefinition kpid = columnDefinitions.Find(c => c.Field.Equals("KPID"));
            columnDefinitions.Remove(kpid);
            columnDefinitions = columnDefinitions.OrderBy(c => c.FieldName).ToList();
            columnDefinitions.Insert(0, kpid);

            return columnDefinitions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<string> GetPropertyDefaultValues(SPDataAccess spDataAccess, string listName, string propertyName)
        {
            // return default values
            List<string> defaultValues = new List<string>();

            switch(propertyName)
            {
                case "KPTeam":
                    defaultValues = spDataAccess.CurrentUser.Teams.Select(t => t.Nick).ToList();
                    break;
                case "KPStatus":
                    defaultValues = ENTITY_STATUS_STATES[listName];
                    break;
                case "KPItemState":
                    defaultValues = ITEM_STATES;
                    break;
                case "Country":
                    defaultValues = COUNTRIES;
                    break;
                case "KPPrimaryVP":
                case "KPSecondaryVPs":
                    defaultValues = this.GetItemValues(spDataAccess.GetLookupItems("KPVPs"), "Nick");
                    break;
                case "GoalSet":
                    defaultValues = this.GetItemValues(spDataAccess.GetLookupItems("KPGoalSets"), "Title");
                    break;
                case "CategoryL1":
                case "CategoryL2":
                    defaultValues = this.GetItemValues(spDataAccess.GetLookupItems("KP" + propertyName), "Title");
                    break;
            }
            return defaultValues;
        }

        /// <summary>
        /// Get the value from the KPListItem
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<string> GetItemValues(List<KPListItem> list, string fieldName)
        {
            List<string> values = new List<string>();
            foreach(KPListItem listItem in list)
            {
                values.Add(listItem[fieldName].Value);
            }
            return values;
        }
    }
}
