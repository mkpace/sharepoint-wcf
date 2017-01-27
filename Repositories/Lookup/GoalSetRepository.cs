using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;

namespace Amazon.Kingpin.WCF2.Repositories.Lookup
{
    public class GoalSetRepository : LookupRepository<GoalSet>
    {
        #region Private member fields
        /// <summary>
        /// List Name
        /// </summary>
        private string LIST_NAME = "KPGoalSets";

        /// <summary>
        /// Cache - not implemented
        /// </summary>
        private Dictionary<string, GoalSet> cache = new Dictionary<string, GoalSet>();

        #endregion

        public GoalSetRepository(SPDataAccess spDataAccess) : base(spDataAccess) { }

        #region Public member properties
        /// <summary>
        /// Collection of Entity objects of type T
        /// Getter only property for all the teams
        /// </summary>
        public override List<GoalSet> Items { get; set; }
        /// <summary>
        /// Static property holds same values as Items
        /// ** this is only a helper property - duplicates instance property Items
        /// </summary>
        public static List<GoalSet> GoalSets { get; set; }

        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public override string ListName { get { return this.LIST_NAME; } }
        #endregion

        #region Public member methods
        public override void Init()
        {
            base.Init();
            GoalSetRepository.GoalSets = this.Items;
        }

        #endregion

    }
}
