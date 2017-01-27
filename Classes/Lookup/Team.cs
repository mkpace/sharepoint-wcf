using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Lookup
{
    [DataContract()]
    public class Team : BaseItem, IKPItem
    {
        private string siteUrl;
 
        /// <summary>
        /// Specific object properties
        /// </summary>
        [DataMember(Name = "Nick")]
        public string Nick { get; set; }

        [DataMember(Name = "Parent")]
        public int? ParentId { get; set; }

        [DataMember(Name = "SubUrl")]
        public string SubUrl { get; set; }

        [DataMember(Name = "TeamVision")]
        public string TeamVision { get; set; }

        [DataMember(Name = "TeamIntro")]
        public string TeamIntro { get; set; }

        // don't need to serialize these properties
        public bool IsRoot { get { return this.ParentId < 1; } }

        public List<Team> Children { get; set; }

        /// <summary>
        /// Returns the site url for this team
        /// Site collection root is duplicated "Consumer/Consumer"
        /// </summary>
        public string SiteUrl
        {
            get { return this.siteUrl; }
        }

        /// <summary>
        /// Default param-less Ctor
        /// </summary>
        public Team()
        {
            // set values to -1 if they should not be added to SP list item
            this.ParentId = -1;
            this.Children = new List<Team>();
        }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            // set item properties to serialize object
            this.itemProperties = item;
            this.Nick = item["Nick"].Value;
            this.ParentId = CheckNullInt("Parent", item);
            this.SubUrl = item["SubUrl"].Value;
            this.TeamVision = KPUtilities.StripHTML(item["TeamVision"].Value, false);
            this.TeamIntro = KPUtilities.StripHTML(item["TeamIntro"].Value, false);
            this.siteUrl = GetTeamUrl();
            base.SetBaseProperties(item, listName);
        }

        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {

            this.itemProperties = new KPListItem();
            // parent id can be '0' but in SP it's null so set it to empty
            string parentId = (this.ParentId > -1) ? this.ParentId.ToString() : string.Empty;
            string orderIndex = (this.OrderIndex > -1) ? this.OrderIndex.ToString() : string.Empty;
            // get instance properties
            this.itemProperties.Add("Nick", new KPItem(this.Nick, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Parent", new KPItem(parentId, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("SubUrl", new KPItem(this.SubUrl, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("TeamVision", new KPItem(this.TeamVision, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("TeamIntro", new KPItem(this.TeamIntro, EntityConstants.ItemTypes.NOTE));
            // add base properties
            base.GetBaseProperties();
            return this.itemProperties;
        }

        public void SetParentUrl(string parentSiteUrl)
        {
            this.siteUrl = parentSiteUrl + this.siteUrl;
        }
        /// <summary>
        /// Helper method to create team url
        /// TODO: INCOMPLETE
        /// </summary>
        /// <returns></returns>
        private string GetTeamUrl()
        {
            string url = string.Empty;
            // root teams duplicate the team name (subUrl)
            string root = "/{0}/{0}";
            string child = "/{0}";
            string format = child;
            if (this.IsRoot)
            {
                format = root;
            }
            if (string.IsNullOrEmpty(this.siteUrl))
            {
                url = string.Format(format, this.SubUrl);
            }
            else
            {
                url = this.siteUrl;
            }
            return url;
        }
    }

}
