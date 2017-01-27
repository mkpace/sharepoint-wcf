using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Entities
{
    [DataContract()]
    public class EntityContainer
    {
        [DataMember(Name = "KPAccomplishmentMisses", EmitDefaultValue = false)]
        public List<Accomplishment> Accomplishments { get; set; }
        [DataMember(Name = "KPGoals", EmitDefaultValue = false)]
        public List<Goal> Goals { get; set; }
        [DataMember(Name = "KPKeyInsightsInnovations", EmitDefaultValue = false)]
        public List<KeyInsightInnovation> KeyInsightInnovations { get; set; }
        [DataMember(Name = "KPMilestones", EmitDefaultValue = false)]
        public List<Milestone> Milestones { get; set; }
        [DataMember(Name = "KPProjects", EmitDefaultValue = false)]
        public List<Project> Projects { get; set; }

        [DataMember(Name = "KPDocuments", EmitDefaultValue = false)]
        public List<Document> Documents { get; set; }
        [DataMember(Name = "KPReports", EmitDefaultValue = false)]
        public List<Report> Reports { get; set; }
    }
}
