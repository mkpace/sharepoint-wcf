using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Importing.Entities;

namespace Amazon.Kingpin.WCF2.Importing.Utilities
{
    public class TemplateMapping
    {
        public static Dictionary<string,string> Import
        {
            get
            {
                // create manual mapping for standard template sent to "clients"
                MappingData mappingData = new MappingData();
                mappingData.Mappings = new Dictionary<string, string>() { 
                { "KPPrimaryVP", "Primary Owner" },
                { "KPSecondaryVPs",  "Secondary Owner" },
                { "KPBusinessOwner", "Goal Manager" },
                { "Title","Goal Short Title"  },
                { "KPDescription",  "Goal Description" },
                { "Year", "Goal Year" },
                { "GoalSet", "Goal Set" },
                { "CategoryL1", "First Level Category" },
                { "CategoryL2", "Second Level Category" },
                { "KPOriginalDate", "Original Goal Date" },
                { "KPDate", "Current Goal Date" },
                { "Country", "Country" },
                { "KPCompletionDate", "Goal Origination" },
                { "KPID", "Replace Goal ID" },
                { "MetricValue", "Metric" },
                { "KPComments", "Comments" },
                { "KPTeam", "KPTeam" }
            };
                return mappingData.Mappings;
            }
            private set { }
        }
        public static Dictionary<string,string>  Export
        {
            get
            {
                MappingData mappingData = new MappingData();
                mappingData.Mappings = new Dictionary<string, string>() { 
                    { "KPTeam", "Team" },
                    { "KPID", "ID" },
                    { "GoalSet", "GoalSet" },
                    { "CategoryL1", "CategoryL1" },
                    { "CategoryL2", "CategoryL2" },
                    { "Title", "Title" },
                    { "KPDescription", "Description" },
                    { "KPSoWhat", "Why This Goal Matters" },
                    { "MetricValue", "Goal Measurement" },
                    { "KPBusinessOwner", "BusinessOwner" },
                    { "KPFinanceOwner", "FinanceOwner" },
                    { "KPGoalContributor", "GoalContributor" },
                    { "KPStatus", "Status" },
                    { "KPComments", "Where do we stand" },
                    { "KPPathToGreen", "Path To Green" },
                    { "KPOriginalDate", "OriginalDate" },
                    { "KPDate", "Date" },
                    { "KPCompletionDate", "CompletionDate" },
                    { "KPPrimaryVP", "PrimaryVP" },
                    { "KPSecondaryVPs", "SecondaryVPs" },
                    { "Year", "Year" },
                    { "Country", "Country" }
                };
                return mappingData.Mappings;
            }
            private set { }
        }

        public static Dictionary<string,string> Current
        {
            get
            {
                MappingData mappingData = new MappingData();
                mappingData.Mappings = new Dictionary<string, string>() { 
                { "KPExternalID", "id" },
                { "Title", "Name" },
                { "KPDescription", "Project Update" },
                { "KPStatus", "Status" },
                { "KPComments", "Short Description" },
                { "KPDate", "Planned Ship" },
                { "KPCompletionDate", "Actual Project Ship" },
                { "ProjectType", "ProjectType" },
                { "KPTeam", "KPTeam" }
            };
                return mappingData.Mappings;
            }
            private set { }
        }
    }
}
