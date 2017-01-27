using System;
using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Utilities;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Security;
using Microsoft.SharePoint;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Importing.Utilities;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using System.IO;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Diagnostics;
using System.Text.RegularExpressions;
using Amazon.Kingpin.WCF2.DataPersistence.Access;
using System.Linq;
using System.Text;
using Amazon.Kingpin.WCF2.Repositories;
using System.Net;
using Amazon.Kingpin.WCF2.Importing.Utilities;

namespace Amazon.Kingpin.WCF2.Data.Access
{
    /// <summary>
    /// The SPDataAccess class the main entry point to access SP Data.
    /// SPDataAccess is tightly coupled to the data source API in
    /// this case our data source is the SharePoint API (SPDataProvider).
    /// This class should be used to perform all SharePoint operations.
    /// The SPDataProvider is a low-level provider for SP specific operations and
    /// should not be used unless a specific operation is not avialable from this class.
    /// THe SPDataAccess class should created and instantiated once per session so
    /// as to allow caching of all calls to the SPDatProvider.
    /// </summary>
    public class SPDataAccess
    {
        #region Private member fields
        const string ITEM_QUERY = "<Where><Eq><FieldRef Name=\"{0}\"/><Value Type=\"{1}\">{2}</Value></Eq></Where>";

        private KPTimer timer = new KPTimer();

        static bool KPIDLock = false;
        /// <summary>
        /// SPHandler instance
        /// </summary>
        private SPDataProvider spDataProvider = null;
        /// <summary>
        /// A cache of user objects - items created when GetUserAccount is called
        /// </summary>
        private static Dictionary<string, KPUser> userCache = new Dictionary<string, KPUser>();

        #endregion

        #region Ctor
        /// <summary>
        /// Default Ctor
        /// </summary>
        public SPDataAccess()
        {
            this.timer.Start("Initializing SPDataAccess");
            // initialize instance members
            this.spDataProvider = new SPDataProvider();
            this.LoadCoreLookups();
        }

        /// <summary>
        /// TODO: need to test this initialization and refactor
        /// the entity methods that also call SetCurrentUser
        /// </summary>
        /// <param name="initializeUser"></param>
        public SPDataAccess(bool initializeUser) : this()
        {
            this.InitializeCurrentUser();
        }

        #endregion

        #region Dtor
        /// <summary>
        /// Class destructor used to clean-up any open
        /// SharePoint resources - namely SPCollection
        /// </summary>
        ~SPDataAccess()
        {
            EventLogger.WriteLine("SPDataAccess destructor called");
            //this.spDataProvider.Dispose();
        }
        #endregion

        #region public member properties
        public KPTimer Timer
        {
            get { return this.timer; }
        }
        /// <summary>
        /// The configuration list items - this should be
        /// UserConfiguration or UserProfiles
        /// </summary>
        public List<ConfigList> ConfigList { get; set; }
        /// <summary>
        /// Teams
        /// </summary>
        public List<Team> Teams { get; set; }

        public static List<Team> TeamCache { get; set; }

        /// <summary>
        /// Gets the current user
        /// </summary>
        public KPUser CurrentUser { get; set; }

        public string HostUrl { get { return this.spDataProvider.HostUrl; } }

        #endregion

        #region public member methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public DateTime CheckListUpdateTimestamp(string siteUrl, string entityName)
        {
            DateTime dateTime;
            bool isList = true;
            switch (entityName)
            {
                case "KPDocuments":
                case "KPReports":
                    isList = false;
                    break;
            }

            // list and library urls are different
            dateTime = this.spDataProvider.CheckListUpdateTimestamp(siteUrl, entityName, isList);
            return dateTime;
        }

        #region Lookups
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="kpGUID"></param>
        /// <returns></returns>
        public T GetLookupObjectByKPID<T>(string listName, int KPID) where T : IKPItem, new()
        {
            SPListItem spListItem = this.GetKPListItemByKPID("/Common", listName, KPID);
            KPListItem item = CreateKPListItem(spListItem);
            T newEntity = new T();
            newEntity.SetProperties(item, listName);
            return newEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T GetLookupObjectByID<T>(string listName, int ID) where T : IKPItem, new()
        {
            SPListItem spListItem = this.GetKPListItemByID("/Common", listName, ID);
            KPListItem item = CreateKPListItem(spListItem);
            T newEntity = new T();
            newEntity.SetProperties(item, listName);
            return newEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T GetLookupObjectByField<T>(string listName, string fieldName, string fieldType, string fieldValue) where T : IKPItem, new()
        {
            SPListItem spListItem = this.GetKPCommonListItemCAML(listName, fieldName, fieldType, fieldValue);
            KPListItem item = CreateKPListItem(spListItem);
            T newEntity = new T();
            newEntity.SetProperties(item, listName);
            return newEntity;
        }

        /// <summary>
        /// Method creates a list of objects of type T
        /// This method generates the object instance from 
        /// the source DataProvider. Fetches from specififed TeamUrl/ListName
        /// </summary>
        /// <typeparam name="T">Entity or Item Type</typeparam>
        /// <param name="listName">Name of the target List</param>
        /// <returns></returns>
        public List<T> GetLookupObjects<T>(string listName) where T : IKPItem, new()
        {
            return this.GetLookupObjects<T>(listName, null);
        }

        /// <summary>
        /// Creates list of objects limited by count
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetLookupObjects<T>(string listName, string count) where T : IKPItem, new()
        {
            List<KPListItem> entities = this.CreateEntityObjects("/Common", listName, count);
            List<T> list = new List<T>();
            foreach (KPListItem entity in entities)
            {
                T newEntity = new T();
                newEntity.SetProperties(entity, listName);
                list.Add(newEntity);
            }
            return list;
        }

        /// <summary>
        /// Gets the lookup items as a List<KPListItem> 
        /// without converting to a specified entity type
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        public List<KPListItem> GetLookupItems(string listName)
        {
            List<KPListItem> entities = this.CreateEntityObjects("/Common", listName, string.Empty);
            return entities;
        }

        /// <summary>
        /// Add a new item to the specified lookup list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public KPListItem AddNewLookupItem(string listName, KPListItem listItem)
        {
            // call spHandler to get the new SPListItem
            SPListItem newItem = this.spDataProvider.GetNewLookupListItem(listName);
            // enumerate the fields in KPListItem to populate SPListItem
            foreach (KeyValuePair<string, KPItem> item in listItem)
            {
                // cannot create new item with ID or GUID
                if (item.Key != "ID" && item.Key != "GUID"
                    && item.Key != "CreatedBy" && item.Key != "ModifiedBy"
                    && item.Key != "Created" && item.Key != "Modified")
                {
                    // check for existing fields - some Lookups (e.g. EventLogging don't have KPID)
                    if (newItem.Fields.ContainsField(item.Key))
                    {
                        newItem[item.Key] = item.Value.Value;
                    }
                }
            }
            // add (update) the list item
            newItem.Update();
            try
            {
                // add the newly created ID and as KPID
                listItem["KPID"] = new KPItem(newItem["ID"].ToString(), EntityConstants.ItemTypes.NUMBER);
                // add to new item also
                newItem["KPID"] = listItem["KPID"].Value;
                // update the item without updating any metadata (Created, Version, etc.)
                newItem.SystemUpdate(false);
            }
            catch(Exception)
            {
                /* eat this exception for now - TODO: need to log this exception */
            }

            // add the new id to the return list (object)
            listItem["ID"] = new KPItem(newItem["ID"].ToString(), EntityConstants.ItemTypes.NUMBER);
            // add the created date to the return list (object)
            listItem["Created"] = new KPItem(newItem["Created"].ToString(), EntityConstants.ItemTypes.DATETIME);
            // add the updated date to the return list (object)
            listItem["Modified"] = new KPItem(newItem["Modified"].ToString(), EntityConstants.ItemTypes.DATETIME);

            // return the new instance as KPListItem which has the new item ID
            return listItem;
        }

        /// <summary>
        /// Update a lookup item. This method supports saving by both KPID and ID
        /// since some lists don't have KPID we need to support ID as well.
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="listItem"></param>
        /// <param name="ID"></param>
        /// <param name="isKPID"></param>
        /// <returns></returns>
        public KPListItem UpdateLookupItem(string listName, KPListItem listItem, int ID, bool isKPID)
        {
            SPListItem spListItem;
            if(isKPID)
                spListItem = this.GetKPListItemByKPID("/Common", listName, ID);
            else
                spListItem = this.GetKPListItemByID("/Common", listName, ID);

            foreach (KeyValuePair<string, KPItem> kvp in listItem)
            {
                spListItem[kvp.Key] = kvp.Value.Value;
            }
            spListItem.Update();

            // creates the partial item
            return this.CreateKPListItem(spListItem);
        }

        /// <summary>
        /// TODO: not implemented - we don't delete any items in Kingpin
        /// only soft-deletes are supported - this should set that flag
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="itemKPGUID"></param>
        /// <returns></returns>
        public bool DeleteLookupItem(string listName, int itemKPGUID)
        {
            throw new NotImplementedException("Delete not implemented");
        }

        /// <summary>
        /// Returns a specific item queried by KPGUID
        /// </summary>
        /// <param name="KPGUID"></param>
        /// <returns></returns>
        public int GetIndexedItem(string KPGUID)
        {
            int teamId = -1;
            string camlQuery = string.Format(ITEM_QUERY, "KPGUID", EntityConstants.ItemTypes.TEXT, KPGUID);
            // get index lookup info for item to peform fetch
            using (this.spDataProvider)
            {
                SPListItem indexItem = this.spDataProvider.GetTeamListItem("/Common", "KPIndex", camlQuery);
                teamId = int.Parse(indexItem["KPTeam"].ToString());
            }
            return teamId;
        }
        #endregion

        #region  Entities
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="kpGUID"></param>
        /// <returns></returns>
        public T GetEntityObjectByKPID<T>(string teamUrl, string listName, int KPID) where T : IKPEntity, new()
        {
            DateTime lastModifiedDt = this.CheckListUpdateTimestamp(teamUrl, listName);
            T newEntity = new T();

            if (EntityCache<T>.IsValid(teamUrl, listName, lastModifiedDt))
            {
                // get data from the cache
                newEntity = EntityCache<T>.Entities[teamUrl][listName].Find(i => i.KPID == KPID);
            }
            else
            {
                SPListItem spListItem = this.GetKPListItemByKPID(teamUrl, listName, KPID);
                KPListItem item = CreateKPListItem(spListItem);
                newEntity.SetProperties(item, listName);
            }

            return newEntity;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="KPID"></param>
        /// <returns></returns>
        public List<T> GetEntityObjectVersionsByKPID<T>(string teamUrl, string listName, int KPID) where T : IKPEntity, new()
        {
            List<KPItemVersion> versions = new List<KPItemVersion>();

            SPListItem spListItem = this.GetKPListItemByKPID(teamUrl, listName, KPID);

            //KPListItem kpItem = CreateKPListItem(spListItem);
            KPListItem kpItem = new KPListItem();

            // remove HTML tags from all fields
            kpItem = CleanKPListItem(kpItem);

            List<T> entityVersions = new List<T>();

            SPListItemVersion previousVersion = null;

            // reverse the order of the version creation
            for (int i = spListItem.Versions.Count - 1; i > -1; i--)
            {
                KPItemVersion version = null;

                SPListItemVersion currentVersion = spListItem.Versions[i];

                // our first version so we should create a new object here
                if (i == spListItem.Versions.Count - 1)
                {
                    kpItem = CreateInitialVersion(currentVersion);

                    if (!kpItem.ContainsKey("KPID"))
                        // some items do not have a kpid in the initial item
                        kpItem.Add("KPID", new KPItem(spListItem["KPID"].ToString(), "Number"));
                    else
                        kpItem["KPID"].Value = spListItem["KPID"].ToString();
                             

                    if (!kpItem.ContainsKey("Created"))
                        kpItem.Add("Created", new KPItem(currentVersion.Created, "DateTime"));
                    else
                        kpItem["Created"].Value = currentVersion.Created.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");

                    if (!kpItem.ContainsKey("CreatedBy"))
                        kpItem.Add("CreatedBy", new KPItem(currentVersion.CreatedBy.User.Email.Split('@')[0], "Text"));
                    else
                        kpItem["CreatedBy"].Value = currentVersion.CreatedBy.User.Email.Split('@')[0];


                    if (!kpItem.ContainsKey("Modified"))
                        kpItem.Add("Modified", new KPItem(currentVersion.Created, "DateTime"));
                    else
                        kpItem["Modified"].Value = currentVersion.Created.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");

                    if (!kpItem.ContainsKey("ModifiedBy"))
                        kpItem.Add("ModifiedBy", new KPItem(currentVersion.CreatedBy.User.Email.Split('@')[0], "Text"));
                    else
                        kpItem["ModifiedBy"].Value = currentVersion.CreatedBy.User.Email.Split('@')[0];


                    T newEntity = new T();

                    newEntity.SetProperties(kpItem, listName);
                    entityVersions.Add(newEntity);

                }
                else
                {
                    version = CreateKPListItemVersion(spListItem, currentVersion, previousVersion);
                }

                if(version != null && version.Fields.Count > 0)
                {
                    version.Modified = currentVersion.Created.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
                    version.ModifiedBy = currentVersion.CreatedBy.User.Email.Split('@')[0];
                    version.Number = Convert.ToInt32(Convert.ToDecimal(currentVersion.VersionLabel));
                    versions.Add(version);
                }

                previousVersion = currentVersion;
            }
            
            // loop through the versions creating the previous item
            foreach (KPItemVersion version in versions)
            {
                T newEntity = new T();

                kpItem = this.SetVersionProperties(kpItem, version);

                newEntity.SetProperties(kpItem, listName);
                entityVersions.Add(newEntity);
            }

            return entityVersions;
        }

        private KPListItem CreateInitialVersion(SPListItemVersion version)
        {
            KPListItem kpListItem = new KPListItem();
            foreach (SPField field in version.Fields)
            {
                // do not get hidden, read-only, or base-type fields
                if (field.Hidden && field.ReadOnlyField && field.FromBaseType)
                {
                    continue;
                }
                // skip KPGUIDs
                if (field.StaticName == "KPGUID")
                {
                    continue;
                }

                if(version[field.StaticName] != null)
                {
                    string fieldValue = KPUtilities.StripHTML(version[field.StaticName].ToString(), true);
                    KPItem kpItem = new KPItem(fieldValue, field.Type.ToString());
                    kpListItem.Add(field.StaticName, kpItem);
                }
                else
                {
                    KPItem kpItem = new KPItem(string.Empty, field.Type.ToString());
                    kpListItem.Add(field.StaticName, kpItem);
                }
            }
            return kpListItem;
        }

        private KPListItem SetVersionProperties(KPListItem kpItem, KPItemVersion version)
        {
            if (!kpItem.ContainsKey("Modified"))
                kpItem.Add("Modified", new KPItem(version.Modified, "DateTime"));
            else
                kpItem["Modified"].Value = version.Modified;

            if (!kpItem.ContainsKey("ModifiedBy"))
                kpItem.Add("ModifiedBy", new KPItem(version.ModifiedBy, "Text"));
            else
                kpItem["ModifiedBy"].Value = version.ModifiedBy;

            foreach(KeyValuePair<string,string> kvp in version.Fields)
            {
                if(kpItem.ContainsKey(kvp.Key))
                {
                    kpItem[kvp.Key].Value = kvp.Value;
                }
            }

            return kpItem;
        }

        /// <summary>
        /// Method creates a list of objects of type T
        /// This method generates the object instance from 
        /// the source DataProvider. Fetches from specififed TeamUrl/ListName
        /// </summary>
        /// <typeparam name="T">Entity or Item Type</typeparam>
        /// <param name="teamUrl">Url of Team Site to fecth objects from</param>
        /// <param name="listName">Name of the target List</param>
        /// <returns></returns>
        public List<T> GetEntityObjects<T>(string teamUrl, string listName) where T : IKPEntity, new()
        {
            List<T> list = new List<T>();

            DateTime lastModifiedDt = this.CheckListUpdateTimestamp(teamUrl, listName);

            if (EntityCache<T>.IsValid(teamUrl, listName, lastModifiedDt))
            {
                // get data from the cache
                list = EntityCache<T>.Entities[teamUrl][listName];
            }
            else
            {
                // get the rest of the items from the user's additional teams list
                List<KPListItem> entities = this.CreateEntityObjects(teamUrl, listName);
                try
                {
                    foreach (KPListItem entity in entities)
                    {
                        T newEntity = new T();
                        newEntity.SetProperties(entity, listName);
                        list.Add(newEntity);
                    }
                }
                catch(Exception ex)
                {
                    string errMsg = string.Format("Error setting properties on entity. Team: {0}; Entity: {1}: {2}", teamUrl, listName, ex.Message);
                    throw new Exception(errMsg, ex.InnerException);
                }
                // cache items
                EntityCache<T>.UpdateCache(teamUrl, listName, list, lastModifiedDt);
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public List<T> GetLibraryObjects<T>(string teamUrl, string listName) where T : IKPEntity, new()
        {
            EventLogger.WriteLine("Fetching TeamUrl: {0}", teamUrl);

            List<T> list = new List<T>();

            DateTime lastModifiedDt = this.CheckListUpdateTimestamp(teamUrl, listName);

            if (EntityCache<T>.IsValid(teamUrl, listName, lastModifiedDt))
            {
                // get data from the cache
                list = EntityCache<T>.Entities[teamUrl][listName];
            }
            else
            {
                // get the rest of the items from the user's additional teams list
                List<KPListItem> entities = this.CreateLibraryObjects(teamUrl, listName);
                foreach (KPListItem entity in entities)
                {
                    T newEntity = new T();
                    newEntity.SetProperties(entity, listName);
                    list.Add(newEntity);
                }

                // cache items
                EntityCache<T>.UpdateCache(teamUrl, listName, list, lastModifiedDt);
            }

            EventLogger.WriteLine("Found {0} objects", list.Count);

            return list;
        }

        /// <summary>
        /// Hard-coded to retrive a document (KPDocuments) by the "Name" field.
        /// TODO: need to make this more flexible if we want to get any document from a library
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public T GetLibraryObjectByName<T>(string teamUrl, string libraryName, string fileName) where T : IKPEntity, new()
        {
            EventLogger.WriteLine("Fetching TeamUrl: {0}", teamUrl);
            fileName += (fileName.IndexOf(".docx") > -1) ? string.Empty : ".docx";
            SPListItem spListItem = this.GetLibraryObjectByName(teamUrl, libraryName, "FileLeafRef", "File", fileName);
            KPListItem item = CreateKPListItem(spListItem);
            T newEntity = new T();
            newEntity.SetProperties(item, libraryName);

            EventLogger.WriteLine("Found object");
            return newEntity;
        }
       
        /// <summary>
        /// Method creates a list of objects of type T
        /// This method generates the object instance from 
        /// the source DataProvider. Fetches from specififed TeamUrl/ListName
        /// </summary>
        /// <typeparam name="T">Entity or Item Type</typeparam>
        /// <param name="teamUrl">Url of Team Site to fecth objects from</param>
        /// <param name="listName">Name of the target List</param>
        /// <returns></returns>
        public List<KPListItem> GetKPListItems(int teamId, string listName)
        {
            string teamUrl = this.CurrentUser.GetTeamUrl(teamId);
            return this.CreateEntityObjects(teamUrl, listName);
        }

        /// <summary>
        /// Adds a new item to the team/entity list
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public KPListItem AddNewEntityItem(string teamUrl, string listName, KPListItem listItem)
        {
            // call spHandler to get the new SPListItem
            SPListItem newItem = this.spDataProvider.GetNewTeamListItem(teamUrl, listName);
            // enumerate the fields in KPListItem to populate SPListItem
            foreach (KeyValuePair<string, KPItem> item in listItem)
            {
                // cannot create new item with ID or GUID
                if (item.Key != "ID" && item.Key != "GUID"
                    && item.Key != "CreatedBy" && item.Key != "ModifiedBy"
                    && item.Key != "Created" && item.Key != "Modified"
                    && !string.IsNullOrEmpty(item.Value.Value))
                {
                    string value = KPUtilities.ConvertToSPType(item.Value.Value, item.Value.Type);
                    newItem[item.Key] = value;
                }
            }
            // add (update) the list item
            newItem.Update();

            // fetch new KPID from Common/KPCounter
            string kpid = string.Empty;
            if (listItem.ContainsKey("KPTeam")) // is this a regular entity
                kpid = this.GetNewKPID(listItem["KPTeam"].Value.ToString(), listName).ToString();
            else // view / checkpoint, not indexed to any team site
                kpid = this.GetNewKPID(string.Empty, listName).ToString();

            // set the new item's KPID
            newItem["KPID"] = kpid;
            // also set the new item's KPGUID
            newItem["KPGUID"] = newItem["GUID"].ToString().Replace("{", string.Empty).Replace("}", string.Empty);
            // update the item without updating any metadata (Created, Version, etc.)
            newItem.SystemUpdate(false);

            // add the new id to the return list (object)
            listItem["ID"] = new KPItem(newItem["ID"].ToString(), EntityConstants.ItemTypes.NUMBER);
            // add the newly created ID and as KPID to the list item
            listItem["KPID"] = new KPItem(kpid.ToString(), EntityConstants.ItemTypes.TEXT);
            // add the new id to the return list (object)
            listItem["KPGUID"] = new KPItem(newItem["KPGUID"].ToString(), EntityConstants.ItemTypes.TEXT);
            // add the created date to the return list (object)
            listItem["Created"] = new KPItem(newItem["Created"].ToString(), EntityConstants.ItemTypes.DATETIME);
            // add the updated date to the return list (object)
            listItem["Modified"] = new KPItem(newItem["Modified"].ToString(), EntityConstants.ItemTypes.DATETIME);
            // add the created by (user)
            listItem["CreatedBy"] = new KPItem(this.GetUserAliasById(newItem["Created By"].ToString(), teamUrl), EntityConstants.ItemTypes.TEXT);
            // add the updated by (user)
            listItem["ModifiedBy"] = new KPItem(this.GetUserAliasById(newItem["Modified By"].ToString(), teamUrl), EntityConstants.ItemTypes.TEXT);

            // return the new instance as KPListItem which has the new item ID
            return listItem;
        }

        /// <summary>
        /// HACK-TODO: this is a duplicate of the other method
        /// this method checks and returns a boolean - need to 
        /// consolidate these methods so all Entity updates use the same one.
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="item"></param>
        /// <param name="KPID"></param>
        /// <returns></returns>
        public KPListItem UpdateEntityItem(string teamUrl, string listName, KPListItem item, int KPID, out bool isUpdated)
        {
            // initialize out parameter
            isUpdated = false;
            // get the SPListItem by KPID
            SPListItem listItem = this.GetKPListItemByKPID(teamUrl, listName, KPID);
            // convert this original item to a KPListItem to compare
            KPListItem originalItem = this.CreateKPListItem(listItem);
            // compare fields for both items
            KPListItem updateItem = KPUtilities.UpdateItemFields(item, originalItem);

            try
            {
                // check for any updated fields
                if (updateItem.Count > 0)
                {
                    // replace item property values with new values
                    foreach (KeyValuePair<string, KPItem> kvp in updateItem)
                    {
                        string value = KPUtilities.ConvertToSPType(kvp.Value.Value, kvp.Value.Type);
                        listItem[kvp.Key] = value;
                    }
                    // updates the list item
                    listItem.Update();
                    isUpdated = true;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error updating: {0}, StackTrace: {1}", ex.Message, ex.StackTrace), ex.InnerException);
            }

            // creates the partial item
            return this.CreateKPListItem(listItem);
        }

        /// <summary>
        /// Updates a list item in the team/entity list
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        //public KPListItem UpdateEntityItem(string teamUrl, string listName, KPListItem item, int KPID)
        //{
        //    // get the SPListItem by KPID
        //    SPListItem listItem = this.GetKPListItemByKPID(teamUrl, listName, KPID);
        //    // convert this original item to a KPListItem to compare
        //    KPListItem originalItem = this.CreateKPListItem(listItem);
        //    // compare fields for both items
        //    KPListItem updateItem = KPUtilities.UpdateItemFields(item, originalItem);

        //    try
        //    {
        //        // check for any updated fields
        //        if (updateItem.Count > 0)
        //        {
        //            // replace item property values with new values
        //            foreach (KeyValuePair<string, KPItem> kvp in updateItem)
        //            {
        //                listItem[kvp.Key] = kvp.Value.Value;
        //            }
        //            // updates the list item
        //            listItem.Update();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format("Error updating: {0}, StackTrace: {1}", ex.Message, ex.StackTrace), ex.InnerException);
        //    }

        //    // creates the partial item
        //    return this.CreateKPListItem(listItem);
        //}

        /// <summary>
        /// TODO: not implemented - we don't delete any items in Kingpin
        /// only soft-deletes are supported - this should set that flag
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="itemKPGUID"></param>
        /// <returns></returns>
        public bool DeleteEntityItem(string teamUrl, string listName, int itemKPGUID)
        {
            throw new NotImplementedException("Delete not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        public void SaveFileToLibrary(int teamId, string fileName, byte[] fileStream)
        {
            string teamUrl = this.CurrentUser.GetTeamUrl(teamId);
            this.spDataProvider.SaveFileToLibrary(teamUrl, fileName, fileStream, true);
        }
        #endregion

        /// <summary>
        /// Get the file stream from specified library and file name
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="libraryName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public MemoryStream GetFileStream(int teamId, string libraryName, string fileName)
        {
            string teamUrl = this.CurrentUser.GetTeamUrl(teamId);
            return GetFileStream(teamUrl, libraryName, fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="libraryName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public MemoryStream GetFileStream(string siteUrl, string libraryName, string fileName)
        {
            SPFile file = null;
            MemoryStream stream = null;
            string fileUrl = string.Format("{0}/{1}", libraryName, fileName);
            file = this.spDataProvider.GetTeamWeb(siteUrl).GetFile(fileUrl);
            byte[] fileBytes = file.OpenBinary();
            stream = new MemoryStream(fileBytes);
            return stream;
        }

        /// <summary>
        /// Gets/Sets the new KPID using KPCounter
        /// TODO: This will be replaced with the new index list
        /// </summary>
        /// <returns></returns>
        public int GetNewKPID()
        {
            string LIST_KPCOUNTER = "KPCounter";
            int kpid = -1;
            SPQuery query = new SPQuery();
            query.Query = string.Empty;
            query.RowLimit = 1;

            // TODO: need to handle this lock
            if (!SPDataAccess.KPIDLock)
            {
                // set the lock
                SPDataAccess.KPIDLock = true;

                SPList list = this.spDataProvider.GetLookupList(LIST_KPCOUNTER);
                SPListItem item = list.GetItemById(1);

                // increment the id
                kpid = int.Parse(item["Title"].ToString());
                kpid++;
                item["Title"] = kpid.ToString();
                // update the item
                item.Update();

                // release the lock
                SPDataAccess.KPIDLock = false;
            }
            // return the KPID
            return kpid;
        }

        /// <summary>
        /// Returns the new KPID from the EntityIndex list
        /// </summary>
        /// <param name="teamId">ID of the Team Item is associated with</param>
        /// <param name="itemId">ID of the Team Item is associated with</param>
        /// <param name="itemType">The Entity type of the item</param>
        /// <returns></returns>
        public int GetNewKPID(string teamId, string itemType)
        {
            string LIST_KPINDEX = "EntityIndex";
            int kpid = -1;
            SPListItem item = this.spDataProvider.GetNewLookupListItem(LIST_KPINDEX);
            item["Title"] = "-";
            item["KPTeam"] = teamId;
            item["KPType"] = itemType;
            item.Update();
            kpid = int.Parse(item["ID"].ToString());
            return kpid;
        }

        public Dictionary<string, string> GetEntityIndexByKPID(int kpid)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            SPListItem listItem = this.spDataProvider.GetLookupListItemById("EntityIndex", kpid);

            foreach (SPField field in listItem.Fields)
            {
                // do not get hidden, read-only, or base-type fields
                if (!field.Hidden && !field.ReadOnlyField && !field.FromBaseType)
                {
                    // create a new item
                    fields.Add(field.InternalName, field.TypeAsString);
                }
            }
            // add back in "Title" since it's in the base type
            // and we exclude the base type fields
            fields.Add("Title", EntityConstants.ItemTypes.TEXT);

            return fields;
        }

        /// <summary>
        /// Gets the field names and data type from the specified list
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetEntityFields(string teamUrl, string listName)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            SPList spList = this.spDataProvider.GetTeamList(teamUrl, listName);
            //SPListItem item = spList.Items[0];

            foreach (SPField field in spList.Fields)
            {
                // do not get hidden, read-only, or base-type fields
                if (!field.Hidden && !field.ReadOnlyField && !field.FromBaseType)
                {
                    // create a new item
                    fields.Add(field.InternalName, field.TypeAsString);
                }
            }
            // add back in "Title" since it's in the base type
            // and we exclude the base type fields
            fields.Add("Title", EntityConstants.ItemTypes.TEXT);

            return fields;
        }

        /// <summary>
        /// Helper method to create team hierarchy & paths
        /// this method always begins at the root teams
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<Team> GetTeamHierarchy(List<Team> teamsList)
        {
            int rootId = 0; // root team parent id's are -0- in kingpin
            List<Team> rootTeams = teamsList.FindAll(t => t.ParentId.Equals(rootId));
            List<Team> allTeams = new List<Team>();
            foreach (Team rootTeam in rootTeams)
            {
                allTeams.AddRange(this.GetChildTeams(rootTeam, teamsList));
            }
            allTeams.AddRange(rootTeams);
            return allTeams;
        }

        /// <summary>
        /// TODO: Move to utilities
        /// Creates a new site column
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="displayName"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public KPListItem CreateSiteColumn(string teamUrl, string displayName, SPFieldType fieldType)
        {
            string KPGROUP_NAME = "Kingpin Content Types";
            SPField spField = this.spDataProvider.CreateSiteColumn(teamUrl, displayName, fieldType, KPGROUP_NAME);
            return new KPListItem();
        }

        /// <summary>
        /// TODO: Move to utilities
        /// Gets the list of Site Content Types
        /// </summary>
        /// <returns></returns>
        public KPContentType GetKPContentTypes(string teamUrl, string cTypeName)
        {
            SPContentType ctype = this.spDataProvider.GetSiteContentTypes(teamUrl, cTypeName);
            return new KPContentType()
            {
                Id = ctype.Id.ToString(),
                Name = ctype.Name,
                GroupName = ctype.Group
            };
        }

        /// <summary>
        /// Get the user's name and alias by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetUserAliasById(string spUserValue, string teamUrl)
        {
            string SP_IDENTITY_SEPARATOR = ";#";
            // format is "lastName, firstName|alias"
            string identityFormat = "{0}|{1}";
            string[] stringSeparators = new string[] { SP_IDENTITY_SEPARATOR };
            string[] identityValues;
            int userId;

            if (spUserValue.IndexOf(SP_IDENTITY_SEPARATOR) > -1)
            {
                identityValues = spUserValue.Split(stringSeparators, StringSplitOptions.None);
                userId = Int32.Parse(identityValues[0]);
                string alias = this.spDataProvider.GetAliasFromSPUserId(userId, teamUrl);
                spUserValue = string.Format(identityFormat, identityValues[1], alias);
            }

            return spUserValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadCoreLookups()
        {
            EventLogger.WriteLine("Loading core lookups");
            List<Team> teams = null;
            
            //if (this.CheckListUpdateTimestamp("/Common", "KPTeams"))

            //if (SPDataAccess.TeamCache == null)
            //{
                teams = this.GetLookupObjects<Team>("KPTeams");
                this.Teams = this.GetTeamHierarchy(teams);
                this.Teams = this.Teams.OrderBy(x => x.KPID).ToList();
                SPDataAccess.TeamCache = teams;
            //}
            //else
            //{
            //    // sets the local property to the static cache
            //    // TODO: need to add a flush and expiration on cache
            //    this.Teams = SPDataAccess.TeamCache;
            //}
        }

        /// <summary>
        /// Clears all items from the cache
        /// </summary>
        public void FlushCache()
        {
            SPDataAccess.TeamCache.Clear();
        }
        #endregion

        #region Private member methods
        /// <summary>
        /// Gets the SPListItem by its KPID
        /// converts and returns the object as a KPListItem
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="KPID"></param>
        /// <returns></returns>
        private SPListItem GetKPListItemByKPID(string teamUrl, string listName, int KPID)
        {
            return GetKPTeamListItemCAML(teamUrl, listName, "KPID", EntityConstants.ItemTypes.TEXT, KPID.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="KPID"></param>
        /// <returns></returns>
        private SPListItem GetKPListItemByID(string teamUrl, string listName, int ID)
        {
            return GetKPTeamListItemCAML(teamUrl, listName, "ID", EntityConstants.ItemTypes.NUMBER, ID.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private SPListItem GetKPTeamListItemCAML(string teamUrl, string listName, string fieldName, string fieldType, string fieldValue)
        {
            string camlQuery = string.Format(ITEM_QUERY, fieldName, fieldType, fieldValue);
            SPListItem spListItem = this.spDataProvider.GetTeamListItem(teamUrl, listName, camlQuery);
            return spListItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private SPListItem GetKPCommonListItemCAML(string listName, string fieldName, string fieldType, string fieldValue)
        {
            string camlQuery = string.Format(ITEM_QUERY, fieldName, fieldType, fieldValue);
            return this.spDataProvider.GetLookupListItem(listName, camlQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SPListItem GetLibraryObjectByName(string teamUrl, string libraryName, string fieldName, string fieldType, string fieldValue)
        {
            string camlQuery = string.Format(ITEM_QUERY, fieldName, fieldType, fieldValue);
            return this.spDataProvider.GetTeamLibraryItem(teamUrl, libraryName, camlQuery);
        }

        /// <summary>
        /// Creates KPListItem objects that map to entity objects
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<KPListItem> CreateEntityObjects(string teamUrl, string listName)
        {
            return this.CreateEntityObjects(teamUrl, listName, null);
        }

        /// <summary>
        /// Method creates a new object of type KPListItem per item
        /// This method generates the object instance from the list item
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<KPListItem> CreateEntityObjects(string teamUrl, string listName, string count)
        {
            SPQuery spQuery = null;
            SPListItemCollection spListItems = null;
            List<KPListItem> items = new List<KPListItem>();
            SPList spList = this.spDataProvider.GetTeamList(teamUrl, listName);
            EventLogger.Timer.Start(string.Format("Create entities: {0}", listName));
            if(!string.IsNullOrEmpty(count))
            {
                spQuery = new SPQuery();
                spQuery.RowLimit = UInt32.Parse(count);
                spListItems = spList.GetItems(spQuery);
            }
            else{
                spListItems = spList.Items;
            }
            foreach (SPListItem item in spListItems)
            {
                items.Add(CreateKPListItem(item));
            }
            long elapsed = EventLogger.Timer.Stop(string.Format("Create entities: {0}", listName));
            long avg = (items.Count > 0) ? elapsed / items.Count : elapsed;
            EventLogger.WriteLine("Creating entites {0}({1}) average elapsed: {2}ms", listName, items.Count, avg);
            return items;
        }

        /// <summary>
        /// Method creates a new object of type KPListItem per item
        /// This method generates the object instance from the list item
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<KPListItem> CreateLibraryObjects(string teamUrl, string listName)
        {
            List<KPListItem> items = new List<KPListItem>();
            SPDocumentLibrary spLibrary = this.spDataProvider.GetTeamLibrary(teamUrl, listName);
            EventLogger.Timer.Start(string.Format("Create entities: {0}", listName));
            SPListItemCollection spListItems = spLibrary.Items;
            foreach (SPListItem item in spListItems)
            {
                items.Add(CreateKPListItem(item));
            }
            long elapsed = EventLogger.Timer.Stop(string.Format("Create entities: {0}", listName));
            long avg = (items.Count > 0) ? elapsed / items.Count : elapsed;
            EventLogger.WriteLine("Creating entites {0}({1}) average elapsed: {2}ms", listName, items.Count, avg);
            return items;
        }

        /// <summary>
        /// Creates a new KPListItem using SPListItem 
        /// to populate data values in the KPListItem
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="spItem"></param>
        /// <returns></returns>
        private KPListItem CreateKPListItem(SPListItem spItem)
        {
            KPItem item;
            KPListItem kpListItem = new KPListItem();
            SPFieldCollection spItemFields = spItem.Fields;
            // enumerate spfields
            foreach (SPField field in spItemFields)
            {
                // do not get hidden, read-only, or base-type fields
                if (!field.Hidden && !field.ReadOnlyField && !field.FromBaseType)
                {
                    try
                    {
                        // seems to be throwing on "SubUrl"?
                        if(!kpListItem.ContainsKey(field.Title))
                        {
                            // create a new item
                            item = new KPItem(spItem, field);
                            kpListItem.Add(field.Title, item);
                        }
                    }
                    catch(Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
            // add skipped base-type fields
            // Title, Modified, Created, ModifiedBy, CreatedBy, ID, GUID
            kpListItem.Add("ID", new KPItem(spItem, spItem.Fields["ID"]));
            kpListItem.Add("GUID", new KPItem(spItem, spItem.Fields["GUID"]));
            // special case with Documents they do not derive from base type
            if (!kpListItem.ContainsKey("Title")) { kpListItem.Add("Title", new KPItem(spItem, spItem.Fields["Title"])); }
            kpListItem.Add("Created", new KPItem(spItem, spItem.Fields["Created"]));
            // the SPUser is retrieved, but only the ID is returned - TODO: need to think about this
            kpListItem.Add("CreatedBy", new KPItem(spItem, spItem.Fields["Created By"]));
            kpListItem.Add("Modified", new KPItem(spItem, spItem.Fields["Modified"]));
            // the SPUser is retrieved, but only the ID is returned - TODO: need to think about this
            kpListItem.Add("ModifiedBy", new KPItem(spItem, spItem.Fields["Modified By"]));
            return kpListItem;
        }

        /// <summary>
        /// Strips HTML from all "Note" value fields
        /// </summary>
        /// <param name="kpListItem"></param>
        /// <returns></returns>
        private KPListItem CleanKPListItem(KPListItem kpListItem)
        {
            foreach(KeyValuePair<string, KPItem> kvp in kpListItem)
            {
                kpListItem[kvp.Key].Value = KPUtilities.StripHTML(kpListItem[kvp.Key].Value, true);
            }

            return kpListItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spItem"></param>
        /// <param name="spItemFields"></param>
        /// <returns></returns>
        private KPItemVersion CreateKPListItemVersion(SPListItem spItem, SPListItemVersion currentVersion, SPListItemVersion previousVersion)
        {
            KPItemVersion kpItemVersion = new KPItemVersion();
            string version = string.Empty;
            foreach (SPField field in currentVersion.Fields)
            {

                string currentVersionValue = string.Empty;
                string previousVersionValue = string.Empty;

                if (field.ShowInVersionHistory == false)
                {
                    continue;
                }

                // do not get hidden, read-only, or base-type fields
                if (field.Hidden && field.ReadOnlyField && field.FromBaseType)
                {
                    continue;
                }

                // skip KPGUIDs
                if (field.StaticName == "KPGUID" || field.StaticName == "KPID")
                {
                    continue;
                }

                if (previousVersion == null)
                {
                    continue;
                }

                if (currentVersion[field.StaticName] == null && previousVersion[field.StaticName] == null)
                {
                    continue;
                }

                if (currentVersion[field.StaticName] != null)
                {
                    currentVersionValue = KPUtilities.StripHTML(currentVersion[field.StaticName].ToString(), true);
                }

                if (previousVersion[field.StaticName] != null)
                {
                    previousVersionValue = KPUtilities.StripHTML(previousVersion[field.StaticName].ToString(), true);
                    version = currentVersion.VersionLabel;
                }

                if (currentVersionValue.Equals(previousVersionValue))
                {
                    continue;
                }

                kpItemVersion.Fields.Add(field.StaticName, currentVersionValue);
            }


            return kpItemVersion;
        }
        /// <summary>
        /// Takes the SPUser and converts to KPUser
        /// </summary>
        /// <param name="spUser"></param>
        /// <returns></returns>
        public void InitializeCurrentUser()
        {
            SPUser spUser = this.spDataProvider.GetCurrentUser();
            KPUser kpUser = new KPUser()
            {
                ID = spUser.ID,
                Alias = spUser.Email.Split('@')[0],
                FullName = spUser.Name ?? string.Empty,
                Email = spUser.Email,
                IsSiteAdmin = spUser.IsSiteAdmin,
                LoginName = spUser.LoginName
            };

            if (string.IsNullOrEmpty(kpUser.Alias)) // test accounts don't have emails. Alias is saved in fullname
            {
                kpUser.Alias = kpUser.FullName;
                kpUser.Email = kpUser.Alias + "@amazon.com";
                kpUser.FullName = kpUser.Alias + ", " + kpUser.Alias;
            }

            // fetch config list to get list of teams
            bool userInCache = false;

            if (!userInCache)
            {
                List<string> additionalTeams = new List<string>();
                SPListItem config = this.GetKPCommonListItemCAML("KPConfigList", "KPUserName", EntityConstants.ItemTypes.TEXT, kpUser.Alias);

                if (config == null) // user config does not exist, check all teams for permissions
                {
                    string teamIds = GetTeamPermissions(kpUser.Alias);
                    string[] teamIdsArray = teamIds.Split(';');
                    string primaryTeamValue = string.Empty;
                    string additionalTeamValue = teamIds;
                    if (!string.IsNullOrEmpty(teamIdsArray[0]))
                    {
                        primaryTeamValue = teamIdsArray[0];
                    }


                    if (string.IsNullOrEmpty(primaryTeamValue))
                    {
                        throw new Exception("User has no permissions for any team in Kingpin");
                    }

                    bool goalsTab = true;
                    bool businesssReviewTab = false;
                    if (primaryTeamValue.Equals("176"))
                    {
                        goalsTab = false;
                        businesssReviewTab = true;
                    }
                    ConfigList newUser = new ConfigList()
                    {
                        FullName = "AutoGen-2",
                        KPUserName = kpUser.Alias,
                        PrimaryTeam = int.Parse(primaryTeamValue),
                        AdditionalTeams = string.IsNullOrEmpty(additionalTeamValue) ? new List<int>() : additionalTeamValue.Split(';').Select(Int32.Parse).ToList(),
                        GoalsTab = goalsTab,
                        BusinessReviewTab = businesssReviewTab
                    };

                    LookupRepository<ConfigList> lookupRepo = new LookupRepository<ConfigList>(this, "KPConfigList");
                    lookupRepo.AddItem(newUser);
                    config = this.GetKPCommonListItemCAML("KPConfigList", "KPUserName", EntityConstants.ItemTypes.TEXT, kpUser.Alias);
                }

                string[] name;
                string[] alias;
                if (config != null)  // user found/has permissions
                {
                    if (config["AdditionalTeams"] != null)
                    {
                        string addlTeams = config["AdditionalTeams"].ToString();
                        additionalTeams = new List<string>(addlTeams.Split(GlobalConstants.MULTIVALUE_DELIMITER));
                    }

                    name = (!string.IsNullOrEmpty(spUser.Name)) ? spUser.Name.Split(',') : new string[] { string.Empty, string.Empty };
                    alias = (!string.IsNullOrEmpty(spUser.Email)) ? spUser.Email.Split('@') : new string[] { string.Empty, string.Empty };
                    
                    string primaryTeam = config["PrimaryTeam"].ToString();

                    kpUser.PrimaryTeam = this.Teams.Find(t => t.KPID == Int32.Parse(primaryTeam));
                    kpUser.Teams = this.Teams.FindAll(t => additionalTeams.Contains(t.KPID.ToString()));
                    kpUser.CacheTime = DateTime.UtcNow;
                }
                else // user does not exist and has no permissions
                {
                    name = (!string.IsNullOrEmpty(spUser.Name)) ? kpUser.FullName.Split(',') : new string[] { string.Empty, string.Empty };
                    alias = (!string.IsNullOrEmpty(spUser.Email)) ? kpUser.Email.Split('@') : new string[] { string.Empty, string.Empty };
                    kpUser.PrimaryTeam = null;
                    kpUser.Teams = new List<Team>();
                }

                if (name.Length > 1)
                {
                    kpUser.FirstName = name[1].Trim();
                    kpUser.LastName = name[0].Trim();
                }
                else
                {
                    kpUser.FirstName = kpUser.Alias;
                    kpUser.LastName = kpUser.Alias;
                }

                // Fix me: initial load will have 3 async calls and conflict here
                try
                {
                    userCache.Add(kpUser.Alias, kpUser);
                }
                catch (Exception)
                {
                    // ignore: do nothing if key already added
                }
            }

            this.CurrentUser = kpUser;
        }

        private string GetTeamPermissions(string userAlias)
        {
            string userFullAlias = "i:0#.w|ant\\" + userAlias;

            StringBuilder teamAccessIDs = new StringBuilder();
            foreach (Team team in this.Teams)
            {
                if (!string.IsNullOrEmpty(team.SiteUrl) && this.spDataProvider.CheckSubSiteExist(team.SiteUrl))
                {
                    SPWeb oWeb = this.spDataProvider.GetTeamWeb(team.SiteUrl);

                    bool readPermissions = false;
                    if (string.IsNullOrEmpty(userAlias))
                        readPermissions = oWeb.DoesUserHavePermissions(SPBasePermissions.ViewListItems);
                    else
                    {
                        try
                        {
                            readPermissions = oWeb.DoesUserHavePermissions(userFullAlias, SPBasePermissions.ViewListItems);
                        }
                        catch (Exception) { }
                    }

                    if (readPermissions)
                    {
                        if (teamAccessIDs.Length > 0)
                        {
                            teamAccessIDs.Append(";");
                        }
                        teamAccessIDs.Append(team.KPID);
                    }
                }
            }
            return teamAccessIDs.ToString();
        }

        /// <summary>
        /// Recursive method to build team hierarchy and create team URLs
        /// </summary>
        /// <param name="parentTeam"></param>
        private List<Team> GetChildTeams(Team parentTeam, List<Team> teamsList)
        {
            List<Team> childTeams = teamsList.FindAll(c => c.ParentId.Equals(parentTeam.KPID));
            List<Team> allTeams = new List<Team>();
            //parentTeam.Children.AddRange(childTeams);
            foreach (Team childTeam in childTeams)
            {
                childTeam.SetParentUrl(parentTeam.SiteUrl);
                allTeams.AddRange(GetChildTeams(childTeam, teamsList));
            }
            allTeams.AddRange(childTeams);
            return allTeams;
        }

        #endregion

    }
}
