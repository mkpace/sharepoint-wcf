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
    public class Admin : BaseItem, IKPItem
    {
        [DataMember(Name = "KPUserName")]
        public string KPUserName { get; set; }
        [DataMember(Name = "AdminTools")]
        public bool AdminTools { get; set; }
        [DataMember(Name = "LockdownEditor")]
        public bool LockdownEditor { get; set; }
        [DataMember(Name = "Lockdown")]
        public bool Lockdown { get; set; }
        [DataMember(Name = "GoalReordering")]
        public bool GoalReordering { get; set; }
        [DataMember(Name = "GoalSanitization")]
        public bool GoalSanitization { get; set; }
        [DataMember(Name = "ImportGoals")]
        public bool ImportGoals { get; set; }
        [DataMember(Name = "EventLog")]
        public bool EventLog { get; set; }
        [DataMember(Name = "DeleteGoals")]
        public bool DeleteGoals { get; set; }
        [DataMember(Name = "UserLog")]
        public bool UserLog { get; set; }
        [DataMember(Name = "BusinessReview")]
        public bool BusinessReview { get; set; }
        [DataMember(Name = "GoalStrikethroughOverride")]
        public bool GoalStrikethroughOverride { get; set; }
        [DataMember(Name = "ConfigListTool")]
        public bool ConfigListTool { get; set; }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.KPUserName = item["KPUserName"].Value;
            this.AdminTools = ParseBool(item["AdminTools"].Value);
            this.LockdownEditor	= ParseBool(item["LockdownEditor"].Value);
            this.Lockdown = ParseBool(item["Lockdown"].Value);
            this.GoalReordering = ParseBool(item["GoalReordering"].Value);
            this.GoalSanitization = ParseBool(item["GoalSanitization"].Value);
            this.ImportGoals = ParseBool(item["ImportGoals"].Value);
            this.EventLog = ParseBool(item["EventLog"].Value);
            this.DeleteGoals = ParseBool(item["DeleteGoals"].Value);
            this.UserLog = ParseBool(item["UserLog"].Value);
            this.BusinessReview = ParseBool(item["BusinessReview"].Value);
            this.GoalStrikethroughOverride = ParseBool(item["GoalStrikethroughOverride"].Value);
            this.ConfigListTool = ParseBool(item["ConfigListTool"].Value);
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("KPUserName", new KPItem(this.KPUserName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("AdminTools", new KPItem(this.AdminTools, EntityConstants.ItemTypes.CHOICE));
            this.itemProperties.Add("LockdownEditor", new KPItem(this.LockdownEditor, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("Lockdown", new KPItem(this.Lockdown, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("GoalReordering", new KPItem(this.GoalReordering, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("GoalSanitization", new KPItem(this.GoalSanitization, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ImportGoals", new KPItem(this.ImportGoals, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("EventLog", new KPItem(this.EventLog, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("DeleteGoals", new KPItem(this.DeleteGoals, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("UserLog", new KPItem(this.UserLog, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("BusinessReview", new KPItem(this.BusinessReview, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("GoalStrikethroughOverride", new KPItem(this.GoalStrikethroughOverride, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ConfigListTool", new KPItem(this.ConfigListTool, EntityConstants.ItemTypes.YESNO));
            base.GetBaseProperties();
            return this.itemProperties;
        }
    }
}
