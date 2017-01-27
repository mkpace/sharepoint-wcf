using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Repositories.Lookup;
using Amazon.Kingpin.WCF2.Security;
using Microsoft.SharePoint;
using Amazon.Kingpin.WCF2.Repositories.Managers;
using Amazon.Kingpin.WCF2.Diagnostics;

namespace Amazon.Kingpin.WCF2.Repositories
{
    /// <summary>
    /// Facade for repositories and SPDataAccess. 
    /// This class loads Lookup lists on initialization
    /// and creates domain objects that are requested.
    /// This class also contains methods that return
    /// Message responses for the various APIs
    /// </summary>
    public class DomainManager
    {
        private SPDataAccess spDataAccess;
        private bool isInitialized = false;
        private EntityManager entityManager;
        private LookupManager lookupManager;

        /// <summary>
        /// Get the current user context (SPUser) as KPUser object instance
        /// </summary>
        public KPUser User { get; set; }

        /***********************
        * Entity Repositories
        ***********************/
        public GoalRepository GoalRepository { get; set; }

        /***********************
        * Lookup Repositories 
        ************************/
        public TeamRepository TeamRepository { get; set; }
        public ConfigListRepository ConfigListRepository { get; set; }
        public GoalSetRepository GoalSetRepository { get; set; }
        public CategoryL1Repository CategoryL1Repository { get; set; }
        public CategoryL2Repository CategoryL2Repository { get; set; }
        public VPRepository VPRepository { get; set; }

        /// <summary>
        /// Default Ctor to create repository instances
        /// </summary>
        public DomainManager()
        {
            // create DataAccess instance
            this.spDataAccess = new SPDataAccess();            
            // create lookup repository instances
            this.ConfigListRepository = new ConfigListRepository(this.spDataAccess);
            this.TeamRepository = new TeamRepository(this.spDataAccess);

            // create entity repository instances
            this.GoalRepository = new GoalRepository(this.spDataAccess);
            this.SetCurrentUser();

            this.entityManager = new EntityManager(spDataAccess, User);
            this.lookupManager = new LookupManager(spDataAccess);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public void EnableCache(string entityName, bool enable)
        {
            this.entityManager.EnableCache(entityName, enable);
        }

        /// <summary>
        /// Wrapper method calls EntityManager method of same name. 
        /// Gets all user entities by entity name
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message GetUserEntities(WebOperationContext ctx, string entityName)
        {
            return this.entityManager.GetUserEntities(ctx, entityName);
        }

        /// <summary>
        /// Wrapper method calls EntityManager method of same name. 
        /// Gets all user entities by entity name and aggregates team(s)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public Message GetUserEntitiesByTeams(WebOperationContext ctx, string entityName, string teamIds)
        {
            return this.entityManager.GetUserEntities(ctx, entityName, teamIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityNames"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public Message GetAllEntitiesByNameByTeam(WebOperationContext ctx, string entityNames, string teamIds)
        {
            return this.entityManager.GetAllEntitiesByNameByTeam(ctx, entityNames, teamIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="entityKPID"></param>
        /// <returns></returns>
        public Message GetEntityById(WebOperationContext ctx, string entityName, string entityKPID)
        {
            return this.entityManager.GetEntityById(ctx, entityName, entityKPID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="entityKPID"></param>
        /// <returns></returns>
        public Message GetEntityVersionsById(WebOperationContext ctx, string entityName, string entityKPID)
        {
            return this.entityManager.GetEntityVersionsById(ctx, entityName, entityKPID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="teamId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Message GetFileByName(WebOperationContext ctx, string teamId, string fileName)
        {
            return this.entityManager.GetFileByName(ctx, teamId, fileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="dataMsg"></param>
        /// <returns></returns>
        public Message SaveEntity(WebOperationContext ctx, string entityName, Message dataMsg)
        {
            return this.entityManager.SaveEntity(ctx, entityName, dataMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message GetLookup(WebOperationContext ctx, string entityName)
        {
            return this.GetLookup(ctx, entityName, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message GetLookup(WebOperationContext ctx, string entityName, string count)
        {
            return this.lookupManager.GetLookup(ctx, entityName, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="dataMsg"></param>
        /// <returns></returns>
        public Message SaveLookup(WebOperationContext ctx, string entityName, Message dataMsg)
        {
            return this.lookupManager.SaveLookup(ctx, entityName, dataMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userAlias"></param>
        /// <returns></returns>
        public Message GetConfigListItemByAlias(WebOperationContext ctx, string userAlias)
        {
            return this.lookupManager.GetConfigListItemByAlias(ctx, userAlias);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public Message GetAllChildren(WebOperationContext ctx, string parentID)
        {
            return this.lookupManager.GetAllChildren(ctx, parentID);
        }

        /// <summary>
        /// Performs default initialization of repository data and loads default objects
        /// </summary>
        public void InitLookups()
        {
            if (this.VPRepository == null)
                this.VPRepository = new VPRepository(this.spDataAccess);

            this.GenerateGoalSetHierarchy();
        }

        /// <summary>
        /// Used by SheetImport to fetch user items for comparison - Add/Update items
        /// This method gets items from the specified Team (Org) and all sub-teams the user has access to
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<KPListItem> GetUserEntities(string entityName, int teamId)
        {
            // initializes lookup lists and creates KPUser context
            if(this.User == null)
            {
                this.SetCurrentUser();
            }

            List<KPListItem> items = new List<KPListItem>();
            List<int> teamIds = TeamRepository.GetChildTeamIds(this.User.Teams, teamId);
            teamIds.Add(teamId);
            // get the specified entities for the current user
            foreach (int tid in teamIds)
            {
                items.AddRange(this.spDataAccess.GetKPListItems(tid, entityName));
            }

            return items;
        }

        /// <summary>
        /// Gets all entities for the specified teams
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public List<KPListItem> GetUserEntities(string entityName, List<int> teamIds)
        {
            List<KPListItem> items = new List<KPListItem>();
            foreach(int teamId in teamIds)
            {
                items.AddRange(this.GetUserEntities(entityName, teamId));
            }
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUrl"></param>
        /// <param name="displayName"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public KPListItem CreateSiteColumn(string teamUrl, string displayName, SPFieldType fieldType)
        {
            return this.spDataAccess.CreateSiteColumn(teamUrl, displayName, SPFieldType.Text);
        }

        /// <summary>
        /// Gets the list of Site Content Types
        /// </summary>
        /// <returns></returns>
        public KPContentType GetKPContentTypes(string teamUrl, string cTypeName)
        {
            KPContentType ctypes = this.spDataAccess.GetKPContentTypes(teamUrl, cTypeName);
            return ctypes;
        }

        /// <summary>
        /// Gets the lookup value for an item field
        /// TODO: Create an IRespository
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public string GetLookupValue(string fieldName, string fieldValue, string lookupField)
        {
            string value = string.Empty;
            switch (fieldName)
            {
                case "KPTeam":
                    Team team = this.spDataAccess.Teams.Find(i => i.Nick == fieldValue || i.Title == fieldValue);
                    value = (team != null) ? team.KPID.ToString() : string.Empty;
                    break;
                case "GoalSet":
                    GoalSet goalSet = this.GoalSetRepository.Items.Find(i => i.Title == fieldValue);
                    value = (goalSet != null) ? goalSet.KPID.ToString() : string.Empty;
                    break;
                case "KPPrimaryVP":
                    VP vp = this.VPRepository.Items.Find(i => i.Nick == fieldValue);
                    value = (vp != null) ? vp.KPID.ToString() : string.Empty;
                    break;
                case "KPSecondaryVPs":
                    // handle multiple
                    string[] values;
                    List<string> newValues = new List<string>();
                    if (fieldValue.Contains(","))
                    {
                        values = fieldValue.Split(',');
                        foreach (string val in values)
                        {
                            VP svp = this.VPRepository.Items.Find(i => i.Nick == val);
                            value = (svp != null) ? svp.KPID.ToString() : string.Empty;
                            // leave loop since we have a value mismatch
                            if (string.IsNullOrEmpty(value)) { break; }
                            // otherwise we'll continue adding values
                            newValues.Add(value);
                            value = string.Join(",", newValues.ToArray());
                        }
                    }
                    else
                    {
                        VP svp = this.VPRepository.Items.Find(i => i.Nick == fieldValue);
                        value = (svp != null) ? svp.KPID.ToString() : string.Empty;
                    }
                    break;
                case "CategoryL1":
                    CategoryL1 catL1 = this.CategoryL1Repository.Items.Find(i => i.Title == fieldValue);
                    value = (catL1 != null) ? catL1.KPID.ToString() : string.Empty;
                    break;
                case "CategoryL2":
                    CategoryL2 catL2 = this.CategoryL2Repository.Items.Find(i => i.Title == fieldValue);
                    value = (catL2 != null) ? catL2.KPID.ToString() : string.Empty;
                    break;
            }

            return value;
        }

        /// <summary>
        /// Initializes the current user object
        /// generates the KPUser object for the current user
        /// and initializes the KPUser config list and access permissions
        /// </summary>
        public void SetCurrentUser()
        {
            if(!this.isInitialized)
            {
                this.spDataAccess.InitializeCurrentUser();
                this.User = this.spDataAccess.CurrentUser;
                this.isInitialized = true;
            }
        }

        #region Private Member Methods
        /// <summary>
        /// Helper method to generate the GoalSet hierarchy - should be moved to Repository
        /// </summary>
        private void GenerateGoalSetHierarchy()
        {
            if (this.GoalSetRepository == null)
                this.GoalSetRepository = new GoalSetRepository(this.spDataAccess);

            if (this.CategoryL1Repository == null)
                this.CategoryL1Repository = new CategoryL1Repository(this.spDataAccess);

            if (this.CategoryL2Repository == null)
                this.CategoryL2Repository = new CategoryL2Repository(this.spDataAccess);

            List<GoalSet> goalSets = this.GoalSetRepository.Items;
            List<CategoryL1> categoryL1s = this.CategoryL1Repository.Items;
            List<CategoryL2> categoryL2s = this.CategoryL2Repository.Items;
            // add all assigned child category L2's to each category L1
            categoryL1s.ForEach(cl1s => cl1s.CategoryL2s = categoryL2s.FindAll(cl2s => cl2s.CategoryL1Id == cl1s.ID));
            // add all assigned child category L1's to each goal set
            goalSets.ForEach(gs => gs.CategoryL1s = categoryL1s.FindAll(cl1 => cl1.GoalSetId == gs.ID));

        }
        #endregion
    }
}
