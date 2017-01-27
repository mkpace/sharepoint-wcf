using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.API.Interfaces;

namespace Amazon.Kingpin.WCF2.API.Services.Data
{
    public partial class Data : IData
    {
        /// <summary>
        /// Enables or disables cach based on bool state
        /// </summary>
        /// <param name="entityName">Name of the entity in cache to affect (scope is entity only - does not include team</param>
        /// <param name="state">Enable or disable</param>
        /// <returns>State message</returns>
        public Message EntityCacheEnableGET(string entityName, string enable)
        {
            this.domainManager.EnableCache(entityName, bool.Parse(enable));
            string response = (bool.Parse(enable)) ? "Cache enabled" : "Cache disabled";
            return ctx.CreateJsonResponse<string>(response);
        }

    }
}
