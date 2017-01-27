using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Security;
using System.IO;
using System.ServiceModel;

namespace Amazon.Kingpin.WCF2.Data.Providers
{
    /// <summary>
    /// SPDataProvider manages connections to SPSite and SPWeb
    /// caches instances of these for re-use so we don't
    /// have to open and close connections each time we access the same URL
    /// When using this class, it should be used just as you would use an SPSite/SPWeb
    /// object. This class should not be used directly unless low-level SharePoint 
    /// access is needed. You will typically use the SPDataAccess class to perform
    /// all SharePoint related operations.
    /// <code>
    ///     using(spDataProvider) 
    ///     {
    ///         // ... code goes here
    ///     }
    /// </code>
    /// </summary>
    internal class SPDataProvider : IDisposable
    {
        #region Private member fields
        /// <summary>
        /// Internal dictionary tracks SPSite/SPWeb instances
        /// these are cleaned-up in the Dispose() method
        /// </summary>
        private Dictionary<string, SPSiteCollection> spTeamSiteInstances = new Dictionary<string, SPSiteCollection>();
        /// <summary>
        /// Current Site URL being accessed
        /// </summary>
        private string SITE_URL = string.Empty;

        private List<string> lookupListNames = new List<string>() { 
            "KPTeams",
            "KPGoalSets",
            "KPVPs",
            "KPCategoryL1",
            "KPCategoryL2",
            "KPCounter",
        };

        #endregion

        #region Ctor/Dtor
        ~SPDataProvider()
        {
            EventLogger.WriteLine("SPDataAccess destructor called");
            this.Dispose();
        }
        #endregion

        #region Public Member Properties
        /// <summary>
        /// Returns the host name and port if applicable (e.g. http://kingpin-dev-02:8080)
        /// </summary>
        internal string HostUrl
        {
            get { return this.SITE_URL; }
        }
        #endregion

        #region Internal member methods
        /// <summary>
        /// Checks the timestamp on the specified list. 
        /// Lists and Libraries are accessed using a different URL path.
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="entityName"></param>
        /// <param name="isList"></param>
        /// <returns></returns>
        internal DateTime CheckListUpdateTimestamp(string teamUrl, string entityName, bool isList)
        {
            // final target - list or library
            string target = string.Empty;
            // specifier for lists
            string lists = "Lists/";
            // format target list/library url
            string targetUrl = "{0}{1}/{2}{3}";
            DateTime lastModified;
            try
            {
                // uses lists value or is empty
                target = (isList) ? lists : target;
                SPWeb spWeb = this.GetTeamWeb(teamUrl);
                string listUrl = string.Format(targetUrl, this.SITE_URL, spWeb.ServerRelativeUrl, target, entityName);
                SPList spList = spWeb.GetList(listUrl);
                lastModified = spList.LastItemModifiedDate;
            }
            catch (Exception) 
            {
                lastModified = DateTime.Now;
            }

            return lastModified;
        }

        /// <summary>
        /// Gets the current user
        /// </summary>
        /// <returns></returns>
        internal SPUser GetCurrentUser()
        {
            SPWeb spWeb = this.GetCommonWeb();
            SPUser spUser = spWeb.CurrentUser;
            return spUser;
        }

        /// <summary>
        /// Get the Site Collection <root> web
        /// </summary>
        /// <returns>An SPWeb object</returns>
        internal SPWeb GetRootWeb()
        {
            return this.GetWeb(string.Empty);
        }

        /// <summary>
        /// Get the "Common" web where all lookup lists are stored
        /// </summary>
        /// <returns>An SPWeb object</returns>
        internal SPWeb GetCommonWeb()
        {
            string siteUrl = GetCurrentUrl() + "/Common";
            return this.GetWeb(siteUrl);
        }

        /// <summary>
        /// Get the specified team (subsite) web
        /// </summary>
        /// <param name="subTeamUrl">Path to the team site (e.g. /Finance/Finance)</param>
        /// <returns>An SPWeb object</returns>
        internal SPWeb GetTeamWeb(string subTeamUrl)
        {
            subTeamUrl = (subTeamUrl.IndexOf('/') != 0) ? "/" + subTeamUrl : subTeamUrl;
            string siteUrl = GetCurrentUrl() + subTeamUrl;
            return this.GetWeb(siteUrl);
        }

        /// <summary>
        /// Get an instance of the referenced list
        /// </summary>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        internal SPList GetTeamList(string teamUrl, string listName)
        {
            SPWeb spWeb = GetTeamWeb(teamUrl);
            string listUrl = this.SITE_URL + spWeb.ServerRelativeUrl + "/Lists/" + listName;
            return spWeb.GetList(listUrl);
        }

        /// <summary>
        /// Gets the instance of the refernced library
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        internal SPDocumentLibrary GetTeamLibrary(string teamUrl, string libraryName)
        {
            SPWeb spWeb = GetTeamWeb(teamUrl);
            string libraryUrl = string.Format("{0}{1}/{2}", this.SITE_URL, spWeb.ServerRelativeUrl, libraryName);
            return (SPDocumentLibrary)spWeb.GetList(libraryUrl);
        }

        /// <summary>
        /// Gets the item from the referenced library
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        internal SPListItem GetTeamLibraryItem(string teamUrl, string libraryName, string camlQuery)
        {
            SPListItem spLibraryItem = null;
            SPDocumentLibrary library = this.GetTeamLibrary(teamUrl, libraryName);
            SPQuery query = new SPQuery();
            query.Query = camlQuery;
            query.RowLimit = 1;
            SPListItemCollection items = library.GetItems(query);
            // only looking for a single item
            if (items != null)
                spLibraryItem = items[0];
            else
                throw new Exception(string.Format("No dpcument found. team:{0}; list:{1}; caml:{2}", teamUrl, libraryName, camlQuery));

            return spLibraryItem;
        }

        /// <summary>
        /// Gets the specified item using the CAML 
        /// query from the specified site/list.
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <param name="camlQuery"></param>
        /// <returns></returns>
        internal SPListItem GetTeamListItem(string teamUrl, string listName, string camlQuery)
        {
            SPListItem spListItem = null;
            SPList spList = this.GetTeamList(teamUrl, listName);
            // query for item by KPID
            SPQuery query = new SPQuery();
            query.Query = camlQuery;
            query.RowLimit = 1;
            SPListItemCollection items = spList.GetItems(query);
            // only looking for a single item
            if (items != null)
                spListItem = items[0];
            else
                throw new Exception(string.Format("No item found. team:{0}; list:{1}; caml:{2}", teamUrl, listName, camlQuery));

            return spListItem;
        }

        /// <summary>
        /// Returns a new team list item
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        internal SPListItem GetNewTeamListItem(string teamUrl, string listName)
        {
            SPList spList = this.GetTeamList(teamUrl, listName);
            SPListItem newListItem = spList.Items.Add();
            return newListItem;
        }

        /// <summary>
        /// Get the specified lookup list
        /// </summary>
        /// <param name="listName"></param>
        /// <returns></returns>
        internal SPList GetLookupList(string listName)
        {
            SPWeb spWeb = this.GetCommonWeb();
            string listUrl = this.SITE_URL + spWeb.ServerRelativeUrl + "/Lists/" + listName;
            return spWeb.GetList(listUrl);
        }

        /// <summary>
        /// Gets a new list item to be populated with values to create a new list item
        /// </summary>
        /// <param name="listName">name of the list</param>
        /// <param name="listItem"></param>
        /// <returns></returns>
        internal SPListItem GetNewLookupListItem(string listName)
        {
            SPList list = this.GetLookupList(listName);
            SPListItem newItem = list.Items.Add();
            return newItem;           
        }

        /// <summary>
        /// Lookup list items have KPID = ID
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="kpid"></param>
        /// <returns></returns>
        internal SPListItem GetLookupListItemById(string listName, int KPID)
        {
            SPList list = this.GetLookupList(listName);
            SPListItem item = list.GetItemById(KPID);
            return item;
        }

        /// <summary>
        /// Lookup list items have KPID = ID
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="kpid"></param>
        /// <returns></returns>
        internal SPListItem GetLookupListItem(string listName, string camlQuery)
        {
            SPList spList = this.GetLookupList(listName);
            SPListItem spListItem = null;
            // query for item (caml)
            SPQuery query = new SPQuery();
            query.Query = camlQuery;
            query.RowLimit = 1;
            SPListItemCollection items = spList.GetItems(query);
            // only looking for a single item
            if (items.Count > 0)
                spListItem = items[0];

            return spListItem;
        }

        /// <summary>
        /// Get the SPUser by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal KPUser GetUserById(int id)
        {
            SPWeb web = this.GetCommonWeb();
            SPUser user = web.AllUsers.GetByID(id);
            KPUser kpUser = new KPUser();
            kpUser.Email = user.Email;
            kpUser.Alias = user.LoginName;

            return kpUser;
        }

        /// <summary>
        /// Saves the provided byte[] as a file to the specified document library
        /// </summary>
        /// <param name="teamUrl">Url to target team site</param>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="fileStream">file contents in bytes</param>
        /// <param name="replaceExistingFile">flag to overwrite file</param>
        internal void SaveFileToLibrary(string teamUrl, string fileName, byte[] fileStream, bool replaceExistingFile)
        {
            string libraryName = "KPDocuments";

            SPWeb spWeb = GetTeamWeb(teamUrl);
            SPFolder library = spWeb.Folders[libraryName];

            // Upload document
            SPFile spfile = library.Files.Add(fileName, fileStream, replaceExistingFile);

            // Commit 
            library.Update();

        }

        /// <summary>
        /// Gets the list of Site Content Types
        /// </summary>
        /// <returns></returns>
        internal SPContentType GetSiteContentTypes(string teamUrl, string cTypeName)
        {
            SPWeb web = this.GetTeamWeb(teamUrl);
            SPContentTypeCollection allCTypes = web.ContentTypes;
            List<SPContentType> kpCTypes = new List<SPContentType>();
            //List<KPContentType> ctypes = new List<KPContentType>();
            foreach (SPContentType cType in allCTypes)
            {
                kpCTypes.Add(cType);
                if(cType.Name == cTypeName)
                {
                    return cType;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a new site column in the specified web (teamUrl
        /// </summary>
        /// <param name="teamUrl">The SiteCollection</param>
        /// <param name="displayName">Display name of the column</param>
        /// <param name="fieldType">Field type of the column</param>
        /// <param name="groupDescriptor">Group name for the column</param>
        /// <returns></returns>
        internal SPField CreateSiteColumn(string teamUrl, string displayName, SPFieldType fieldType, string groupDescriptor)
        {
            using (SPWeb spWeb = GetTeamWeb(teamUrl))
            {
                SPField field = this.GetSiteColumn(teamUrl, displayName);
                if (field != null)
                {
                    string fieldName = spWeb.Fields.Add(displayName, fieldType, false);
                    field = spWeb.Fields.GetFieldByInternalName(fieldName);
                    field.Group = groupDescriptor;
                    field.Update();
                }
                return field;
            }
        }

        /// <summary>
        /// Get a SiteColumn by display name
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        internal SPField GetSiteColumn(string teamUrl, string displayName)
        {
            SPField field = null;
            using (SPWeb spWeb = GetTeamWeb(teamUrl))
            {
                if (spWeb.Fields.ContainsField(displayName))
                {
                    field = spWeb.Fields[displayName];
                }
            }
            return field;
        }

        /// <summary>
        /// Creates a new Site Content Type
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="parentItemCTypeId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        internal SPContentType CreateSiteContentType(string teamUrl, string contentTypeName, SPContentTypeId parentItemCTypeId, string group)
        {
            using (SPWeb spWeb = GetTeamWeb(teamUrl))
            {
                if (spWeb.AvailableContentTypes[contentTypeName] == null)
                {
                    SPContentType itemCType = spWeb.AvailableContentTypes[parentItemCTypeId];
                    SPContentType contentType = new SPContentType(itemCType, spWeb.ContentTypes, contentTypeName) { Group = @group };
                    spWeb.ContentTypes.Add(contentType);
                    contentType.Update();
                    return contentType;
                }
                return spWeb.ContentTypes[contentTypeName];
            }
        }

        /// <summary>
        /// Add the specified field to the provided ContentType (Id)
        /// </summary>
        /// <param name="web"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="field"></param>
        public void AddFieldToContentType(string teamUrl, SPContentTypeId contentTypeId, SPField field)
        {
            using (SPWeb spWeb = GetTeamWeb(teamUrl))
            {
                SPContentType contentType = spWeb.ContentTypes[contentTypeId];
                if (contentType == null) return;
                if (contentType.Fields.ContainsField(field.Title)) return;
                SPFieldLink fieldLink = new SPFieldLink(field);
                contentType.FieldLinks.Add(fieldLink);
                contentType.Update();
            }
        }

        /// <summary>
        /// Gets the URL of the current SPSite being accessed
        /// </summary>
        /// <returns></returns>
        internal string GetCurrentUrl()
        {
            // SPContext.Current not available from
            //  WCF - need to manually get the url
            Uri uri = null;
            string urlFormat = "http{0}://{1}{2}";
            string siteUrl = string.Empty;
            string host = string.Empty;
            string ssl = "s";
            uri = OperationContext.Current.RequestContext.RequestMessage.Headers.To;
            if (uri.Host.Contains("ant"))
                host = uri.Host.Split('.')[0];
            else
                host = uri.Host;

            if (uri.Port == 80)
            {
                siteUrl = string.Format(urlFormat, string.Empty, host, string.Empty);
            }
            else if (uri.Port == 443)
            {
                siteUrl = string.Format(urlFormat, ssl, host, string.Empty);
            }
            else
            {
                siteUrl = string.Format(urlFormat, string.Empty, host, ":" + uri.Port);
            }

            return siteUrl;
        }

        /// <summary>
        /// Dispose method implementation for IDispose.
        /// <para>This method calls DisposeAll to release all reources opened during a session</para>
        /// </summary>
        public void Dispose() 
        {
            this.DisposeAll();
        }

        /// <summary>
        /// Dispose of a specific SPWeb/SPSite by URL
        /// </summary>
        /// <param name="siteUrl"></param>
        internal void Dispose(string siteUrl)
        {
            if (spTeamSiteInstances.ContainsKey(siteUrl))
            {
                SPSiteCollection teamSite = spTeamSiteInstances[siteUrl];
                if (teamSite.spSite != null)
                {
                    //teamSite.spSite.Dispose();
                    try { teamSite.spSite.Dispose(); }
                    catch { /* eat any handle exceptions */}
                }                   
                if (teamSite.spWeb != null)
                {
                    //teamSite.spWeb.AllowUnsafeUpdates = false;
                    teamSite.spWeb.Dispose();
                }
            }
        }

        #endregion

        #region Private Member methods
        /// <summary>
        /// Get a reference to an instance of SPSite/SPWeb given an URL
        /// TODO: this should *really* just open a new SPSite
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        private SPSite GetSite(string siteUrl)
        {
            return NewInstance(siteUrl).spSite;
        }

        /// <summary>
        /// Get a reference to an instance of SPWeb
        /// </summary>
        /// <param name="webUrl"></param>
        /// <returns></returns>
        private SPWeb GetWeb(string siteUrl)
        {
            EventLogger.WriteLine("Get Web: {0}", siteUrl);
            return NewInstance(siteUrl).spWeb;
        }

        /// <summary>
        /// Gets a new instance of the 
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        private SPSiteCollection NewInstance(string siteUrl)
        {
            SPSiteCollection siteCollection = new SPSiteCollection();
            if (spTeamSiteInstances.ContainsKey(siteUrl))
            {
                siteCollection.spSite = spTeamSiteInstances[siteUrl].spSite;
                siteCollection.spWeb = spTeamSiteInstances[siteUrl].spWeb;
                EventLogger.WriteLine("Get NewInstance from cache: {0}", siteUrl);
            }
            else
            {
                EventLogger.Timer.Start("SPDataProvider.NewInstance");
                siteCollection.spSite = new SPSite(siteUrl);
                siteCollection.spWeb = siteCollection.spSite.OpenWeb();
                // TODO: use ValidateFormDigest - more secure than AllowUnsafeUpdates
                // teamSite.spWeb.ValidateFormDigest();
                siteCollection.spWeb.AllowUnsafeUpdates = true;
                // cache instance for later use so we aren't fetching up everytime
                spTeamSiteInstances.Add(siteUrl, siteCollection);
                long elapsed = EventLogger.Timer.Stop("SPDataProvider.NewInstance");
                EventLogger.WriteLine("Create NewInstance elapsed: {0}ms", elapsed);
            }

            return siteCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        internal SPList GetListInstance(string siteUrl, string listName)
        {
            return this.GetTeamWeb(siteUrl).Lists.TryGetList(listName);
        }

        /// <summary>
        /// Get the user alias from the supplied id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal string GetAliasFromSPUserId(int userId, string teamUrl)
        {
            string alias = userId.ToString() + " (not found)";
            SPWeb spWeb = null;
            if (string.IsNullOrEmpty(teamUrl))
                spWeb = this.GetCommonWeb();
            else
                spWeb = this.GetTeamWeb(teamUrl);
            try
            {
                SPUser user = spWeb.AllUsers.GetByID(userId);
                if (!string.IsNullOrEmpty(user.Email))
                {
                    alias = user.Email.Split('@')[0];
                }
            }
            catch (Exception)
            {
                // eat this user not found exception
            }
            return alias;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAlias"></param>
        /// <returns></returns>
        internal string GetSPUserFullName(string userAlias)
        {
            SPWeb spWeb = this.GetRootWeb();
            SPUser spUser = null;
            string fullName = string.Empty;
            try
            {
                spUser = spWeb.EnsureUser(userAlias);
            }
            catch (Exception) // Hack: exception if user left Amazon
            {
                try // one last try with email find
                {
                    spUser = spWeb.AllUsers.GetByEmail(userAlias);
                }
                catch (Exception) // user can't be found, create them
                {
                    //spWeb.AllUsers.Add(versionCreator.LoginName, versionCreator.Email, versionCreator.Name, "");
                    //userAuthor = destWeb.AllUsers.GetByEmail(versionCreator.Email);
                }
            }

            if (spUser == null || string.IsNullOrEmpty(spUser.Name))
                fullName = userAlias;
            else
                fullName = spUser.Name;

            return fullName;
        }

        /// <summary>
        /// Disposes of all open instances of SPSite/SPWeb
        /// </summary>
        private void DisposeAll()
        {
            EventLogger.WriteLine("Dispose all called");
            if (spTeamSiteInstances.Count > 0)
            {
                foreach (string key in spTeamSiteInstances.Keys)
                {
                    this.Dispose(key);
                }
                // clear all cached instances of SPSiteCollection
                spTeamSiteInstances.Clear();
            }
        }

        #endregion

        internal bool CheckSubSiteExist(string subSiteUrl)
        {
            string url = GetCurrentUrl() + subSiteUrl;
            //remove the backslash from the end of the url
            if (url.EndsWith("/"))
            {
                url = url.Remove(url.Length - 1);
            }

            bool result = false;
            SPSecurity.RunWithElevatedPrivileges(delegate() // required, to be able to use .AllWebs you require permissions, some users don't have permissions before propagation
            {
                using (SPSite site = new SPSite(url))
                {
                    //get the site collection url
                    string sitecol = site.AllWebs[0].Url;
                    //check if the subsite is the topsite
                    if (!sitecol.Equals(url))
                    {
                        try
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                if (!sitecol.Equals(web.Url))
                                {
                                    if (!web.Exists)
                                    {
                                        result = false;
                                    }
                                    else
                                    {
                                        result = true;
                                    }
                                }
                                else
                                {

                                    result = false;
                                }
                            }
                        }
                        catch
                        {
                            result = false;
                            //break;
                        }
                    }
                    else
                    { result = true; }
                }
            });
            return result;
        }

        #region Private Classes

        /// <summary>
        /// SiteCollection class holds the SPSite/SPWeb object references
        /// </summary>
        private class SPSiteCollection
        {
            internal SPSite spSite { get; set; }
            internal SPWeb spWeb { get; set; }
        }

        #endregion
    }
}
