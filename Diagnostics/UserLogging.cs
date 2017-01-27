using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Diagnostics
{
    [DataContract()]
    public class UserLogging : BaseItem, IKPItem
    {
        [DataMember(Name = "Action")]
        public string Action { get; set; }
        [DataMember(Name = "TeamPath")]
        public string TeamPath { get; set; }
        [DataMember(Name = "ListName")]
        public string ListName { get; set; }
        [DataMember(Name = "ItemKPID")]
        public string ItemKPID { get; set; }
        [DataMember(Name = "ItemGUID")]
        public string ItemGUID { get; set; }
        [DataMember(Name = "ItemData")]
        public string ItemData { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.Action = item["Action"].Value;
            this.TeamPath = item["TeamPath"].Value;
            this.ListName = item["ListName"].Value;
            this.ItemKPID = item["ItemKPID"].Value;
            this.ItemGUID = item["ItemGUID"].Value;
            this.ItemData = item["ItemData"].Value;
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("Action", new KPItem(this.Action, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("TeamPath", new KPItem(this.TeamPath, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ListName", new KPItem(this.ListName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ItemKPID", new KPItem(this.ItemKPID, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ItemGUID", new KPItem(this.ItemGUID, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ItemData", new KPItem(this.ItemData, EntityConstants.ItemTypes.TEXT));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
