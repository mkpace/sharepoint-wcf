using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Data.Access;

namespace Amazon.Kingpin.WCF2.Repositories
{
    /// <summary>
    /// Team Repository
    /// Lookup Repositories inherit from BaseLookupRepository. The abstract base
    /// instantiates the SPDataAccess for use in all concrete Repository classes.
    /// The Repository class interfaces with the SPDataAccess and does not include
    /// any code or implementation for the Data Source (SPDataAccess) to maintain a strict SoC.
    /// </summary>
    public class TeamRepository : LookupRepository<Team>
    {
        #region Private member fields
        /// <summary>
        /// List Name
        /// </summary>
        private string LIST_NAME = "KPTeams";
        /// <summary>
        /// Current root team id in list
        /// </summary>    
        private int ROOT_TEAM = 0;
        /// <summary>
        /// Cache - not implemented
        /// </summary>
        private Dictionary<string, Team> cache = new Dictionary<string, Team>();

        #endregion

        #region Public member properties
        /// <summary>
        /// Collection of Entity objects of type T
        /// Getter only property for all the teams
        /// </summary>
        public override List<Team> Items { get; set; }
        /// <summary>
        /// Static property holds same valuse as Items
        /// </summary>
        public static List<Team> Teams { get; set; }

        /// <summary>
        /// Getter for the Team Hierarchies
        /// </summary>
        public List<Team> TeamHierarchy { get; private set; }
        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public override string ListName { get { return this.LIST_NAME; } }
        #endregion

        public TeamRepository(SPDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        #region Public member methods
        public override void Init()
        {
            base.Init();
            TeamRepository.Teams = this.Items;
            // create team paths
            this.CreateTeamPaths();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static Team GetTeam(int teamId)
        {
            if(TeamRepository.Teams != null)
                return TeamRepository.Teams.Find(t => t.KPID.Equals(teamId));
            else
            {
                TeamRepository repo = new TeamRepository(new SPDataAccess());
                repo.Init();
                return repo.Items.Find(t => t.KPID.Equals(teamId));
            }
        }

        /// <summary>
        /// Helper method to create team hierarchy & paths
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<Team> CreateTeamHierarchy(int teamId)
        {
            if (this.Items == null)
                this.Init();

            List<Team> rootTeams = this.Items.Where(t => t.ParentId.Equals(teamId)).ToList();
            foreach (Team rootTeam in rootTeams)
            {
                this.GetChildTeams(rootTeam);
            }
            return rootTeams;
        }
        #endregion

        #region Private member methods
        /// <summary>
        /// Recursive method to build team hierarchy and create team URLs
        /// </summary>
        /// <param name="parentTeam"></param>
        private void GetChildTeams(Team parentTeam)
        {
            List<Team> childTeams = this.Items.Where(c => c.ParentId.Equals(parentTeam.KPID)).ToList();
            if (parentTeam.Children == null) { parentTeam.Children = new List<Team>(); }
            parentTeam.Children.AddRange(childTeams);
            foreach (Team childTeam in childTeams)
            {
                //childTeam.AddParentSiteUrl(parentTeam.SiteUrl);
                GetChildTeams(childTeam);
            }
        }

        /// <summary>
        /// Creates the team paths for teams based on parents
        /// </summary>
        private void CreateTeamPaths()
        {
            if (this.Items == null)
                this.Init();

            List<Team> rootTeams = this.Items.Where(t => t.ParentId.Equals(ROOT_TEAM)).ToList();
            foreach (Team rootTeam in rootTeams)
            {
                this.FindChildTeams(rootTeam);
            }

        }

        private void FindChildTeams(Team parentTeam)
        {
            List<Team> childTeams = this.Items.Where(c => c.ParentId.Equals(parentTeam.KPID)).ToList();
            foreach(Team childTeam in childTeams)
            {
                // add parent path
                childTeam.SetParentUrl(parentTeam.SiteUrl);
                GetChildTeams(childTeam);
            }
        }

        public List<int> GetChildTeamIds(List<Team> teams, int rootTeamId)
        {
            List<int> teamIds = new List<int>() {};
            List<int> childTeamIds = teams.Where(t => t.ParentId == rootTeamId).Select(t => t.KPID).ToList();
            foreach (int childTeamId in childTeamIds)
            {
                teamIds.Add(childTeamId);
                teamIds.AddRange(GetChildTeamIds(teams, childTeamId));
            }
            return teamIds;
        }

        #endregion
    }
}
