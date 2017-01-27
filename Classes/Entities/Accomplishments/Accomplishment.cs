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
    public class Accomplishment : BaseEntity, IKPEntity
    {
        // Public Serializable Properties
        // are marked with this attribute to declare which
        // properties are to be serialized and what their 
        // JSON representation should be. If we wish to create
        // proper camel-cased names then we would change these attribute names
        [DataMember(Name = "IsAccomplishment")]
        public bool IsAccomplishment { get; set; }

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
            this.IsAccomplishment = ParseBool(item["IsAccomplishment"].Value);
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
            this.itemProperties.Add("IsAccomplishment", new KPItem(this.IsAccomplishment, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("Theme", new KPItem(this.Theme, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("ProjectOwner", new KPItem(this.ProjectOwner, EntityConstants.ItemTypes.TEXT));
            base.GetEntityProperties();
            return this.itemProperties;
        }
    }
}
