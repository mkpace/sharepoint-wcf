using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class EffortInstance : BaseEntity, IKPEntity
    {
        // Public Serializable Properties
        // are marked with this attribute to declare which
        // properties are to be serialized and what their 
        // JSON representation should be. If we wish to create
        // proper camel-cased names then we would change these attribute names
        [DataMember(Name = "IsAccomplishment")]
        public int? TotalEffort { get; set; }
        [DataMember(Name = "AvoidableEffort")]
        public int? AvoidableEffort { get; set; }
        [DataMember(Name = "TimesPerYear")]
        public int? TimesPerYear { get; set; }
        [DataMember(Name = "InvestmentToReduceAE")]
        public int? InvestmentToReduceAE { get; set; }
        [DataMember(Name = "KeyAssumptions")]
        public string KeyAssumptions { get; set; }
        [DataMember(Name = "WorkloadDescription")]
        public string WorkloadDescription { get; set; }
        [DataMember(Name = "WorkloadsPerYear")]
        public int? WorkloadsPerYear { get; set; }
        [DataMember(Name = "TaskRole")]
        public string TaskRole { get; set; }
        [DataMember(Name = "ReductionType")]
        public string ReductionType { get; set; }
        [DataMember(Name = "UnavoidableEffort")]
        public int? UnavoidableEffort { get; set; }
        [DataMember(Name = "KPType")]
        public string KPType { get; set; }
        [DataMember(Name = "ProjectedAvoidableEffortReduction")]
        public int? ProjectedAvoidableEffortReduction { get; set; }
        [DataMember(Name = "ProjectedTimesPerYearReduction")]
        public int? ProjectedTimesPerYearReduction { get; set; }
        [DataMember(Name = "RelatedEffortInstances")]
        public string RelatedEffortInstances { get; set; }
        [DataMember(Name = "RelatedProject")]
        public string RelatedProject { get; set; }
        [DataMember(Name = "ConfidenceLevel")]
        public string ConfidenceLevel { get; set; }

        /// <summary>
        /// Set the properties of this object.
        /// This method is used by the DataAccess layer (DAL) 
        /// to create new object instances from the underlying 
        /// data persistence layer (DataProvider)
        /// </summary>
        /// <param name="item"></param>
        public override void SetProperties(KPListItem item, string listName)
        {
            this.TotalEffort = ParseInt(item["TotalEffort"].Value);
            this.AvoidableEffort = ParseInt(item["AvoidableEffort"].Value);
            this.TimesPerYear = ParseInt(item["TimesPerYear"].Value);
            this.InvestmentToReduceAE = ParseInt(item["InvestmentToReduceAE"].Value);
            this.KeyAssumptions = KPUtilities.StripHTML(item["KeyAssumptions"].Value, false);
            this.WorkloadDescription = KPUtilities.StripHTML(item["WorkloadDescription"].Value, false);
            this.WorkloadsPerYear = ParseInt(item["WorkloadsPerYear"].Value);
            this.TaskRole = item["TaskRole"].Value;
            this.ReductionType = item["ReductionType"].Value;
            this.UnavoidableEffort = ParseInt(item["UnavoidableEffort"].Value);
            this.KPType = item["KPType"].Value;
            this.ProjectedAvoidableEffortReduction = ParseInt(item["ProjectedAvoidableEffortReduction"].Value);
            this.ProjectedTimesPerYearReduction = ParseInt(item["ProjectedTimesPerYearReduction"].Value);
            this.RelatedEffortInstances = item["RelatedEffortInstances"].Value;
            this.RelatedProject = item["RelatedProject"].Value;
            this.ConfidenceLevel = item["ConfidenceLevel"].Value;
            base.SetProperties(item, listName);
        }

        /// <summary>
        /// Get the properties of this object instance.
        /// This method is used by the DataAccess layer (DAL) 
        /// to serialize the object instance to persist the 
        /// data to the underlying data storage (DataProvider)
        /// </summary>
        /// <returns></returns>
        public KPListItem GetProperties()
        {
            this.itemProperties = new KPListItem();
            //this.itemProperties.Add("", new KPItem(this.IsAccomplishment, EntityConstants.ItemTypes.YESNO));

            this.itemProperties.Add("TotalEffort", new KPItem(this.TotalEffort, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("AvoidableEffort", new KPItem(this.AvoidableEffort, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("TimesPerYear", new KPItem(this.TimesPerYear, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("InvestmentToReduceAE", new KPItem(this.InvestmentToReduceAE, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("KeyAssumptions", new KPItem(this.KeyAssumptions, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("WorkloadDescription", new KPItem(this.WorkloadDescription, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("WorkloadsPerYear", new KPItem(this.WorkloadsPerYear, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("TaskRole", new KPItem(this.TaskRole, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ReductionType", new KPItem(this.ReductionType, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("UnavoidableEffort", new KPItem(this.UnavoidableEffort, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("KPType", new KPItem(this.KPType, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ProjectedAvoidableEffortReduction", new KPItem(this.ProjectedAvoidableEffortReduction, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ProjectedTimesPerYearReduction", new KPItem(this.ProjectedTimesPerYearReduction, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("RelatedEffortInstances", new KPItem(this.RelatedEffortInstances, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("RelatedProject", new KPItem(this.RelatedProject, EntityConstants.ItemTypes.YESNO));
            this.itemProperties.Add("ConfidenceLevel", new KPItem(this.ConfidenceLevel, EntityConstants.ItemTypes.YESNO));

            base.GetEntityProperties();
            return this.itemProperties;
        }
    }
}
