using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing
{
    public class ImportConstants
    {
        public static List<string> STATUS_VALUES = new List<string>(){
            "GREEN",
            "YELLOW",
            "RED",
            "COMPLETE",
            "PUNTED",
            "CANCELLED",
            "NO STATUS",
            "NOT STARTED",
            "DNM",
            "TBD"
        };

        /// <summary>
        /// Default value delimited with a -#-
        /// json string { mappings: [{ key:"Title", value: "#Default Title" }] }
        /// </summary>
        public const string MAPPING_DEFAULT_DELIMITER = "#";

        public const string ERR_IMPORTING = "An error occured while importing your data";
        public const string ERR_MAPPING = "{0} is a required key field, please map this field.";
        public const string ERR_KPGUID_MISSING = "Error importing: KPGUID missing on row: {0}";
        public const string ERR_COLUMN_MISSING = "Error column missing in list: {0}";
        public const string ERR_COLUMN_UPDATE = "Error updating column: {0}";
        public const string ERR_INCORRECT_LOOKUP = "Row {0}, Column: {1} = {2}";
        public const string ERR_VERSION_CONFLICT = "Row {0}: Version Conflict";

        public const string SUCCESS_IMPORT = "Successfully imported sheet.";
        public const string SUCCESS_BULK_IMPORT = "Successfully parsed sheet data.";
        /// <summary>
        /// Skipped rows 0=Unchanged; 1=Skipped rows
        /// </summary>
        public const string SUCCESS_SKIPPED_ROWS = "Skipped unchanged rows <br/>{0} Unchanged rows skipped: [{1}]<br/>";

        public const string CAML_WHERE_AND = "<Where><And>{0}</And></Where>";
        public const string CAML_WHERE = "<Where>{0}</Where>";
        public const string CAML_FIELD_EQ_REF = "<Eq><FieldRef Name='{0}'/><Value Type='{1}'>{2}</Value></Eq>";
        public const string CAML_FIELD_CONTAINS_REF = "<Contains><FieldRef Name='{0}' /><Value Type='{1}'>{2}</Value></Contains>";

        public const string COLUMN_NAME_TITLE = "Title";
        //public const string COLUMN_NAME_ID = "ID";
        public const string STATUS_NOT_MAPPED = "TBD";
    }
}
