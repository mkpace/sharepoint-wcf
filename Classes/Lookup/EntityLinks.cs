using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Lookup
{
    [DataContract()]
    public class EntityLinks : BaseItem, IKPItem
    {
        [DataMember(Name = "ParentID")]
        public int ParentID { get; set; }
        [DataMember(Name = "ChildID")]
        public int ChildID { get; set; }
        [DataMember(Name = "Deleted")]
        public bool Deleted { get; set; }

        /// <summary>
        /// Default param-less Ctor
        /// </summary>
        public EntityLinks() { }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.ParentID = ParseInt(item["ParentID"].Value);
            this.ChildID = ParseInt(item["ChildID"].Value);
            this.Deleted = ParseBool(item["Deleted"].Value);
            base.SetBaseProperties(item, listName);
        }

        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            // get instance properties
            this.itemProperties.Add("ParentID", new KPItem(this.ParentID, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("ChildID", new KPItem(this.ChildID, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("Deleted", new KPItem(this.Deleted, EntityConstants.ItemTypes.YESNO));
            // add base properties
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
