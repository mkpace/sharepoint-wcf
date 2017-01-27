using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Data.Access;

namespace Amazon.Kingpin.WCF2.Repositories.Lookup
{
    public class VPRepository : LookupRepository<VP>
    {
        #region Private member fields
        /// <summary>
        /// List Name
        /// </summary>
        private string LIST_NAME = "KPVPs";
        /// <summary>
        /// Cache - not implemented
        /// </summary>
        private Dictionary<string, VP> cache = new Dictionary<string, VP>();

        #endregion

        public VPRepository(SPDataAccess spDataAccess) : base(spDataAccess) { }

        #region Public member properties
        /// <summary>
        /// Collection of Entity objects of type T
        /// Getter only property for all the teams
        /// </summary>
        public override List<VP> Items { get; set; }
        /// <summary>
        /// Static property holds same values as Items
        /// ** this is only a helper property - duplicates instance property Items
        /// </summary>
        public static List<VP> VPs { get; set; }

        /// <summary>
        /// Lookup List name to query
        /// </summary>
        public override string ListName { get { return this.LIST_NAME; } }
        #endregion

        #region Public member methods
        public override void Init()
        {
            base.Init();
            VPRepository.VPs = this.Items;
        }

        #endregion
    }
}
