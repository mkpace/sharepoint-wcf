using Amazon.Kingpin.WCF2.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.Diagnostics;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Classes.Base;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using System.Runtime.Serialization;

namespace Amazon.Kingpin.WCF2.Classes.Diagnostics
{
    [DataContract()]
    internal class EventLogger : BaseItem, IKPItem
    {
        private SPDataProvider spDataProvider = null;
        private string LIST_NAME = "EventLogging";

        [DataMember(Name = "Action")]
        internal string Action { get; set; }
        [DataMember(Name = "ErrorMessage")]
        internal string ErrorMessage { get; set; }
        [DataMember(Name = "FieldNames")]
        internal string FieldNames { get; set; }
        [DataMember(Name = "KPUserName")]
        internal string KPUserName { get; set; }
        [DataMember(Name = "ListName")]
        internal string ListName { get; set; }
        [DataMember(Name = "StackTrace")]
        internal string StackTrace { get; set; }
        [DataMember(Name = "TeamSiteName")]
        internal string TeamSiteName { get; set; }

        internal static KPTimer kpTimer;

        static EventLogger()
        {
            kpTimer = new KPTimer();
        }

        public EventLogger() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spUtilitites"></param>
        internal EventLogger(SPDataProvider spDataProvider)
        {
            this.spDataProvider = spDataProvider;
            kpTimer = new KPTimer();
        }

        /// <summary>
        /// Implements IKPEntity interface
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void SetProperties(KPListItem item, string listName)
        {
            this.Action = item["Action"].Value;
            this.ErrorMessage = item["ErrorMessage"].Value;
            this.FieldNames = item["FieldNames"].Value;
            this.ListName = item["ListName"].Value;
            this.KPUserName = item["KPUserName"].Value;
            this.StackTrace = item["StackTrace"].Value;
            this.TeamSiteName = item["TeamSiteName"].Value;
            base.SetBaseProperties(item, listName);
        }
        /// <summary>
        /// Returns a KPList item that represents this object instance
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            this.itemProperties.Add("Action", new KPItem(this.Action, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ErrorMessage", new KPItem(this.ErrorMessage, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("FieldNames", new KPItem(this.FieldNames, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ListName", new KPItem(this.ListName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPUserName", new KPItem(this.KPUserName, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("StackTrace", new KPItem(this.StackTrace, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("TeamSiteName", new KPItem(this.TeamSiteName, EntityConstants.ItemTypes.TEXT));
            base.GetBaseProperties();
            return this.itemProperties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal void GetObject(SPListItem item)
        {
            this.Title = item["Title"].ToString();
            this.Action = item["Action"].ToString();
            this.ErrorMessage = item["ErrorMessage"].ToString();
            this.FieldNames = item["FieldNames"].ToString();
            this.KPGUID = item["KPGUID"].ToString();
            this.KPUserName = item["CreatedBy"].ToString();
            this.ListName = item["ListName"].ToString();
            this.StackTrace = item["StackTrace"].ToString();
            this.TeamSiteName = item["TeamSiteName"].ToString();
        }

        /// <summary>
        /// Adds event to EventLogging list
        /// </summary>
        internal void LogEvent()
        {
            SPList list = this.spDataProvider.GetCommonWeb().Lists.TryGetList(LIST_NAME);
            SPListItem item = list.AddItem();
            item["Title"] = this.Title;
            item["Action"] = this.Action;
            item["ErrorMessage"] = this.ErrorMessage;
            item["FieldNames"] = this.FieldNames;
            item["KPGUID"] = this.KPGUID;
            item["KPUserName"] = this.KPUserName;
            item["ListName"] = this.ListName;
            item["StackTrace"] = this.StackTrace;
            item["TeamSiteName"] = this.TeamSiteName;
            item.Update();
        }

        internal static KPTimer Timer
        {
            get
            {
                return kpTimer;
            }
        }

        /// <summary>
        /// Helper method to log to diagnostics window - wraps 
        /// the native method Debug.WriteLine - to simplify this call and assign a category
        /// TODO: eventually we can connect this to the logger and output 
        /// to another DB or to a SharePoint list
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void WriteLine(string format, params object[] args)
        {
            string message = string.Format(format, args);
            System.Diagnostics.Debug.WriteLine(message, "DEBUG-Kingpin");
        }

        internal static void WriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message, "DEBUG-Kingpin");
        }
    }
}
