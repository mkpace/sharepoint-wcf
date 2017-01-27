using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Security;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Repositories.Base;
using System;
using Amazon.Kingpin.WCF2.Data.Access;

namespace Amazon.Kingpin.WCF2.Repositories
{
    /// <summary>
    /// Goal Repository
    /// Entity Repositories inherit from BaseEntityRepository. The abstract base
    /// instantiates the SPDataAccess for use in all concrete Repository classes.
    /// The Repository class interfaces with the SPDataAccess and does not include
    /// any code or implementation for the Data Source (SPDataAccess) to maintain a strict SoC.
    /// </summary>
    public class GoalRepository : EntityRepository<Goal>
    {
        /// <summary>
        /// List Name
        /// </summary>
        private string LIST_NAME = "KPGoals";

        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public override string ListName { get { return this.LIST_NAME; } }

        public GoalRepository(SPDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        /// <summary>
        /// TODO: this method uses the Index to get the specific item from an Org list
        /// Need to implement the ItemIndex
        /// </summary>
        /// <param name="kpGUID"></param>
        /// <returns></returns>
        public Goal GetGoal(int teamId, int KPID)
        {
            //nt teamId = this.dataAccess.GetIndexedItem(KPID);
            string teamUrl = this.dataAccess.CurrentUser.GetTeamUrl(teamId);
            Goal goal = this.dataAccess.GetEntityObjectByKPID<Goal>(teamUrl, this.ListName, KPID);
            return goal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpGUID"></param>
        /// <returns></returns>
        //public Goal GetGoalByTeamId(string kpGUID, int teamId)
        //{
        //    string teamUrl = this.dataAccess.CurrentUser.GetTeamUrl(teamId);
        //    Goal goal = this.dataAccess.GetEntityObject<Goal>(teamUrl, this.ListName, kpGUID);
        //    return goal;
        //}

        public List<Goal> GetGoalsByTeam(int teamId)
        {
            Team team = TeamRepository.GetTeam(teamId);
            //if (this.dataAccess.CurrentUser.Teams.Contains(team))
                return this.dataAccess.GetEntityObjects<Goal>(team.SiteUrl, this.LIST_NAME);
            //else
                // TODO: make this a type AccessDeniedException
           //     throw new Exception("User does not have access to this team.");
        }

        public List<Goal> FilterGoals(Dictionary<string, int> filters)
        {
            return new List<Goal>();
        }

        public List<Goal> SortGoals()
        {
            return new List<Goal>();
        }

    }
}
