using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Amazon.Kingpin.WCF2.Classes
{
    /// <summary>
    /// Mapped columns to return to UI
    /// Contains definition to display proper dropdowns to user
    /// </summary>
    [DataContract(Name = "columnDefinition")]
    public class ColumnDefinition
    {
        /// <summary>
        /// Column field name from SPList
        /// </summary>
        [DataMember(Name = "field")]
        public string Field { get; set; }
        /// <summary>
        /// Column field name from SPList
        /// </summary>
        [DataMember(Name = "fieldName")]
        public string FieldName { get; set; }
        /// <summary>
        /// Flag to indicate field name matches worksheet column name
        /// </summary>
        [DataMember(Name = "isMapped")]
        public bool IsMapped { get; set; }
        /// <summary>
        /// Used in the Client UI - will always be false by default
        /// so the default textbox is disabled (misnomer)
        /// </summary>
        [DataMember(Name = "disableDefault")]
        public bool DisableDefault { get; set; }
        /// <summary>
        /// Default values from SPList [choice|lookup] field
        /// </summary>
        [DataMember(Name = "defaultValues")]
        public List<string> DefaultValues { get; set; }

        /// <summary>
        /// Default Ctor: needed for serialization / DataContract
        /// </summary>
        public ColumnDefinition() : this(string.Empty) { }

        /// <summary>
        /// Ctor initializes object with field
        /// </summary>
        /// <param name="field"></param>
        public ColumnDefinition(string field)
        {
            this.IsMapped = false;
            this.DisableDefault = true;
            this.Field = field;
            this.FieldName = this.ConvertFieldName(field);
            this.DefaultValues = new List<string>();
        }

        /// <summary>
        /// Primative convert to remove the "KP" prefix
        /// and split the name by adding a space before any upper cased character
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string ConvertFieldName(string field)
        {
            if (!string.IsNullOrEmpty(field))
            {
                if(field.Contains("L1") || field.Contains("L2"))
                {
                    field = field.Replace("L", "Level ");
                }
                field = field.Replace("KP", string.Empty);
                field = Regex.Replace(field, @"(\B[A-Z]+?(?=[A-Z][^A-Z][^a-z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
            }
            return field;
        }
    }
}
