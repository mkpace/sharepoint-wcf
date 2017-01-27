using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Utilities
{
    public class GlobalConstants
    {
        public const char COMMA_DELIMITER = ',';
        public const char MULTIVALUE_DELIMITER = ';';
    }

    public static class EntityConstants
    {
        public static class ItemTypes
        {
            public const string TEXT = "Text";
            public const string NOTE = "Note";
            public const string NUMBER = "Number";
            public const string CHOICE = "Choice";
            public const string DATE = "Date";
            public const string DATETIME = "DateTime";
            public const string YESNO = "YesNo";
            public const string JSON = "JSON";
        }
        public static class Lookup
        {
            public const string INDEX = "KPIndex";
            public const string TEAMS = "KPTeams";
            public const string GOAL_SETS = "KPGoalSets";
            public const string CATEGORY_L1 = "KPCategoryL1";
            public const string CATEGORY_L2 = "KPCategoryL2";
            public const string COUNTRY = "KPCountry";
            public const string CUSTOMERS = "KPCustomers";
        }
        public static class Diagnostics
        {
            public const string EVENT_LOGGING = "EventLogging";
            public const string USER_LOGGING = "UserLogging";
        }
        public static class Entity
        {
            public const string GOALS = "KPGoals";
            public const string PROJECTS = "KPProjects";
        }
        public static class Fields
        {
            public const string KPGUID = "KPGUID";
        }
    }
}
