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
    /// <summary>
    /// KPCategoryL1 extends KPCategory
    /// </summary>
    [DataContract()]
    public class CategoryL1 : BaseItem, IKPItem
    {
        [DataMember(Name = "GoalSet")]
        public int GoalSetId { get; set; }

        public List<CategoryL2> CategoryL2s { get; set; }

        /// <summary>
        /// Implments IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.GoalSetId = ParseInt(item["GoalSet"].Value);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("GoalSet", new KPItem(this.GoalSetId, EntityConstants.ItemTypes.NUMBER));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
    /// <summary>
    /// KPCategoryL2 extends KPCategory
    /// </summary>
    [DataContract()]
    public class CategoryL2 : BaseItem, IKPItem
    {
        [DataMember(Name = "CategoryL1")]
        public int CategoryL1Id { get; set; }
        
        /// <summary>
        /// Implments IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.CategoryL1Id = ParseInt(item["CategoryL1"].Value);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("CategoryL1", new KPItem(this.CategoryL1Id, EntityConstants.ItemTypes.NUMBER));
            base.GetBaseProperties();
            return this.itemProperties;
        }   
    }

}
