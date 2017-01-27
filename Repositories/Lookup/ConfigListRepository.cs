using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Security;

namespace Amazon.Kingpin.WCF2.Repositories
{
    /// <summary>
    /// ConfigList Repository holds info about user Teams
    /// TODO: Consider calling this UserAccount
    /// </summary>
    public class ConfigListRepository : LookupRepository<ConfigList>
    {
        #region Private member fields
        /// <summary>
        /// List name
        /// </summary>
        private string LIST_NAME = "KPConfigList";
        /// <summary>
        /// A cache of user objects - items created when GetUserAccount is called
        /// </summary>
        private static Dictionary<string, KPUser> userCache = new Dictionary<string, KPUser>();

        #endregion

        #region Public member properties
        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public override string ListName { get { return this.LIST_NAME; } }
        #endregion

        public ConfigListRepository(SPDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        #region Public member methods
        /// <summary>
        /// Get the user account info by Alias
        /// </summary>
        /// <param name="userAlias"></param>
        /// <returns></returns>
        public ConfigList GetItemByAlias(string userAlias)
        {
            this.Init();
            return this.Items.Find(c => c.KPUserName.Equals(userAlias));
        }

        /// <summary>
        /// Gets the user account information including
        /// the info from KPConfigList and KPAdmins
        /// TODO: should consider collapsing these lists
        /// into a single account list that includes access control
        /// </summary>
        /// <returns></returns>
        public void GetCurrentUserAccount(KPUser kpUser)
        {
            this.Init();
            // fetch config list to get list of teams
            //if (userCache.ContainsKey(kpUser.Alias) && cacheTimestamp.AddMinutes(30) < new DateTime())
            if (userCache.ContainsKey(kpUser.Alias))
            {
                kpUser = userCache[kpUser.Alias];
            }
            else
            {
                ConfigList config = this.Items.Find(c => c.KPUserName == kpUser.Alias);
                TeamRepository teamRepository = new TeamRepository(this.dataAccess);
                List<Team> teams = teamRepository.GetAllItems();
                kpUser.PrimaryTeam = teams.Find(t => t.KPID == config.PrimaryTeam);
                kpUser.Teams = teams.FindAll(t => config.AdditionalTeams.Contains(t.KPID));
            }
        }
        #endregion
    }
}
