using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Lookup
{
    [DataContract()]
    public class ConfigList : BaseItem, IKPItem
    {
        [DataMember(Name = "FullName")]
        public string FullName { get; set; }
        [DataMember(Name = "KPUserName")]
        public string KPUserName { get; set; }
        [DataMember(Name = "PrimaryTeam")]
        public int PrimaryTeam { get; set; }
        [DataMember(Name = "AdditionalTeams")]
        public List<int> AdditionalTeams { get; set; }
        [DataMember(Name = "GoalsTab")]
        public bool GoalsTab { get; set; }
        [DataMember(Name = "BusinessReviewTab")]
        public bool BusinessReviewTab { get; set; }
        [DataMember(Name = "RecentEdits")]
        public List<int> RecentEdits { get; set; }
        [DataMember(Name = "RecentReports")]
        public string RecentReports { get; set; }
        [DataMember(Name = "VersionNumber")]
        public string VersionNumber { get; set; }
        [DataMember(Name = "DefaultView")]
        public string DefaultView { get; set; }

        /// <summary>
        /// Default param-less Ctor
        /// </summary>
        public ConfigList() { }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.FullName = item["FullName"].Value;
            this.KPUserName = item["KPUserName"].Value;
            this.PrimaryTeam = ParseInt(item["PrimaryTeam"].Value);
            this.AdditionalTeams = ConvertListInt(item["AdditionalTeams"].Value);
            this.GoalsTab = ParseBool(item["GoalsTab"].Value);
            this.BusinessReviewTab = ParseBool(item["BusinessReviewTab"].Value);
            this.RecentEdits = ConvertListInt(item["RecentEdits"].Value);
            this.RecentReports = item["RecentReports"].Value;
            this.VersionNumber = item["VersionNumber"].Value;
            this.DefaultView = item["DefaultView"].Value;
            base.SetBaseProperties(item, listName);
        }

        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            // get instance properties
            this.itemProperties.Add("FullName", new KPItem(this.FullName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPUserName", new KPItem(this.KPUserName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("PrimaryTeam", new KPItem(this.PrimaryTeam, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("AdditionalTeams", new KPItem(this.AdditionalTeams, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("GoalsTab", new KPItem(this.GoalsTab, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("BusinessReviewTab", new KPItem(this.BusinessReviewTab, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("RecentEdits", new KPItem(this.RecentEdits, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RecentReports", new KPItem(this.RecentReports, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("VersionNumber", new KPItem(this.VersionNumber, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("DefaultView", new KPItem(this.DefaultView, EntityConstants.ItemTypes.TEXT));
            // add base properties
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
