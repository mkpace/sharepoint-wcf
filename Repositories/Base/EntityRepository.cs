using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Utilities;
using System;
using Amazon.Kingpin.WCF2.DataPersistence.Access;

namespace Amazon.Kingpin.WCF2.Repositories.Base
{
    /// <summary>
    /// Class creates object instances of type T
    /// This class implmenets abstract class BaseRepository
    /// All concrete repository classes for entities extend this class
    /// </summary>
    public class EntityRepository<T> : BaseRepository<T> where T : IKPEntity, new()
    {

        #region CTORs        
        public EntityRepository() { }

        public EntityRepository(SPDataAccess dataAccess, string listName)
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
            // NOT IMPLEMENTED
        }

        /// <summary>
        /// Get all items and populate Teams property
        /// This method does not create the hierarchy.
        /// Call GetTeamHierarchy to create the tree
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllItems(KPUser kpUser)
        {
            List<T> items = new List<T>();

            // get the rest of the items from the user's additional teams list
            foreach (Team team in kpUser.Teams)
            {
                items.AddRange(this.GetAllItemsByTeam(team.ID));
            }
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpUser"></param>
        /// <returns></returns>
        public T GetItemById(string entityKPID)
        {
            // get teamId from index
            EntityIndex index = this.dataAccess.GetLookupObjectByID<EntityIndex>("EntityIndex", Int32.Parse(entityKPID));
            // get team url
            Team team = this.dataAccess.Teams.Find(t => t.KPID == index.KPTeamId.Value);
            if(team == null)
            {
                throw new Exception(string.Format("Entity index lookup failed: KPID {0} not found. Endpoint pattern '/Entity/[entityName]/[entityKPID]' expected. Exception thrown at EntityRepository.GetItemById", entityKPID));
            }
            // get item from team list
            T item = this.dataAccess.GetEntityObjectByKPID<T>(team.SiteUrl, this.ListName, index.ID);
            EventLogger.WriteLine("Found object");
            return item;
        }

        public List<T> GetItemVersionsById(string entityKPID)
        {
            // get teamId from index
            EntityIndex index = this.dataAccess.GetLookupObjectByID<EntityIndex>("EntityIndex", Int32.Parse(entityKPID));
            // get team url
            Team team = this.dataAccess.Teams.Find(t => t.KPID == index.KPTeamId.Value);
            if (team == null)
            {
                throw new Exception(string.Format("Entity index lookup failed: KPID {0} not found. Endpoint pattern '/Entity/[entityName]/[entityKPID]' expected. Exception thrown at EntityRepository.GetItemById", entityKPID));
            }
            // get item from team list
            List<T> items = this.dataAccess.GetEntityObjectVersionsByKPID<T>(team.SiteUrl, this.ListName, index.ID);
            EventLogger.WriteLine("Found object");
            return items;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpUser"></param>
        /// <returns></returns>
        public T GetFileByName(string teamId, string fileName)
        {
            // get team url
            Team team = this.dataAccess.Teams.Find(t => t.KPID == Int32.Parse(teamId));
            // get item from team list
            T item = this.dataAccess.GetLibraryObjectByName<T>(team.SiteUrl, this.ListName, fileName);
            EventLogger.WriteLine("Found object");
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpUser"></param>
        /// <returns></returns>
        public List<T> GetAllFiles(KPUser kpUser)
        {
            List<T> items = new List<T>();
            // initialize object list
            if (!this.isInitialized)
            {
                // get the rest of the items from the user's additional teams list
                foreach (Team team in kpUser.Teams)
                {
                    items.AddRange(this.GetAllFilesByTeam(team.ID));
                }
                // here for caching - NOT IMPLEMENTED
                this.isInitialized = true;
            }
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpUser"></param>
        /// <returns></returns>
        public List<T> GetAllItemsByTeam(int teamId)
        {
            Team team = this.dataAccess.Teams.Find(t => t.KPID == teamId);
            List<T> items = new List<T>();
            EventLogger.WriteLine("Fetching TeamUrl: {0}; ID:{1}", team.SiteUrl, team.ID);

            // initialize object list
            items = dataAccess.GetEntityObjects<T>(team.SiteUrl, this.ListName);
            EventLogger.WriteLine("Found {0} objects", items.Count);

            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpUser"></param>
        /// <returns></returns>
        public List<T> GetAllFilesByTeam(int teamId)
        {
            List<T> items = new List<T>();
            // initialize object list
            Team team = this.dataAccess.Teams.Find(t => t.KPID == teamId);
            EventLogger.WriteLine("Fetching TeamUrl: {0}; ID:{1}", team.SiteUrl, team.ID);
            List<T> objects = dataAccess.GetLibraryObjects<T>(team.SiteUrl, this.ListName);
            EventLogger.WriteLine("Found {0} objects", objects.Count);
            items.AddRange(objects);
            return items;
        }

        /// <summary>
        /// Add a new item to the Team list
        /// </summary>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public override T AddItem(T item)
        {
            string teamUrl;
            if (item.KPTeamId == null) // Default to common
                teamUrl = "/common";
            else
                teamUrl = dataAccess.CurrentUser.GetTeamUrl(item.KPTeamId.Value);
            KPListItem entityItem = item.GetProperties();
            KPListItem newItem = dataAccess.AddNewEntityItem(teamUrl, this.ListName, entityItem);
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
            // don't use this but required by UpdateEntityItem
            bool isUpdated = false;
            // get the team urlstring teamUrl;
            string teamUrl;
            if (updateItem.KPTeamId == null) // Default to common
                teamUrl = "/common";
            else
                teamUrl = dataAccess.CurrentUser.GetTeamUrl(updateItem.KPTeamId.Value);
            
            // get the typed entity object
            T listItem = dataAccess.GetEntityObjectByKPID<T>(teamUrl, this.ListName, updateItem.KPID);

            // get the filtered updated item - only fields that have changed
            KPListItem updatedItem = this.UpdateItemFields(listItem, updateItem);
            updatedItem = dataAccess.UpdateEntityItem(teamUrl, this.ListName, updatedItem, updateItem.KPID, out isUpdated);
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
