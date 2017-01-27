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
    public class EntityIndex : BaseItem, IKPItem
    {
        [DataMember(Name = "KPType")]
        public string KPType { get; set; }
        [DataMember(Name = "KPTeam")]
        public int? KPTeamId { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPType = item["KPType"].Value;
            this.KPTeamId = ParseInt(item["KPTeam"].Value);
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
            this.itemProperties.Add("KPType", new KPItem(this.KPType, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPTeam", new KPItem(this.KPTeamId.Value, EntityConstants.ItemTypes.NUMBER));
            // add base properties
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
