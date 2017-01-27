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

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class Report : BaseItem, IKPEntity
    {
        // Public Serializable Properties
        // are marked with this attribute to declare which
        // properties are to be serialized and what their 
        // JSON representation should be. If we wish to create
        // proper camel-cased names then we would change these attribute names
        [DataMember(Name = "KPType", EmitDefaultValue = false)]
        public string KPType { get; set; }

        // needed for implementation of IKPEntity
        public int? KPTeamId { get; set; }

        [DataMember(Name = "KPTeam", EmitDefaultValue = false)]
        public string KPTeam { get; set; }
        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPType = item["KPType"].Value;
            this.KPTeam = item["KPTeam"].Value;
            base.SetBaseProperties(item, listName);
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
            this.itemProperties.Add("KPType", new KPItem(this.KPType, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPTeam", new KPItem(this.KPTeam, EntityConstants.ItemTypes.TEXT));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
