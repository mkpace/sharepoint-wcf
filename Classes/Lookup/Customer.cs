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
    public class Customer : BaseItem, IKPItem
    {

        [DataMember(Name = "KPDescription")]
        public string KPDescription { get; set; }

        /// <summary>
        /// Default param-less Ctor
        /// </summary>
        public Customer() 
        {
            // set values to -1 if they should not be added to SP list item
            //this.ParentId = -1;
            //this.OrderIndex = -1;
            //this.Children = new List<Team>();
        }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPDescription = KPUtilities.StripHTML((item.ContainsKey("KPDescription")) ? item["KPDescription"].Value : string.Empty, false);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("KPDescription", new KPItem(this.KPDescription, EntityConstants.ItemTypes.TEXT));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
