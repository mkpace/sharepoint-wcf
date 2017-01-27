using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.API.Interfaces;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.Repositories;
using Microsoft.SharePoint.Client.Services;

namespace Amazon.Kingpin.WCF2.API.Services.Data
{
    public partial class Data : IData
    {
        /**********************************
         *  LOOKUP ACCESS METHODS 
         **********************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message LookupGET(string entityName)
        {
            EventLogger.WriteLine("Lookup GET: {0}", entityName);
            return this.domainManager.GetLookup(ctx, entityName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message LookupMaxGET(string entityName, string count)
        {
            EventLogger.WriteLine("Lookup GET: {0}", entityName);
            return this.domainManager.GetLookup(ctx, entityName, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message LookupPOST(string entityName)
        {
            Message msg = OperationContext.Current.RequestContext.RequestMessage;
            return this.domainManager.SaveLookup(ctx, entityName, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Message LookupPUT(string entityName, string entityId)
        {
            Message msg = OperationContext.Current.RequestContext.RequestMessage;
            return this.domainManager.SaveLookup(ctx, entityName, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public Message LookupAllChildrenGET(string parentID)
        {
            EventLogger.WriteLine("Lookup All Children of ParentID GET: {0}", parentID);
            return this.domainManager.GetAllChildren(ctx, parentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAlias"></param>
        /// <returns></returns>
        public Message LookupUserConfigGET(string userAlias)
        {
            EventLogger.WriteLine("ConfigList User Teams GET: {0}", userAlias);
            return this.domainManager.GetConfigListItemByAlias(ctx, userAlias);
        }
    }
}
