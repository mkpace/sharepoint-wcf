using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class KeyInsightInnovation : BaseEntity, IKPEntity
    {

        [DataMember(Name = "Theme")]
        public int? Theme { get; set; }

        [DataMember(Name = "ProjectOwner")]
        public string ProjectOwner { get; set; }
        

        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public override void SetProperties(KPListItem item, string listName)
        {
            this.Theme = CheckNullInt("Theme", item);
            this.ProjectOwner = (item.ContainsKey("ProjectOwner")) ? item["ProjectOwner"].Value : null;
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
            this.itemProperties.Add("Theme", new KPItem(this.Theme, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("ProjectOwner", new KPItem(this.ProjectOwner, EntityConstants.ItemTypes.TEXT));
            base.GetEntityProperties();
            return this.itemProperties;
        }
    }
}
