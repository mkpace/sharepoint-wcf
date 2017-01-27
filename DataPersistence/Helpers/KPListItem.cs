using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Utilities;
using Microsoft.SharePoint;

namespace Amazon.Kingpin.WCF2.DataPersistence.Helpers
{
    /// <summary>
    /// Wraps SPListItem as a dictionary so we don't have to expose SP across our domain model
    /// </summary>
    public class KPListItem : Dictionary<string, KPItem> { }

    /// <summary>
    /// Thin carrier class for an SPItem. This object
    /// is used to assist with conversion to/from the underlying data provider
    /// </summary>
    public class KPItem
    {
        /// <summary>
        /// Value of item as string
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Item type from SPField.Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Ctor to create a new item - used to serialize 
        /// a KP object into an SPItem for save and update
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(string value, string type)
        {
            value = KPUtilities.ConvertToSPType(value, type);
            this.Value = value;
            this.Type = type;
        }
        /// <summary>
        /// Ctor overloaded to handle int value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(int? value, string type)
        {
            this.Value = value.ToString();
            this.Type = type;
        }
        /// <summary>
        /// Ctor overload to handle boolean value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(bool value, string type)
        {
            this.Value = (value) ? "true" : "false";
            this.Type = type;
        }
        /// <summary>
        /// Ctor overloaded to handle List<int> value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(List<int> value, string type)
        {
            if (value != null)
                this.Value = string.Join(GlobalConstants.MULTIVALUE_DELIMITER.ToString(), value.Select(v => v.ToString()).ToArray());
            else
                this.Value = string.Empty;
            this.Type = type;
        }
        /// <summary>
        /// Ctor overloaded to handle List<sring> value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(List<string> value, string type)
        {
            if (value != null)
                this.Value = string.Join(GlobalConstants.MULTIVALUE_DELIMITER.ToString(), value.Select(v => v.ToString()).ToArray());
            else
                this.Value = string.Empty;
            this.Type = type;
        }        
        /// <summary>
        /// Ctor overloaded to handle Double value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(double? value, string type)
        {
            this.Value = value.ToString();
            this.Type = type;
        }
        /// <summary>
        /// Ctor overloaded to handle DateTime value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public KPItem(DateTime? value, string type)
        {
            if (value.HasValue)
                if (type.Equals(EntityConstants.ItemTypes.DATE))
                    this.Value = value.GetValueOrDefault().ToString("yyyy-MM-dd");
                else
                    this.Value = value.GetValueOrDefault().ToString("o");
            else
                this.Value = null;
            this.Type = type;
        }
        /// <summary>
        /// Ctor to create a new item - used to create 
        /// items from an SPItem for use as a domain object
        /// </summary>
        /// <param name="item"></param>
        /// <param name="field"></param>
        public KPItem(SPListItem item, SPField field)
        {
            // get the field type - used later to cast
            this.Type = item.Fields[field.Title].TypeAsString;
            // will cast User as this is a special type and the value is embedded
            switch(this.Type)
            {
                case "User":
                    this.Value = GetSPUser(item, field);
                    break;
                case "Date":
                    this.Value = (string.IsNullOrEmpty(GetValue(item[field.Title]))) ? null : GetValue(item[field.Title]);
                    break;
                case "Guid":
                    // strip off the { and } from the GUID value
                    this.Value = item[field.Title].ToString().Replace("{", string.Empty).Replace("}", string.Empty);
                    break;
                default:
                    this.Value = GetValue(item[field.Title]);
                    break;
            }
        }

        /// <summary>
        /// Checks value for null or empty and returns string.Empty if 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetValue(object value)
        {
            return (value != null) ?  value.ToString() : string.Empty;
        }

        /// <summary>
        /// User object is embedded in the field value as SPUserField object
        /// </summary>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private string GetSPUser(SPListItem item, SPField field)
        {
            string alias = string.Empty;
            // get the FieldUser object
            SPFieldUser fieldUser = item.Fields[field.Title] as SPFieldUser;
            // get the FieldUserValue object
            if(item[field.Title] != null)
            {
                SPFieldUserValue spUserValue = fieldUser.GetFieldValue(item[field.Title].ToString()) as SPFieldUserValue;
                // split out the name if we have one
                string[] name = (!string.IsNullOrEmpty(spUserValue.User.Name)) ? spUserValue.User.Name.Split(GlobalConstants.COMMA_DELIMITER) : new string[] { string.Empty, string.Empty };
                
                // sometimes Share Point User does not have full name
                string firstName = string.Empty;
                string lastName = string.Empty;
                if (name.Length == 1)
                {
                    firstName = name[0].Trim();
                    lastName = name[0].Trim();
                }
                else
                {
                    firstName = name[1].Trim();
                    lastName = name[0].Trim();
                }

                // create a new KPUser object - consider using this?
                KPUser kpUser = new KPUser()
                {
                    ID = spUserValue.User.ID,
                    Alias = spUserValue.User.LoginName,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = spUserValue.User.Email
                };
                // returning only the name and alias pipe (|) delimited for now
                alias = string.Format("{0}|{1}", spUserValue.User.Name, kpUser.Alias.Split('\\')[1]);

            }
            return alias;
        }
    }
}
