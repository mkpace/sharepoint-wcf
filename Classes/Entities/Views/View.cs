using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    /// <summary>
    /// Uses only BaseEntity type - no additional properties
    /// </summary>
    [DataContract()]
    public class View : IKPEntity
    {
        protected KPListItem itemProperties = null;

        [DataMember(Name = "ID")]
        public int ID { get; set; }
        [DataMember(Name = "KPID")]
        public int KPID { get; set; }
        [DataMember(Name = "KPGUID")]
        public string KPGUID { get; set; }
        [DataMember(Name = "Title")]
        public string Title { get; set; }
        [DataMember(Name = "ViewOwner")]
        public string ViewOwner { get; set; }
        [DataMember(Name = "KPFilters")]
        public string KPFilters { get; set; }
        [DataMember(Name = "KPDescription")]
        public string KPDescription { get; set; }
        [DataMember(Name = "GridStates")]
        public string GridStates { get; set; }
        [DataMember(Name = "StrikethroughOverride")]
        public string StrikethroughOverride { get; set; }
        [DataMember(Name = "RelatedCheckpoints")]
        public string RelatedCheckpoints { get; set; }
        [DataMember(Name = "SharedWith")]
        public string SharedWith { get; set; }
        [DataMember(Name = "EntityTypes")]
        public string EntityTypes { get; set; }
        [DataMember(Name = "KPItemState")]
        public string KPItemState { get; set; }


        [DataMember(Name = "SPCreatedDate")]
        public DateTime? SPCreatedDate { get; set; }
        [DataMember(Name = "SPModifiedDate")]
        public DateTime? SPModifiedDate { get; set; }
        [DataMember(Name = "SPCreatedBy")]
        public string SPCreatedBy { get; set; }
        [DataMember(Name = "SPModifiedBy")]
        public string SPModifiedBy { get; set; }

        public int? KPTeamId { get; set; }

        [DataMember(Name = "Type")]
        public string Type { get; set; }


        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public void SetProperties(KPListItem item, string listName)
        {
            this.ID = (item.ContainsKey("ID")) ?KPUtilities.ParseInt(item["ID"].Value) : -1;
            this.KPID = (item.ContainsKey("KPID")) ? KPUtilities.ParseInt(item["KPID"].Value) : -1;
            this.KPGUID = (item.ContainsKey("KPGUID")) ? item["KPGUID"].Value : string.Empty;
            this.Title = (item.ContainsKey("Title")) ? item["Title"].Value : string.Empty;
            this.ViewOwner = (item.ContainsKey("ViewOwner")) ? item["ViewOwner"].Value : string.Empty;
            this.KPFilters = KPUtilities.StripHTML((item.ContainsKey("KPFilters")) ? item["KPFilters"].Value : string.Empty, false);
            this.KPDescription = KPUtilities.StripHTML((item.ContainsKey("KPDescription")) ? item["KPDescription"].Value : string.Empty, false);
            this.GridStates = KPUtilities.StripHTML((item.ContainsKey("GridStates")) ? item["GridStates"].Value : string.Empty, false);
            this.StrikethroughOverride = (item.ContainsKey("StrikethroughOverride")) ? item["StrikethroughOverride"].Value : string.Empty;
            this.RelatedCheckpoints = (item.ContainsKey("RelatedCheckpoints")) ? item["RelatedCheckpoints"].Value : string.Empty;
            this.SharedWith = (item.ContainsKey("SharedWith")) ? item["SharedWith"].Value : string.Empty;
            this.KPItemState = item["KPItemState"].Value;
            this.EntityTypes = (item.ContainsKey("EntityTypes")) ? item["EntityTypes"].Value : string.Empty;

            this.SPCreatedDate = KPUtilities.ParseDateTime(item["Created"].Value);
            this.SPModifiedDate = KPUtilities.ParseDateTime(item["Modified"].Value);
            this.SPCreatedBy = item["CreatedBy"].Value;
            this.SPModifiedBy = item["ModifiedBy"].Value;

            this.KPTeamId = null;

            this.Type = listName;
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
            
            if (this.KPID > -1) { this.ID = this.KPID; }
            // get instance properties
            this.itemProperties.Add("ID", new KPItem(this.ID.ToString(), EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPID", new KPItem(this.KPID.ToString(), EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPGUID", new KPItem(this.KPGUID, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Title", new KPItem(this.Title, EntityConstants.ItemTypes.TEXT));


            this.itemProperties.Add("ViewOwner", new KPItem(this.ViewOwner, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPFilters", new KPItem(this.KPFilters, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPDescription", new KPItem(this.KPDescription, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("GridStates", new KPItem(this.GridStates, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("StrikethroughOverride", new KPItem(this.StrikethroughOverride, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RelatedCheckpoints", new KPItem(this.RelatedCheckpoints, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("SharedWith", new KPItem(this.SharedWith, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPItemState", new KPItem(this.KPItemState, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("EntityTypes", new KPItem(this.EntityTypes, EntityConstants.ItemTypes.TEXT));


            this.itemProperties.Add("Created", new KPItem(this.SPCreatedDate, EntityConstants.ItemTypes.DATETIME));
            this.itemProperties.Add("Modified", new KPItem(this.SPModifiedDate, EntityConstants.ItemTypes.DATETIME));
            this.itemProperties.Add("CreatedBy", new KPItem(this.SPCreatedBy, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ModifiedBy", new KPItem(this.SPModifiedBy, EntityConstants.ItemTypes.TEXT));

            return this.itemProperties;
        }

    }
}
