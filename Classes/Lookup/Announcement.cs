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
    public class Announcement : BaseItem, IKPItem
    {
        [DataMember(Name = "KPDescription")]
        public string KPDescription { get; set; }
        [DataMember(Name = "VersionNumber")]
        public string VersionNumber { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPDescription = item["KPDescription"].Value;
            this.VersionNumber = item["VersionNumber"].Value;
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
            this.itemProperties.Add("KPDescription", new KPItem(this.KPDescription, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("VersionNumber", new KPItem(this.VersionNumber, EntityConstants.ItemTypes.TEXT));
            // add base properties
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
