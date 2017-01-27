using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class BaseItem
    {
        protected KPListItem itemProperties = null;

        [DataMember(Name = "ID")]
        public int ID { get; set; }

        [DataMember(Name = "Title")]
        public string Title { get; set; }

        [DataMember(Name = "KPID")]
        public int KPID { get; set; }

        [DataMember(Name = "KPGUID")]
        public string KPGUID { get; set; }

        [DataMember(Name = "OrderIndex")]
        public double? OrderIndex { get; set; }

        [DataMember(Name = "SPCreatedDate")]
        public DateTime? SPCreatedDate { get; set; }

        [DataMember(Name = "SPModifiedDate")]
        public DateTime? SPModifiedDate { get; set; }

        [DataMember(Name = "SPCreatedBy")]
        public string SPCreatedBy { get; set; }
        [DataMember(Name = "SPModifiedBy")]
        public string SPModifiedBy { get; set; }
        
        [DataMember(Name = "Type")]
        public string Type { get; set; }

        /// <summary>
        /// Initialize the base item ID's
        /// </summary>
        internal BaseItem() 
        {
            this.ID = -1;
            this.KPID = -1;
        }

        /// <summary>
        /// Set the base properties of the object
        /// </summary>
        /// <param name="item"></param>
        internal void SetBaseProperties(KPListItem item, string listName)
        {
            try
            {
                this.ID = (item.ContainsKey("ID")) ? ParseInt(item["ID"].Value) : -1;
                this.KPID = (item.ContainsKey("KPID")) ? ParseInt(item["KPID"].Value) : -1;
                this.KPGUID = (item.ContainsKey("KPGUID")) ? item["KPGUID"].Value : string.Empty;
                this.Title = (item.ContainsKey("Title")) ? item["Title"].Value : string.Empty;
                this.OrderIndex = (item.ContainsKey("OrderIndex")) ? ParseDouble(item["OrderIndex"].Value) : (double?)null;
                this.SPCreatedDate = this.ParseDateTime(item["Created"].Value);
                this.SPModifiedDate = this.ParseDateTime(item["Modified"].Value);
                this.SPCreatedBy = (item.ContainsKey("CreatedBy")) ? item["CreatedBy"].Value : null;
                this.SPModifiedBy = (item.ContainsKey("ModifiedBy")) ? item["ModifiedBy"].Value : null;
                this.Type = listName;
            }
            catch(Exception ex)
            {
                string errMsg = string.Format("Error setting BaseItem properties on {0}: {1}", listName, ex.Message);
                throw new Exception(errMsg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal string GetPropertyValue(string propertyName)
        {
            string value = string.Empty;
            if (itemProperties.ContainsKey(propertyName))
                value = this.itemProperties[propertyName].Value;

            return value;
        }

        /// <summary>
        /// Get the base object properties as a KPListItem
        /// </summary>
        /// <returns>KPListItem</returns>
        protected KPListItem GetBaseProperties()
        {
            if (this.itemProperties == null)
            {
                // get base properties
                this.itemProperties = new KPListItem();
            }

            if (this.KPID > -1) { this.ID = this.KPID; }
            // get instance properties
            this.itemProperties.Add("ID", new KPItem(this.ID.ToString(), EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPID", new KPItem(this.KPID.ToString(), EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPGUID", new KPItem(this.KPGUID, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("Title", new KPItem(this.Title, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("OrderIndex", new KPItem(this.OrderIndex, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("Created", new KPItem(this.SPCreatedDate, EntityConstants.ItemTypes.DATETIME));
            this.itemProperties.Add("Modified", new KPItem(this.SPModifiedDate, EntityConstants.ItemTypes.DATETIME));
            this.itemProperties.Add("CreatedBy", new KPItem(this.SPCreatedBy, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ModifiedBy", new KPItem(this.SPModifiedBy, EntityConstants.ItemTypes.TEXT));
            
            return this.itemProperties;
        }

        /// <summary>
        /// Handles converting string to bool
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        protected bool ParseBool(string strValue)
        {
            bool value = false;
            bool.TryParse(strValue, out value);
            return value;
        }
        /// <summary>
        /// Handles converting string to int
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        protected int ParseInt(string strValue)
        {
            int value = -1;
            int.TryParse(strValue, out value);
            return value;
        }
        /// <summary>
        /// Handles converting string to DateTime (nullable)
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        protected DateTime? ParseDateTime(string strValue)
        {
            DateTime value;
            DateTime? nullableDate = (DateTime?)null;
            if (DateTime.TryParse(strValue, out value))
                nullableDate = (DateTime?)value;

            return nullableDate;
        }
        /// <summary>
        /// Handles converting string to float
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        protected float ParseFloat(string strValue)
        {
            float value = 0.0f;
            float.TryParse(strValue, out value);
            return value;
        }
        /// <summary>
        /// Handles converting string to Double
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        protected Double ParseDouble(string strValue)
        {
            return ParseDouble(strValue, 0.0);
        }
        /// <summary>
        /// Handles converting string to Double returning a default value
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected Double ParseDouble(string strValue, Double defaultValue)
        {
            Double value = defaultValue;
            Double.TryParse(strValue, out value);
            return value;
        }

        protected List<int> ConvertListInt(string value)
        {
            if (!string.IsNullOrEmpty(value))
                return value.Split(GlobalConstants.MULTIVALUE_DELIMITER).Select(int.Parse).ToList();
            else
                return new List<int>();
        }

        /// <summary>
        /// Checks for a null value and returns the nullable int type if value is null
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected int? CheckNullInt(string fieldName, KPListItem item)
        {
            return (item.ContainsKey(fieldName)) ? ParseInt(item[fieldName].Value) : (int?)null;
        }
    }
}
