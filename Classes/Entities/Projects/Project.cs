using Kingpin.WCF.Classes.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Amazon.Kingpin.WCF2.Data.Providers;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;
using Amazon.Kingpin.WCF2.Utilities;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class Project : BaseEntity, IKPEntity
    {
        // Public Serializable Properties
        [DataMember(Name = "AEReduction")]
        public int? AEReduction { get; set; }
        [DataMember(Name = "AssociatedProject")]
        public string AssociatedProject { get; set; }
        [DataMember(Name = "AvoidableEffort")]
        public int? AvoidableEffort { get; set; }
        [DataMember(Name = "EffortPerWorkload")]
        public int? EffortPerWorkload { get; set; }
        [DataMember(Name = "Efforts")]
        public string Efforts { get; set; }
        [DataMember(Name = "Measurement")]
        public string Measurement { get; set; }
        [DataMember(Name = "ParentProgramInitiative")]
        public string ParentProgramInitiative { get; set; }
        [DataMember(Name = "ProjectType")]
        public string ProjectType { get; set; }
        [DataMember(Name = "TotalEffort")]
        public int? TotalEffort { get; set; }
        [DataMember(Name = "TTMImpact")]
        public string TTMImpact { get; set; }
        [DataMember(Name = "Dependency")]
        public string Dependency { get; set; }
        [DataMember(Name = "Risks")]
        public string Risks { get; set; }
        [DataMember(Name = "RelatedGoals")]
        public string RelatedGoals { get; set; }
        [DataMember(Name = "RelatedMilestones")]
        public string RelatedMilestones { get; set; }
        [DataMember(Name = "RelatedEffortInstances")]
        public string RelatedEffortInstances { get; set; }
        [DataMember(Name = "ProjectVP")]
        public string ProjectVP { get; set; }
        [DataMember(Name = "ProjectOwner")]
        public string ProjectOwner { get; set; }
        [DataMember(Name = "ProjectDirectorVP")]
        public string ProjectDirectorVP { get; set; }
        [DataMember(Name = "RelatedIssues")]
        public string RelatedIssues { get; set; }
        [DataMember(Name = "Theme")]
        public int? Theme { get; set; }
        [DataMember(Name = "Year")]
        public int? Year { get; set; }

        //List<Project> Versions;

        public override void SetProperties(KPListItem item, string listName)
        {
            this.AEReduction = CheckNullInt("AEReduction", item);
            this.AssociatedProject = item["AssociatedProject"].Value;
            this.AvoidableEffort = CheckNullInt("AvoidableEffort", item);
            this.EffortPerWorkload = CheckNullInt("EffortPerWorkload", item);
            this.ParentProgramInitiative = item["ParentProgramInitiative"].Value;
            this.ProjectType = item["ProjectType"].Value;
            this.TotalEffort = CheckNullInt("TotalEffort", item);
            this.TTMImpact = item["TTMImpact"].Value;
            this.Dependency = KPUtilities.StripHTML(item["Dependency"].Value, false);
            this.Risks = KPUtilities.StripHTML(item["Risks"].Value, false);
            this.RelatedGoals = item["RelatedGoals"].Value;
            this.RelatedMilestones = item["RelatedMilestones"].Value;
            this.RelatedEffortInstances = (item.ContainsKey("RelatedEffortInstances")) ? item["RelatedEffortInstances"].Value : null;
            this.ProjectVP = item["ProjectVP"].Value;
            this.ProjectOwner = item["ProjectOwner"].Value;
            this.ProjectDirectorVP = item["ProjectDirectorVP"].Value;
            this.RelatedIssues = (item.ContainsKey("RelatedIssues")) ? item["RelatedIssues"].Value : null;
            this.Theme = CheckNullInt("Theme", item);
            this.Year = CheckNullInt("Year", item);
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
            this.itemProperties.Add("AEReduction", new KPItem(this.AEReduction, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("AssociatedProject", new KPItem(this.AssociatedProject, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("AvoidableEffort", new KPItem(this.AvoidableEffort, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("EffortPerWorkload", new KPItem(this.EffortPerWorkload, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("ParentProgramInitiative", new KPItem(this.ParentProgramInitiative, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ProjectType", new KPItem(this.ProjectType, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("TotalEffort", new KPItem(this.TotalEffort, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("TTMImpact", new KPItem(this.TTMImpact, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Dependency", new KPItem(this.Dependency, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Risks", new KPItem(this.Risks, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RelatedGoals", new KPItem(this.RelatedGoals, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RelatedMilestones", new KPItem(this.RelatedMilestones, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RelatedEffortInstances", new KPItem(this.RelatedEffortInstances, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ProjectVP", new KPItem(this.ProjectVP, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ProjectOwner", new KPItem(this.ProjectOwner, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("ProjectDirectorVP", new KPItem(this.ProjectDirectorVP, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("RelatedIssues", new KPItem(this.RelatedIssues, EntityConstants.ItemTypes.TEXT));
            this.itemProperties.Add("Theme", new KPItem(this.Theme, EntityConstants.ItemTypes.NUMBER));
            this.itemProperties.Add("Year", new KPItem(this.Year, EntityConstants.ItemTypes.NUMBER));
            base.GetEntityProperties();
            return this.itemProperties;
        }

    }
}
