using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Repositories.Base
{
    /// <summary>
    /// Abstract class creates SPDataAccess instance
    /// and provides abstract properties and methods for
    /// both derived classes Lookups and Entities
    /// </summary>
    public abstract class BaseRepository<T> where T : IKPItem, new()
    {
        /// <summary>
        /// SPDataAccess instance
        /// </summary>
        internal SPDataAccess dataAccess;
        /// <summary>
        /// Cache timestamp to refresh - not implemented
        /// </summary>
        protected DateTime cacheTimestamp = new DateTime();
        /// <summary>
        /// Initialization flag
        /// </summary>
        protected bool isInitialized = false;
        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public virtual string ListName { get; protected set; }
        /// <summary>
        /// Relative Url to Team Site
        /// </summary>
        public virtual List<T> Items { get; set; }

        /// <summary>
        /// Initialize Entity list objects
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Add a new item to the Team list
        /// </summary>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public abstract T AddItem(T configItem);

        /// <summary>
        /// Update a team item in the Team list
        /// </summary>
        /// <param name="kpTeam"></param>
        /// <returns></returns>
        public abstract T UpdateItem(T item);

        /// <summary>
        /// Delete operation for Team list
        /// </summary>
        /// <param name="kpTeam"></param>
        /// <returns></returns>
        public abstract bool DeleteItem(T item);

        /// <summary>
        /// Checks the existing (listItem) item against the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listItem">Current (existing) list item</param>
        /// <param name="entityItem">Entity with changes/updates</param>
        /// <returns>A new KPListItem with only the changed fields</returns>
        protected KPListItem UpdateItemFields<KPE>(KPE listItem, KPE entityItem) where KPE : IKPEntity
        {
            KPListItem updateItem = entityItem.GetProperties();
            KPListItem originalItem = listItem.GetProperties();
            return KPUtilities.UpdateItemFields(updateItem, originalItem);
        }

        protected KPListItem UpdateLookupItemFields<KPI>(KPI listItem, KPI entityItem) where KPI : IKPItem
        {
            KPListItem updateItem = entityItem.GetProperties();
            KPListItem originalItem = listItem.GetProperties();
            return KPUtilities.UpdateItemFields(updateItem, originalItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateItem"></param>
        /// <param name="originalItem"></param>
        /// <returns></returns>
        //private KPListItem UpdateItemFields(KPListItem updateItem, KPListItem originalItem)
        //{
        //    KPListItem updatedItem = new KPListItem();
        //    string fieldValue = string.Empty;
        //    foreach (KeyValuePair<string, KPItem> kvp in originalItem)
        //    {
        //        if (kvp.Value.Type == EntityConstants.ItemTypes.NOTE)
        //            fieldValue = KPUtilities.StripHTML(kvp.Value.Value, false);
        //        else
        //            fieldValue = kvp.Value.Value;

        //        // check for ignored fields
        //        if (!this.IgnoredFields(kvp.Key))
        //        {
        //            // check if fields values are the same
        //            if (updateItem[kvp.Key].Value != fieldValue)
        //            {
        //                kvp.Value.Value = updateItem[kvp.Key].Value;
        //                // if changed then add to updatedItem fields
        //                updatedItem.Add(kvp.Key, kvp.Value);
        //            }
        //        }
        //    }

        //    return updatedItem;
        //}

        /// <summary>
        /// Checks for fields that should be ignored when updating an item
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        //protected bool IgnoredFields(string fieldName)
        //{
        //    List<string> ignoredFields = new List<string>() { 
        //        "KPID", "KPGUID", "Created", "CreatedBy", "Modified", "ModifiedBy"
        //    };
        //    return ignoredFields.Contains(fieldName);
        //}

    }

}
