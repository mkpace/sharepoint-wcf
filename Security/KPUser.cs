using System;
using System.Collections.Generic;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;

namespace Amazon.Kingpin.WCF2.Security
{
    public class KPUser
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Alias { get; set; }
        public string LoginName { get; set; }
        public Team PrimaryTeam { get; set; }
        public List<Team> Teams { get; set; }
        public DateTime? CacheTime { get; set; }
        /// <summary>
        /// Describes access to resources in Kingpin
        /// The access control value is a bit mask
        /// </summary>
        public int AccessControl { get; set; }
        public bool IsSiteAdmin { get; set; }
        public KPUser() { }

        /// <summary>
        /// Ctor creates a new user object from the supplied item
        /// </summary>
        /// <param name="item"></param>
        public KPUser(KPListItem item)
        {
            this.FirstName = item["FirstName"].Value;
        }

        /// <summary>
        /// Get the team url for this user
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public string GetTeamUrl(int teamId)
        {
            //return "/Consumer/Consumer/NASales";
            string errMsg = "Team with TeamId: {0} not found.";
            Team team = this.Teams.Find(t => t.KPID == teamId);
            if (team != null)
                return team.SiteUrl;
            else
                throw new Exception(string.Format(errMsg, teamId));
        }

        public static KPUser CreateVersionUser(string user)
        {
            KPUser kpUser = new KPUser();
            string[] delim = { ",#" };
            string[] userFields = user.Split(delim, StringSplitOptions.None);
            kpUser.LoginName = userFields[1];
            kpUser.Email = userFields[2];
            kpUser.FullName = userFields[4];

            // Get Alias from LoginName
            GetAliasFromLoginName(kpUser);
            // Ensure email exists
            EnsureEmail(kpUser);
            // Remove extra Commas in Full Name Ex/ Ngo,, Billy
            GetFullName(kpUser);

            return kpUser;
        }

        /// <summary>
        // Get Alias from LoginName
        /// </summary>
        /// <param name="kpUser"></param>
        private static void GetAliasFromLoginName(KPUser kpUser)
        {
            // Get Alias from LoginName
            int index = kpUser.LoginName.IndexOf(@"ANT\", StringComparison.CurrentCultureIgnoreCase);
            if (index > -1)
                kpUser.Alias = kpUser.LoginName.Substring(index + 4);
        }
        /// <summary>
        // Ensure email exists
        /// </summary>
        /// <param name="kpUser"></param>
        private static void EnsureEmail(KPUser kpUser)
        {
            // Ensure email exists
            if (string.IsNullOrEmpty(kpUser.Email)) // No email but Alias is present
            {
                if (!string.IsNullOrEmpty(kpUser.Alias))
                    kpUser.Email = kpUser.Alias + "@amazon.com";
            }
            else
                if (string.IsNullOrEmpty(kpUser.Alias)) // If Email is present, but login name missing
                {
                    int index = kpUser.Email.IndexOf("@");
                    kpUser.Alias = kpUser.Email.Substring(0, index);
                }
        }
        /// <summary>
        // Remove extra Commas in Full Name Ex/ Ngo,, Billy
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private static void GetFullName(KPUser kpUser)
        {
            // Remove extra Commas in Full Name Ex/ Ngo,, Billy
            kpUser.FullName = kpUser.FullName.Replace(",,", ",");
        }

    }

}
