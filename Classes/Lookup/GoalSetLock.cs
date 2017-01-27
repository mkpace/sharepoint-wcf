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
    public class GoalSetLock : BaseItem, IKPItem
    {
        [DataMember(Name = "KPTeam")]
        public string KPTeam { get; set; }
        [DataMember(Name = "GoalSet")]
        public string GoalSet { get; set; }
        [DataMember(Name = "Locked")]
        public bool Locked { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPTeam = item["KPTeam"].Value;
            this.GoalSet = item["GoalSet"].Value;
            this.Locked = ParseBool(item["Locked"].Value);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("KPTeam", new KPItem(this.KPTeam, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("GoalSet", new KPItem(this.GoalSet, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Locked", new KPItem(this.Locked, EntityConstants.ItemTypes.YESNO));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
