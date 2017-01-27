using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Repositories.Base;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Data.Access;

namespace Amazon.Kingpin.WCF2.Repositories
{
    /// <summary>
    /// Class creates object instances of type T
    /// This class implmenets abstract class BaseRepository
    /// All concrete repository classes for lookups extend this class
    /// </summary>
    public class LookupRepository<T> : BaseRepository<T> where T : IKPItem, new()
    {
        #region CTORs
        public LookupRepository() { }

        public LookupRepository(SPDataAccess dataAccess) 
        {
            this.dataAccess = dataAccess;
            this.Init();
        }

        public LookupRepository(SPDataAccess dataAccess, string listName)
        {
            this.ListName = listName;
            this.dataAccess = dataAccess;
        }
        #endregion

        /// <summary>
        /// Initialize Lookup list objects
        /// </summary>
        public override void Init()
        {
            if (!this.isInitialized)
            {
                this.Items = this.dataAccess.GetLookupObjects<T>(this.ListName);
                // here for caching - NOT IMPLEMENTED
                this.isInitialized = true;
            }
        }

        /// <summary>
        /// Get all items and populate Teams property
        /// This method does not create the hierarchy.
        /// Call GetTeamHierarchy to create the tree
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllItems()
        {
            EventLogger.WriteLine("GetAllItems completed");
            return this.GetAllItems(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetAllItems(string count)
        {
            EventLogger.WriteLine("GetAllItems completed");
            return this.dataAccess.GetLookupObjects<T>(this.ListName, count); ;
        }
        /// <summary>
        /// Add a new item to the Team list
        /// </summary>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public override T AddItem(T item)
        {
            KPListItem configObject = item.GetProperties();
            KPListItem newItem = this.dataAccess.AddNewLookupItem(this.ListName, configObject);
            item.SetProperties(newItem, this.ListName);
            return item;
        }

        /// <summary>
        /// Update a team item in the Team list
        /// </summary>
        /// <param name="kpTeam"></param>
        /// <returns></returns>
        public override T UpdateItem(T updateItem)
        {
            T listItem;
            int id;
            bool isKPID = false;
            // get the typed entity object
            if (updateItem.KPID > 0)
            {
                listItem = dataAccess.GetLookupObjectByKPID<T>(this.ListName, updateItem.KPID);
                id = updateItem.KPID;
                isKPID = true;
            }
            else
            {
                listItem = dataAccess.GetLookupObjectByID<T>(this.ListName, updateItem.ID);
                id = updateItem.ID;
            }

            // get the filtered updated item - only fields that have changed
            KPListItem updatedItem = this.UpdateLookupItemFields(listItem, updateItem);
            updatedItem = dataAccess.UpdateLookupItem(this.ListName, updatedItem, id, isKPID);
            updateItem.SetProperties(updatedItem, this.ListName);
            return updateItem;
        }

        /// <summary>
        /// Delete operation for Team list
        /// </summary>
        /// <param name="kpTeam"></param>
        /// <returns></returns>
        public override bool DeleteItem(T item)
        {
            return dataAccess.DeleteLookupItem(this.ListName, item.KPID);
        }

    }
}
