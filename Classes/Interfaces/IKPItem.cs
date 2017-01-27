using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;

namespace Amazon.Kingpin.WCF2.Classes.Base
{
    public interface IKPItem
    {
        /// <summary>
        /// SharePoint ID - needed for lists that do not have KPID
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// KPID is equivalent to SP.ID
        /// </summary>
        int KPID { get; set; }
        /// <summary>
        /// KPID is equivalent to SP.GUID
        /// </summary>
        string KPGUID { get; set; }
        /// <summary>
        /// Set the properties on the KPListItem (Dictionary)
        /// </summary>
        /// <param name="item"></param>
        void SetProperties(KPListItem item, string listName);
        /// <summary>
        /// Get the properties and value of an 
        /// object instance and return the KPListItem
        /// </summary>
        /// <returns></returns>
        KPListItem GetProperties();

    }
}
