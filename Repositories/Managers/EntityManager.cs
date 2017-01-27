using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.DataPersistence.Access;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Diagnostics;
using Amazon.Kingpin.WCF2.Http;
using Amazon.Kingpin.WCF2.Repositories.Base;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Utilities;
using Newtonsoft.Json;

namespace Amazon.Kingpin.WCF2.Repositories.Managers
{
    public class EntityManager
    {
        private SPDataAccess spDataAccess;
        private KPUser User;
        private KPTimer timer = new KPTimer();

        public EntityManager(SPDataAccess spDataAccess, KPUser kpUser)
        {
            this.spDataAccess = spDataAccess;
            // diagnostics timer
            this.timer = this.spDataAccess.Timer;
            this.User = kpUser;
        }

        #region Public Entity Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public void EnableCache(string entityName, bool enable)
        {
            switch (entityName)
            {
                case "KPAccomplishmentMisses":
                    EntityCache<Accomplishment>.EnableCache(enable);
                    break;
                case "KPAuditItems":
                    EntityCache<AuditItem>.EnableCache(enable);
                    break;
                case "KPEffortInstances":
                    EntityCache<EffortInstance>.EnableCache(enable);
                    break;
                case "KPGoals":
                    EntityCache<Goal>.EnableCache(enable);
                    break;
                case "KPKeyInsightsInnovations":
                    EntityCache<KeyInsightInnovation>.EnableCache(enable);
                    break;
                case "KPMilestones":
                    EntityCache<Milestone>.EnableCache(enable);
                    break;
                case "KPProjects":
                    EntityCache<Project>.EnableCache(enable);
                    break;

                // files
                case "KPDocuments":
                    EntityCache<Document>.EnableCache(enable);
                    break;
                case "KPReports":
                    EntityCache<Report>.EnableCache(enable);
                    break;
            }
        }

        /// <summary>
        /// Fetches the entity types that the user has access to.
        /// This method will fecth the named object types using the teams in the user's configuration information.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message GetUserEntities(WebOperationContext ctx, string entityName)
        {
            Message responseMsg = null;

            try
            {
                string ids = string.Join(";", this.spDataAccess.CurrentUser.Teams.Select(t => t.ID.ToString()).ToList());
                // primary team may or may not be included in list of additional teams
                Team primaryTeam = this.spDataAccess.CurrentUser.Teams.Find(t => t.KPID == this.spDataAccess.CurrentUser.PrimaryTeam.KPID);

                // check for primary team if it is not in add'l teams list add it 
                if (primaryTeam == null)
                {
                    if(!string.IsNullOrEmpty(ids))
                        ids = this.User.PrimaryTeam.KPID.ToString() + ";" + ids;   // (in front)
                    else
                        ids = string.Empty;          // user has no primary team meaning no access to any teams
                }

                return this.GetUserEntities(ctx, entityName, ids);
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
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public Message GetUserEntities(WebOperationContext ctx, string entityName, string teamIds)
        {
            Message responseMsg = null;
            List<int> teams = new List<int>();

            if(!string.IsNullOrEmpty(teamIds))
            {
                teams = teamIds.Split(GlobalConstants.MULTIVALUE_DELIMITER).Select(Int32.Parse).ToList();
            }

            try
            {
                // get the specified entities for the specified teams
                switch (entityName)
                {
                    case "KPAccomplishmentMisses":
                        List<Accomplishment> accomplishments = this.GetEntitiesByTeam<Accomplishment>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<Accomplishment>(ctx, accomplishments, this.timer);
                        break;
                    case "KPAuditItems":
                        List<AuditItem> auditItems = this.GetEntitiesByTeam<AuditItem>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<AuditItem>(ctx, auditItems, this.timer);
                        break;
                    case "KPEffortInstances":
                        List<EffortInstance> effortInstances = this.GetEntitiesByTeam<EffortInstance>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<EffortInstance>(ctx, effortInstances, this.timer);
                        break;
                    case "KPGoals":
                        List<Goal> goals = this.GetEntitiesByTeam<Goal>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<Goal>(ctx, goals, this.timer);
                        break;
                    case "KPKeyInsightsInnovations":
                        List<KeyInsightInnovation> keyInsightsInnovations = this.GetEntitiesByTeam<KeyInsightInnovation>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<KeyInsightInnovation>(ctx, keyInsightsInnovations, this.timer);
                        break;
                    case "KPMilestones":
                        List<Milestone> milestones = this.GetEntitiesByTeam<Milestone>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<Milestone>(ctx, milestones, this.timer);
                        break;
                    case "KPProjects":
                        List<Project> projects = this.GetEntitiesByTeam<Project>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<Project>(ctx, projects, this.timer);
                        break;

                    // files
                    case "KPDocuments":
                        List<Document> documents = this.GetFilesByTeam<Document>(entityName, teams);
                        responseMsg = HttpUtilities.GenerateResponse<Document>(ctx, documents, this.timer);
                        break;
                    case "KPReports":
                        List<Report> reports = this.GetFilesByTeam<Report>(entityName, teams);
                        // HACK: for data anomoly - KPTeam is saved with team name rather than KPID
                        // need to convert team name into KPID
                        reports.ForEach(r => r.KPTeam = ConvertTeamNameToKPID(r.KPTeam));
                        responseMsg = HttpUtilities.GenerateResponse<Report>(ctx, reports, this.timer);
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

        public Message GetEntityById(WebOperationContext ctx, string entityName, string entityKPID)
        {
            Message responseMsg = null;

            try
            {
                // get the specified entities for the specified teams
                switch (entityName)
                {
                    case "KPAccomplishmentMisses":
                        Accomplishment accomplishments = this.GetEntityById<Accomplishment>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Accomplishment>(ctx, accomplishments, this.timer);
                        break;
                    case "KPAuditItems":
                        AuditItem auditItems = this.GetEntityById<AuditItem>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<AuditItem>(ctx, auditItems, this.timer);
                        break;
                    case "KPEffortInstances":
                        EffortInstance effortInstances = this.GetEntityById<EffortInstance>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<EffortInstance>(ctx, effortInstances, this.timer);
                        break;
                    case "KPGoals":
                        Goal goals = this.GetEntityById<Goal>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Goal>(ctx, goals, this.timer);
                        break;
                    case "KPKeyInsightsInnovations":
                        KeyInsightInnovation keyInsightsInnovations = this.GetEntityById<KeyInsightInnovation>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<KeyInsightInnovation>(ctx, keyInsightsInnovations, this.timer);
                        break;
                    case "KPMilestones":
                        Milestone milestones = this.GetEntityById<Milestone>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Milestone>(ctx, milestones, this.timer);
                        break;
                    case "KPProjects":
                        Project projects = this.GetEntityById<Project>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Project>(ctx, projects, this.timer);
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

        public Message GetEntityVersionsById(WebOperationContext ctx, string entityName, string entityKPID)
        {
            Message responseMsg = null;

            try
            {
                // get the specified entities for the specified teams
                switch (entityName)
                {
                    case "KPAccomplishmentMisses":
                        List<Accomplishment> accomplishments = this.GetEntityVersionsById<Accomplishment>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Accomplishment>(ctx, accomplishments, this.timer);
                        break;
                    case "KPAuditItems":
                        List<AuditItem> auditItems = this.GetEntityVersionsById<AuditItem>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<AuditItem>(ctx, auditItems, this.timer);
                        break;
                    case "KPEffortInstances":
                        List<EffortInstance> effortInstances = this.GetEntityVersionsById<EffortInstance>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<EffortInstance>(ctx, effortInstances, this.timer);
                        break;
                    case "KPGoals":
                        List<Goal> goals = this.GetEntityVersionsById<Goal>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Goal>(ctx, goals, this.timer);
                        break;
                    case "KPKeyInsightsInnovations":
                        List<KeyInsightInnovation> keyInsightsInnovations = this.GetEntityVersionsById<KeyInsightInnovation>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<KeyInsightInnovation>(ctx, keyInsightsInnovations, this.timer);
                        break;
                    case "KPMilestones":
                        List<Milestone> milestones = this.GetEntityVersionsById<Milestone>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Milestone>(ctx, milestones, this.timer);
                        break;
                    case "KPProjects":
                        List<Project> projects = this.GetEntityVersionsById<Project>(entityName, entityKPID);
                        responseMsg = HttpUtilities.GenerateResponse<Project>(ctx, projects, this.timer);
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
        /// <param name="entityNames"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public Message GetAllEntitiesByNameByTeam(WebOperationContext ctx, string entityNames, string teamIds)
        {
            List<string> names;
            List<int> teams;
            EntityContainer entityContainer = new EntityContainer();

            List<KPListItem> listItems = new List<KPListItem>();
            if (string.IsNullOrEmpty(entityNames))
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return ctx.CreateJsonResponse<string>("Error: No entity names specified");
            }
            if (string.IsNullOrEmpty(teamIds))
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return ctx.CreateJsonResponse<string>("Error: No teams specified");
            }

            // validate data
            if (entityNames.Contains(GlobalConstants.MULTIVALUE_DELIMITER))
                names = entityNames.Split(GlobalConstants.MULTIVALUE_DELIMITER).ToList();
            else
                names = new List<string>() { entityNames };
            if (teamIds.Contains(GlobalConstants.MULTIVALUE_DELIMITER))
                teams = teamIds.Split(GlobalConstants.MULTIVALUE_DELIMITER).Select(Int32.Parse).ToList();
            else
                teams = new List<int>() { Int32.Parse(teamIds) };

            try
            {
                foreach (string name in names)
                {
                    // get entities by team & aggregate
                    this.GetUserEntities(entityContainer, name, teams);
                }
            }
            catch(Exception ex)
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return ctx.CreateJsonResponse<string>(string.Format("{0}: Stack Trace: {1}", ex.Message, ex.StackTrace));
            }

            return HttpUtilities.GenerateResponse<EntityContainer>(ctx, entityContainer, this.timer);
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
            Document document = this.GetFileByName<Document>("KPDocuments", teamId, fileName);
            return HttpUtilities.GenerateResponse<Document>(ctx, document, this.timer);
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
            Message responseMsg = null;
            string jsonData = JSON.GetPayload(dataMsg);
            string errMsg = string.Empty;
            try
            {
                switch (entityName)
                {
                    case "KPAccomplishmentMisses":
                        Accomplishment accomplishment = this.SaveUserEntity<Accomplishment>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Accomplishment>(ctx, accomplishment, this.timer);
                        break;
                    //case "KPDocuments":
                    //Goal goal = this.SaveUserEntity<Goal>(entityName, jsonData);
                    //responseMsg = ctx.CreateJsonResponse<Goal>(goal);
                    //    break;
                    case "KPGoals":
                        Goal goal = this.SaveUserEntity<Goal>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Goal>(ctx, goal, this.timer);
                        break;
                    case "KPKeyInsightsInnovations":
                        KeyInsightInnovation keyInsights = this.SaveUserEntity<KeyInsightInnovation>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<KeyInsightInnovation>(ctx, keyInsights, this.timer);
                        break;
                    case "KPMilestones":
                        Milestone milestone = this.SaveUserEntity<Milestone>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Milestone>(ctx, milestone, this.timer);
                        break;
                    case "KPProjects":
                        Project project = this.SaveUserEntity<Project>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Project>(ctx, project, this.timer);
                        break;
                    case "Views":
                        View view = this.SaveUserEntity<View>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<View>(ctx, view, this.timer);
                        break;
                    case "Checkpoints":
                        Checkpoint checkpoint = this.SaveUserEntity<Checkpoint>(entityName, jsonData);
                        responseMsg = HttpUtilities.GenerateResponse<Checkpoint>(ctx, checkpoint, this.timer);
                        break;
                    default:
                        errMsg = string.Format("Error: No entity list found with the name: {0}", entityName);
                        responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, new Exception(errMsg), "POST", HttpStatusCode.BadRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                responseMsg = HttpUtilities.GenerateExceptionResponse(ctx, ex, "POST", HttpStatusCode.InternalServerError);
            }

            this.timer.Stop();
            return responseMsg;
        }
        #endregion

        #region Private Member Methods
        /// <summary>
        /// Wrapper method calls EntityManager method of same name. 
        /// Gets all user entities by entity name(s) and team(s) - aggregates all results
        /// </summary>
        /// <param name="entityContainer"></param>
        /// <param name="entityName"></param>
        /// <param name="teams"></param>
        /// <returns></returns>
        private EntityContainer GetUserEntities(EntityContainer entityContainer, string entityName, List<int> teams)
        {
            try
            {
                // get the specified entities for the current user
                switch (entityName)
                {
                    case "KPAccomplishmentMisses":
                        List<Accomplishment> accomplishments = this.GetEntitiesByTeam<Accomplishment>(entityName, teams);
                        entityContainer.Accomplishments = accomplishments;
                        break;
                    case "KPGoals":
                        List<Goal> goals = this.GetEntitiesByTeam<Goal>(entityName, teams);
                        entityContainer.Goals = goals;
                        break;
                    case "KPKeyInsightsInnovations":
                        List<KeyInsightInnovation> keyInsightsInnovations = this.GetEntitiesByTeam<KeyInsightInnovation>(entityName, teams);
                        entityContainer.KeyInsightInnovations = keyInsightsInnovations;
                        break;
                    case "KPMilestones":
                        List<Milestone> milestones = this.GetEntitiesByTeam<Milestone>(entityName, teams);
                        entityContainer.Milestones = milestones;
                        break;
                    case "KPProjects":
                        List<Project> projects = this.GetEntitiesByTeam<Project>(entityName, teams);
                        entityContainer.Projects = projects;
                        break;
                    // files
                    case "KPDocuments":
                        List<Document> documents = this.GetFilesByTeam<Document>(entityName, teams);
                        entityContainer.Documents = documents;
                        break;
                    case "KPReports":
                        List<Report> reports = this.GetFilesByTeam<Report>(entityName, teams);
                        // HACK: for data anomoly - KPTeam is saved with team name rather than KPID
                        // need to convert team name into KPID
                        reports.ForEach(r => ConvertTeamNameToKPID(r.KPTeam));
                        entityContainer.Reports = reports;
                        break;

                    default:
                        string errMsg = string.Format("No entity list found with the name: {0}", entityName);
                        throw new Exception(errMsg);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in GetUserEntities: {0};", ex.Message), ex.InnerException);
            }

            return entityContainer;
        }

        /// <summary>
        /// HACK: helper method to convert team name values to team KPID
        /// </summary>
        /// <param name="kpTeamValue"></param>
        /// <returns></returns>
        private string ConvertTeamNameToKPID(string kpTeamValue)
        {
            int kpid = -1;     
            try
            {
                if (!Int32.TryParse(kpTeamValue, out kpid))
                {
                    Team team = this.spDataAccess.Teams.Find(t => t.Title == kpTeamValue);
                    kpTeamValue = team.KPID.ToString();
                }
            }
            catch(Exception)
            {
                //throw new Exception(string.Format("Error finding {0}: {1}", kpTeamValue, ex.Message));
                // set '0' for all teams not found - no match to name
                kpTeamValue = "0";
            }
            return kpTeamValue;
        }

        /// <summary>
        /// Generic method to support any Entity type.
        /// This method uses the BaseEntityRepository directly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        private T SaveUserEntity<T>(string listName, string jsonData) where T : IKPEntity, new()
        {
            T entity = JsonConvert.DeserializeObject<T>(jsonData);
            T item = default(T);
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            if (entity.KPID == -1 || entity.KPID == 0)
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
        private List<T> GetEntity<T>(string listName) where T : IKPEntity, new()
        {
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            List<T> items = repository.GetAllItems(this.User);
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="entityKPID"></param>
        /// <returns></returns>
        private T GetEntityById<T>(string listName, string entityKPID) where T : IKPEntity, new()
        {
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            T item = repository.GetItemById(entityKPID);
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <param name="entityKPID"></param>
        /// <returns></returns>
        private List<T> GetEntityVersionsById<T>(string listName, string entityKPID) where T : IKPEntity, new()
        {
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            List<T> items = repository.GetItemVersionsById(entityKPID);
            return items;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listName"></param>
        /// <returns></returns>
        private List<T> GetFiles<T>(string listName) where T : IKPEntity, new()
        {
            List<int> teams = this.User.Teams.Select(t => t.ID).ToList();
            List<T> items = this.GetFilesByTeam<T>(listName, teams);
            return items;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="libraryName"></param>
        /// <param name="teamId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private T GetFileByName<T>(string libraryName, string teamId, string fileName) where T : IKPEntity, new()
        {
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, libraryName);
            T item = repository.GetFileByName(teamId, fileName);
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teams"></param>
        /// <returns></returns>
        private List<T> GetEntitiesByTeam<T>(string listName, List<int> teams) where T : IKPEntity, new()
        {
            List<T> allItems = new List<T>();
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            if(teams != null)
            {
                foreach (int teamId in teams)
                {
                    List<T> items = repository.GetAllItemsByTeam(teamId);
                    allItems.AddRange(items);
                }
            }
            return allItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="teams"></param>
        /// <returns></returns>
        private List<T> GetFilesByTeam<T>(string listName, List<int> teams) where T : IKPEntity, new()
        {
            List<T> allItems = new List<T>();
            EntityRepository<T> repository = new EntityRepository<T>(this.spDataAccess, listName);
            foreach (int teamId in teams)
            {
                List<T> items = repository.GetAllFilesByTeam(teamId);
                allItems.AddRange(items);
            }
            return allItems;
        }        
        #endregion
    }
}
