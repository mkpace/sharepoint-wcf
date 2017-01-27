using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingpin.WCF.Classes.Data.Entities.Users
{
    public class User
    {
        public string LoginName { get; set; }
        public string Alias { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        public User(string user)
        {
            string[] delim = { ",#" };
            string[] userFields = user.Split(delim, StringSplitOptions.None);
            LoginName = userFields[1];
            Email = userFields[2];
            FullName = userFields[4];

            int index;
            // Get Alias from LoginName
            index = LoginName.IndexOf(@"ANT\", StringComparison.CurrentCultureIgnoreCase);
            if (index > -1)
                Alias = LoginName.Substring(index + 4);

            // Ensure email exists
            if (string.IsNullOrEmpty(Email)) // No email but Alias is present
            {
                if (!string.IsNullOrEmpty(Alias))
                    Email = Alias + "@amazon.com";
            }
            else
                if (string.IsNullOrEmpty(Alias)) // If Email is present, but login name missing
                {
                    index = Email.IndexOf("@");
                    Alias = Email.Substring(0, index);
                }

            // Remove extra Commas in Full Name Ex/ Ngo,, Billy
            FullName = FullName.Replace(",,", ",");
        }
    }
}
