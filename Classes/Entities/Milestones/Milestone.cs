﻿using System;
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
    public class Milestone : BaseEntity, IKPEntity
    {
        // Public Serializable Properties
        // are marked with this attribute to declare which
        // properties are to be serialized and what their 
        // JSON representation should be. If we wish to create
        // proper camel-cased names then we would change these attribute names
        [DataMember(Name = "RelatedProject")]
        public string RelatedProject { get; set; }
        [DataMember(Name = "MilestoneOwner")]
        public string MilestoneOwner { get; set; }

        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public override void SetProperties(KPListItem item, string listName)
        {
            this.RelatedProject = (item.ContainsKey("RelatedProject")) ? item["RelatedProject"].Value : null;
            this.MilestoneOwner = (item.ContainsKey("MilestoneOwner")) ? item["MilestoneOwner"].Value : null;
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
            this.itemProperties.Add("RelatedProject", new KPItem(this.RelatedProject, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("MilestoneOwner", new KPItem(this.MilestoneOwner, EntityConstants.ItemTypes.TEXT));
            base.GetEntityProperties();
            return this.itemProperties;
        }
    }
}
