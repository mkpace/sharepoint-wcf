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

namespace Amazon.Kingpin.WCF2.Classes.Lookup
{
    [DataContract()]
    public class PerspectiveInstance : BaseItem, IKPItem
    {
        [DataMember(Name = "CollapseSettings")]
        public string CollapseSettings { get; set; }
        [DataMember(Name = "GridStates")]
        public string GridStates { get; set; }
        [DataMember(Name = "IsDefault")]
        public bool IsDefault { get; set; }
        [DataMember(Name = "IsOwner")]
        public bool IsOwner { get; set; }
        [DataMember(Name = "IsShared")]
        public bool IsShared { get; set; }
        [DataMember(Name = "KPGUIDRef")]
        public string KPGUIDRef { get; set; }
        [DataMember(Name = "KPUserID")]
        public int? KPUserID { get; set; }
        [DataMember(Name = "KPFilters")]
        public string KPFilters { get; set; }
        [DataMember(Name = "KPUserName")]
        public string KPUserName { get; set; }
        [DataMember(Name = "Module")]
        public string Module { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.CollapseSettings = item["CollapseSettings"].Value;
            this.GridStates = item["GridStates"].Value;
            this.IsDefault = ParseBool(item["IsDefault"].Value);
            this.IsOwner = ParseBool(item["IsOwner"].Value);
            this.IsShared = ParseBool(item["IsShared"].Value);
            this.KPGUIDRef = item["KPGUIDRef"].Value;
            this.KPUserID = CheckNullInt("KPUserID", item);
            this.KPFilters = item["KPFilters"].Value;
            this.KPUserName = item["KPUserName"].Value;
            this.Module = item["Module"].Value;
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("CollapseSettings", new KPItem(this.CollapseSettings, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("GridStates", new KPItem(this.GridStates, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("IsDefault", new KPItem(this.IsDefault, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("IsOwner", new KPItem(this.IsOwner, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("IsShared", new KPItem(this.IsShared, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("KPGUIDRef", new KPItem(this.KPGUIDRef, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPUserID", new KPItem(this.KPUserID, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("KPFilters", new KPItem(this.KPFilters, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPUserName", new KPItem(this.KPUserName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Module", new KPItem(this.Module, EntityConstants.ItemTypes.TEXT));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
