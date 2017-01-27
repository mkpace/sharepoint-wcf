using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    /// <summary>
    /// All Entities inherit from IKPEntity which allows
    /// the dynamic creation of these entity objects
    /// All Entities also inherit from BaseEntity which contains
    /// the base properties for all Entity objects in the system.
    /// </summary>
    [DataContract()]
    public class Goal : BaseEntity, IKPEntity
    {
        // Public Serializable Properties
        // are marked with this attribute to declare which
        // properties are to be serialized and what their 
        // JSON representation should be. If we wish to create
        // proper camel-cased names then we would change these attribute names
        [DataMember(Name = "GoalSet")]
        public int? GoalSetId { get; set; }
        [DataMember(Name = "CategoryL1")]
        public int? CategoryL1Id { get; set; }
        [DataMember(Name = "CategoryL2")]
        public int? CategoryL2Id { get; set; }
        [DataMember(Name = "Country")]
        public string Country { get; set; }
        [DataMember(Name = "Year")]
        public int? Year { get; set; }
        [DataMember(Name = "KPGoalContributor")]
        public string KPGoalContributor { get; set; }
        [DataMember(Name = "MetricValue")]
        public string MetricValue { get; set; }

        // Child Object Members - do not serialize to JSON
        public GoalSet GoalSet { get; set; }
        public CategoryL1 CategoryL1 { get; set; }
        public CategoryL2 CategoryL2 { get; set; }

        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public override void SetProperties(KPListItem item, string listName)
        {
            this.GoalSetId = CheckNullInt("GoalSet", item);
            this.CategoryL1Id = CheckNullInt("CategoryL1", item);
            this.CategoryL2Id = CheckNullInt("CategoryL2", item);
            this.Country = item["Country"].Value;
            this.Year = CheckNullInt("Year", item);
            this.KPGoalContributor = item["KPGoalContributor"].Value;
            this.MetricValue = KPUtilities.StripHTML((item.ContainsKey("MetricValue")) ? item["MetricValue"].Value : string.Empty, false);
            base.SetProperties(item, listName);
        }

        /// <summary>
        /// Get the properties of this object instance.
        /// This method is used by the DataAccess layer (DAL) 
        /// to serialize the object instance to persist the 
        /// data to the underlying data storage (DataProvider)
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            if(this.GoalSet != null)
                this.itemProperties.Add("GoalSet", new KPItem(this.GoalSet.KPID, EntityConstants.ItemTypes.TEXT));
            if (this.CategoryL1 != null)
                this.itemProperties.Add("CategoryL1", new KPItem(this.CategoryL1.KPID, EntityConstants.ItemTypes.TEXT));
            if (this.CategoryL2 != null)
                this.itemProperties.Add("CategoryL2", new KPItem(this.CategoryL2.KPID, EntityConstants.ItemTypes.TEXT));

            if (this.GoalSetId > -1 || this.GoalSetId == null) 
                this.itemProperties.Add("GoalSet", new KPItem(this.GoalSetId, EntityConstants.ItemTypes.TEXT));
            if (this.CategoryL1Id > -1 || this.CategoryL1Id == null) 
                this.itemProperties.Add("CategoryL1", new KPItem(this.CategoryL1Id, EntityConstants.ItemTypes.TEXT));
            if (this.CategoryL2Id > -1 || this.CategoryL2Id == null)
                this.itemProperties.Add("CategoryL2", new KPItem(this.CategoryL2Id, EntityConstants.ItemTypes.TEXT));

            this.itemProperties.Add("Country", new KPItem(this.Country, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Year", new KPItem(this.Year, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPGoalContributor", new KPItem(this.KPGoalContributor, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("MetricValue", new KPItem(this.MetricValue, EntityConstants.ItemTypes.NOTE));
            base.GetEntityProperties();
            return this.itemProperties;
        }


    }
}
