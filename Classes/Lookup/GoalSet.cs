using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Microsoft.SharePoint;

namespace Amazon.Kingpin.WCF2.Classes.Lookup
{
    [DataContract()]
    public class GoalSet : BaseItem, IKPItem
    {
        public bool HasCL1 { get; set; }
        public bool HasCL2 { get; set; }

        public List<CategoryL1> CategoryL1s { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            //this.HasCL1 = ParseBool(item[""].Value);
            //this.HasCL1 = ParseBool(item[""].Value);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
