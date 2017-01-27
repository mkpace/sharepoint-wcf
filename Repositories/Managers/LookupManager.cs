using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Diagnostics;
using Amazon.Kingpin.WCF2.Http;
using Amazon.Kingpin.WCF2.Utilities;
using Newtonsoft.Json;

namespace Amazon.Kingpin.WCF2.Repositories.Managers
{
    public class LookupManager
    {
        private SPDataAccess spDataAccess;
        private KPTimer timer;

        public LookupManager(SPDataAccess spDataAccess)
        {
            this.spDataAccess = spDataAccess;
            this.timer = spDataAccess.Timer;
        }

        #region Public Lookup Methods
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
            Message responseMsg = null;
            try
            {
                // get the specified entities for the current user
                switch (entityName)
                {
                    case "KPTeams":
                        List<Team> teams = this.GetLookup<Team>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Team>(ctx, teams, this.timer);
                        break;
                    case "KPConfigList":
                        List<ConfigList> configItems = this.GetLookup<ConfigList>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<ConfigList>(ctx, configItems, this.timer);
                        break;
                    case "KPCustomers":
                        List<Customer> customers = this.GetLookup<Customer>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Customer>(ctx, customers, this.timer);
                        break;
                    case "KPGoalSets":
                        List<GoalSet> goalsets = this.GetLookup<GoalSet>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<GoalSet>(ctx, goalsets, this.timer);
                        break;
                    case "KPCategoryL1":
                        List<CategoryL1> categoryL1 = this.GetLookup<CategoryL1>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<CategoryL1>(ctx, categoryL1, this.timer);
                        break;
                    case "KPCategoryL2":
                        List<CategoryL2> categoryL2 = this.GetLookup<CategoryL2>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<CategoryL2>(ctx, categoryL2, this.timer);
                        break;
                    case "KPVPs":
                        List<VP> vps = this.GetLookup<VP>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<VP>(ctx, vps, this.timer);
                        break;
                    case "Themes":
                        List<Theme> themes = this.GetLookup<Theme>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Theme>(ctx, themes, this.timer);
                        break;

                    case "KPAdmins":
                        List<Admin> admins = this.GetLookup<Admin>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Admin>(ctx, admins, this.timer);
                        break;
                    case "KPGoalSetLocks":
                        List<GoalSetLock> locks = this.GetLookup<GoalSetLock>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<GoalSetLock>(ctx, locks, this.timer);
                        break;
                    case "KPPerspectiveInstances":
                        List<PerspectiveInstance> perspectives = this.GetLookup<PerspectiveInstance>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<PerspectiveInstance>(ctx, perspectives, this.timer);
                        break;
                    case "OneOffConfigurations":
                        List<OneOffConfiguration> oneOffConfigurations = this.GetLookup<OneOffConfiguration>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<OneOffConfiguration>(ctx, oneOffConfigurations, this.timer);
                        break;
                    case "KingpinLockdown":
                        List<KingpinLockdown> lockdown = this.GetLookup<KingpinLockdown>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<KingpinLockdown>(ctx, lockdown, this.timer);
                        break;
                    case "Announcements":
                        List<Announcement> announcements = this.GetLookup<Announcement>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Announcement>(ctx, announcements, this.timer);
                        break;

                    case "EventLogging":
                        List<EventLogger> eventLogs = this.GetLookup<EventLogger>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<EventLogger>(ctx, eventLogs, this.timer);
                        break;
                    case "UserEventLogging":
                        List<UserLogging> userLogs = this.GetLookup<UserLogging>(entityName, count);
                        userLogs.Reverse();
                        responseMsg = HttpUtilities.GenerateResponse<UserLogging>(ctx, userLogs, this.timer);
                        break;
                    case "Views":
                        List<Amazon.Kingpin.WCF2.Classes.Entities.View> views = this.GetLookup<Amazon.Kingpin.WCF2.Classes.Entities.View>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Amazon.Kingpin.WCF2.Classes.Entities.View>(ctx, views, this.timer);
                        break;
                    case "Checkpoints":
                        List<Amazon.Kingpin.WCF2.Classes.Entities.Checkpoint> checkpoints = this.GetLookup<Amazon.Kingpin.WCF2.Classes.Entities.Checkpoint>(entityName, count);
                        responseMsg = HttpUtilities.GenerateResponse<Amazon.Kingpin.WCF2.Classes.Entities.Checkpoint>(ctx, checkpoints, this.timer);
                        break;
                    default:
                        string errMsg = string.Format("Error: No lookup list found with the name: {0}", entityName);
                        responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, new Exception(errMsg), "GET", HttpStatusCode.BadRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, ex, "GET", HttpStatusCode.InternalServerError);
            }

            this.timer.Stop();
            return responseMsg;
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
            Message responseMsg = null;
            string jsonData = JSON.GetPayload(dataMsg);
            try
            {
                switch (entityName)
                {
                    case "EntityLinks":
                        EntityLinks entityLinks = this.SaveLookup<EntityLinks>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<EntityLinks>(ctx, entityLinks, this.timer);
                        break;
                    case "EventLogging":
                        EventLogger eventLogger = this.SaveLookup<EventLogger>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<EventLogger>(ctx, eventLogger, this.timer);
                        break;
                    case "KPAdmins":
                        Admin kpAdmin = this.SaveLookup<Admin>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Admin>(ctx, kpAdmin, this.timer);
                        break;
                    case "KPCategoryL1":
                        CategoryL1 kpCatL1 = this.SaveLookup<CategoryL1>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<CategoryL1>(ctx, kpCatL1, this.timer);
                        break;
                    case "KPCategoryL2":
                        CategoryL2 kpCatL2 = this.SaveLookup<CategoryL2>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<CategoryL2>(ctx, kpCatL2, this.timer);
                        break;
                    case "KPConfigList":
                        ConfigList kpConfigList = this.SaveLookup<ConfigList>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<ConfigList>(ctx, kpConfigList, this.timer);
                        break;
                    //case "KPCountry":
                    //    Country kpCountry = this.SaveLookup<Country>(entityName, jsonData);
                    //    responseMsg = ctx.CreateJsonResponse<Country>(kpCountry);
                    //    break;
                    case "KPCustomers":
                        Customer kpCustomers = this.SaveLookup<Customer>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Customer>(ctx, kpCustomers, this.timer);
                        break;
                    //case "KPEffortInstances":
                    //    EffortInstance kpEffortInstances = this.SaveLookup<EffortInstance>(entityName, jsonData);
                    //    responseMsg = ctx.CreateJsonResponse<EffortInstance>(kpEffortInstances);
                    //    break;
                    //case "KPEfforts":
                    //    Effort kpEfforts = this.SaveLookup<Effort>(entityName, jsonData);
                    //    responseMsg = ctx.CreateJsonResponse<Effort>(kpEfforts);
                    //    break;
                    case "KPGoalSetLocks":
                        GoalSetLock kpGoalSetLock = this.SaveLookup<GoalSetLock>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<GoalSetLock>(ctx, kpGoalSetLock, this.timer);
                        break;
                    case "KPGoalSets":
                        GoalSet kpGoalSet = this.SaveLookup<GoalSet>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<GoalSet>(ctx, kpGoalSet, this.timer);
                        break;
                    //case "KPPerspectives":
                    //    Perspective kpPerspective = this.SaveLookup<Perspective>(entityName, jsonData);
                    //    responseMsg = ctx.CreateJsonResponse<Perspective>(kpPerspective);
                    //    break;
                    case "KPPerspectiveInstances":
                        PerspectiveInstance kpPerspectiveInstance = this.SaveLookup<PerspectiveInstance>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<PerspectiveInstance>(ctx, kpPerspectiveInstance, this.timer);
                        break;
                    case "KingpinLockdown":
                        KingpinLockdown lockdown = this.SaveLookup<KingpinLockdown>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<KingpinLockdown>(ctx, lockdown, this.timer);
                        break;
                    case "Announcements":
                        Announcement announcement = this.SaveLookup<Announcement>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Announcement>(ctx, announcement, this.timer);
                        break;

                    case "KPTeams":
                        Team kpTeam = this.SaveLookup<Team>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Team>(ctx, kpTeam, this.timer);
                        break;
                    case "KPVPs":
                        VP kpVP = this.SaveLookup<VP>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<VP>(ctx, kpVP, this.timer);
                        break;
                    case "OneOffConfigurations":
                        OneOffConfiguration oneOffConfiguration = this.SaveLookup<OneOffConfiguration>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<OneOffConfiguration>(ctx, oneOffConfiguration, this.timer);
                        break;
                    case "UserEventLogging":
                        UserLogging userEvent = this.SaveLookup<UserLogging>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<UserLogging>(ctx, userEvent, this.timer);
                        break;
                    default:
                        string errMsg = string.Format("Error: No lookup list found with the name: {0}", entityName);
                        responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, new Exception(errMsg), "POST/PUT", HttpStatusCode.BadRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, ex, "POST/PUT", HttpStatusCode.InternalServerError);
            }

            this.timer.Stop();
            return responseMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userAlias"></param>
        /// <returns></returns>
        public Message GetConfigListItemByAlias(WebOperationContext ctx, string userAlias)
        {
            Message responseMsg;
            try
            {
                ConfigList configListItem = this.spDataAccess.GetLookupObjectByField<ConfigList>("KPConfigList", "KPUserName", EntityConstants.ItemTypes.TEXT, userAlias);
                responseMsg = HttpUtilities.GenerateResponse<ConfigList>(ctx, configListItem, this.timer);
            }
            catch (Exception ex)
            {
                responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, ex, "GET", HttpStatusCode.InternalServerError);
            }
            return responseMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public Message GetAllChildren(WebOperationContext ctx, string parentID)
        {
            Message responseMsg = null;
            try
            {
                List<EntityLinks> entityLinks = this.GetLookup<EntityLinks>("EntityLinks", null);
                entityLinks.RemoveAll(x => x.ParentID != int.Parse(parentID));
                responseMsg = HttpUtilities.GenerateResponse<EntityLinks>(ctx, entityLinks, this.timer);
            }
            catch (Exception ex)
            {
                responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, ex, "GET", HttpStatusCode.InternalServerError);
            }

            return responseMsg;
        }
        #endregion

        #region Private Member Methods
        /// <summary>
        /// Add or update the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        private T SaveLookup<T>(string listName, string jsonData) where T : IKPItem, new()
        {
            T entity = JsonConvert.DeserializeObject<T>(jsonData);
            T item = default(T);
            LookupRepository<T> repository = new LookupRepository<T>(this.spDataAccess, listName);
            if (entity.KPID == -1 && entity.ID == -1)
                item = repository.AddItem(entity);
            else
                item = repository.UpdateItem(entity);

            return item;
        }

        /// <summary>
        /// Generic method to support getting any Lookup entity type.
        /// This method uses the BaseLookupRepository directly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<T> GetLookup<T>(string listName, string count) where T : IKPItem, new()
        {
            LookupRepository<T> repository = new LookupRepository<T>(this.spDataAccess, listName);
            List<T> items = repository.GetAllItems(count);
            return items;
        }
        #endregion
    }
}
