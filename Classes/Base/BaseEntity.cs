using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Repositories;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class BaseEntity : BaseItem
    {
        /// <summary>
        /// Represents an exernal ID from another system
        /// This is used on import as a way to track a 'primary key'
        /// </summary>
        [DataMember(Name = "KPExternalID")]
        public int? KPExternalId { get; set; }

        [DataMember(Name = "KPTeam")]
        public int? KPTeamId { get; set; }

        [DataMember(Name = "KPDate")]
        public DateTime? KPDate { get; set; }

        [DataMember(Name = "KPCreatedDate")]
        public DateTime? KPCreatedDate { get; set; }

        [DataMember(Name = "KPOriginalDate")]
        public DateTime? KPOriginalDate { get; set; }

        [DataMember(Name = "KPCompletionDate")]
        public DateTime? KPCompletionDate { get; set; }

        [DataMember(Name = "KPItemState")]
        public string KPItemState { get; set; }

        [DataMember(Name = "KPCustomers")]
        public string KPCustomerIDs { get; set; } // multi lookup "x1;x2;x3"

        public List<string> KPCustomers { get; set; }

        public Team KPTeam { get; set; }

        [DataMember(Name = "KPMovedFromValue")]
        public string KPMovedFromValue { get; set; } // multi lookup "x1;x2;x3"
        public List<int> KPMovedFrom { get; set; }

        [DataMember(Name = "KPMovedToValue")]
        public string KPMovedToValue { get; set; } // lookup "x1"
        public Team KPMovedTo { get; set; }

        [DataMember(Name = "KPExecSummary")]
        public string KPExecSummary { get; set; }

        [DataMember(Name = "KPStatus")]
        public string KPStatus { get; set; }

        [DataMember(Name = "KPComments")]
        public string KPComments { get; set; }

        [DataMember(Name = "KPSoWhat")]
        public string KPSoWhat { get; set; }

        [DataMember(Name = "KPPathToGreen")]
        public string KPPathToGreen { get; set; }

        [DataMember(Name = "KPBusinessOwner")]
        public string KPBusinessOwner { get; set; }

        [DataMember(Name = "KPFinanceOwner")]
        public string KPFinanceOwner { get; set; }

        [DataMember(Name = "KPDescription")]
        public string KPDescription { get; set; }

        /// <summary>
        /// PrimaryVP ID from/to WCF
        /// </summary>
        [DataMember(Name = "KPPrimaryVP")]
        public int? KPPrimaryVPId { get; set; }

        /// <summary>
        /// Id's provided as ';' delimited list of Id's
        /// </summary>
        [DataMember(Name = "KPSecondaryVPs")]
        public string KPSecondaryVPIDs { get; set; }
        //public List<VP> KPSecondaryVPs { get; set; }

        [DataMember(Name = "KPAttachment")]
        public string KPAttachment { get; set; } // Attached Documents

        [DataMember(Name = "DraftMode")]
        public bool DraftMode { get; set; }

        /// <summary>
        /// Setting properties 
        /// The object 'rehydrated' here is an entity
        /// for consumption by external code
        /// </summary>
        /// <param name="item"></param>
        public virtual void SetProperties(KPListItem item, string listName)
        {
            try
            {
                this.KPExternalId = item.ContainsKey("KPExternalID") ? ParseInt(item["KPExternalID"].Value) : -1;

                this.KPDate = ParseDateTime(item["KPDate"].Value);

                // these values do not exist for all entity types
                this.KPCreatedDate = item.ContainsKey("KPCreatedDate") ? ParseDateTime(item["KPCreatedDate"].Value) : null;
                this.KPOriginalDate = item.ContainsKey("KPOriginalDate") ? ParseDateTime(item["KPOriginalDate"].Value) : null;
                this.KPCompletionDate = item.ContainsKey("KPCompletionDate") ? ParseDateTime(item["KPCompletionDate"].Value) : null;
                this.KPCustomerIDs = item.ContainsKey("KPCustomers") ? item["KPCustomers"].Value : string.Empty;
                this.KPCustomers = null;

                this.KPItemState = item["KPItemState"].Value;

                this.KPTeamId = item.ContainsKey("KPTeam") ? ParseInt(item["KPTeam"].Value) : -1;

                this.KPMovedFromValue = item.ContainsKey("KPMovedFrom") ? item["KPMovedFrom"].Value : string.Empty;
                this.KPMovedFrom = null;

                this.KPMovedToValue = item.ContainsKey("KPMovedTo") ? item["KPMovedTo"].Value : string.Empty;
                this.KPMovedTo = null;

                this.KPExecSummary = KPUtilities.StripHTML(item.ContainsKey("KPExecSummary") ? item["KPExecSummary"].Value : string.Empty, false);
                this.KPStatus = item["KPStatus"].Value;
                this.KPComments = KPUtilities.StripHTML(item.ContainsKey("KPComments") ? item["KPComments"].Value : string.Empty, false);
                this.KPSoWhat = KPUtilities.StripHTML(item.ContainsKey("KPSoWhat") ? item["KPSoWhat"].Value : string.Empty, false);
                this.KPPathToGreen = KPUtilities.StripHTML(item.ContainsKey("KPPathToGreen") ? item["KPPathToGreen"].Value : string.Empty, false);

                this.KPBusinessOwner = item.ContainsKey("KPBusinessOwner") ? item["KPBusinessOwner"].Value : string.Empty;
                this.KPFinanceOwner = item.ContainsKey("KPFinanceOwner") ? item["KPFinanceOwner"].Value : string.Empty;

                this.KPDescription = KPUtilities.StripHTML((item.ContainsKey("KPDescription")) ? item["KPDescription"].Value : string.Empty, false);

                this.KPPrimaryVPId = item.ContainsKey("KPPrimaryVP") ? ParseInt(item["KPPrimaryVP"].Value) : -1;

                this.KPSecondaryVPIDs = item.ContainsKey("KPSecondaryVPs") ? item["KPSecondaryVPs"].Value : string.Empty;

                this.KPAttachment = item.ContainsKey("KPAttachment") ? item["KPAttachment"].Value : string.Empty; // Attached Documents

                this.DraftMode = ParseBool(item["DraftMode"].Value);
            }
            catch(Exception ex)
            {
                string errMsg = string.Format("Error setting BaseEntity properties on {0}: {1}", listName, ex.Message);
                throw new Exception(errMsg);
            }

            // set additional core item properties
            base.SetBaseProperties(item, listName);
        }

        /// <summary>
        /// Getting entity properties
        /// Getting the object properties as a KPListItem
        /// to be consumed by the DataProvider to assist with
        /// serialization of the object to be persisted
        /// </summary>
        /// <returns></returns>
        public KPListItem GetEntityProperties()
        {
            if(this.KPExternalId.HasValue)
                this.itemProperties.Add("KPExternalID", new KPItem(this.KPExternalId.Value, EntityConstants.ItemTypes.NUMBER));

            this.itemProperties.Add("KPCreatedDate", new KPItem(this.KPCreatedDate, EntityConstants.ItemTypes.DATE));
            this.itemProperties.Add("KPOriginalDate", new KPItem(this.KPOriginalDate, EntityConstants.ItemTypes.DATE));
            this.itemProperties.Add("KPDate", new KPItem(this.KPDate, EntityConstants.ItemTypes.DATE));
            this.itemProperties.Add("KPCompletionDate", new KPItem(this.KPCompletionDate, EntityConstants.ItemTypes.DATE));
            this.itemProperties.Add("KPItemState", new KPItem(this.KPItemState, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPCustomers", new KPItem(this.KPCustomerIDs, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPTeam", new KPItem(this.KPTeamId.Value, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPMovedFrom", new KPItem(this.KPMovedFrom, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPExecSummary", new KPItem(this.KPExecSummary, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("KPStatus", new KPItem(this.KPStatus, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPComments", new KPItem(this.KPComments, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("KPSoWhat", new KPItem(this.KPSoWhat, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("KPPathToGreen", new KPItem(this.KPPathToGreen, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("KPBusinessOwner", new KPItem(this.KPBusinessOwner, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPFinanceOwner", new KPItem(this.KPFinanceOwner, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPDescription", new KPItem(this.KPDescription, EntityConstants.ItemTypes.NOTE));
            this.itemProperties.Add("KPPrimaryVP", new KPItem(this.KPPrimaryVPId, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPSecondaryVPs", new KPItem(this.KPSecondaryVPIDs, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("KPAttachment", new KPItem(this.KPAttachment, EntityConstants.ItemTypes.JSON));
            this.itemProperties.Add("DraftMode", new KPItem(this.DraftMode, EntityConstants.ItemTypes.YESNO));
            base.GetBaseProperties();
            return this.itemProperties;
        }


        public string Type { get; set; }
    }

}
