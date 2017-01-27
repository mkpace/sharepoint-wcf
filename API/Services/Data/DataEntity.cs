using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.API.Interfaces;
using Amazon.Kingpin.WCF2.Classes.Entities;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.Diagnostics;
using Amazon.Kingpin.WCF2.Http;
using Amazon.Kingpin.WCF2.Repositories;
using Newtonsoft.Json;

namespace Amazon.Kingpin.WCF2.API.Services.Data
{
    public partial class Data : IData
    {
        /**********************************
         *  ENTITY ACCESS METHODS 
         **********************************/

        /// <summary>
        /// Get the items from the specified entity list
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message EntityByNameGET(string entityName)
        {
            this.domainManager.SetCurrentUser();
            Message message = this.domainManager.GetUserEntities(ctx, entityName);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Message FileByNameGET(string teamId, string fileName)
        {
            this.domainManager.SetCurrentUser();
            Message message = this.domainManager.GetFileByName(ctx, teamId, fileName);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Message EntityByIdGET(string entityName, string entityKPID)
        {
            this.domainManager.SetCurrentUser();
            Message message = this.domainManager.GetEntityById(ctx, entityName, entityKPID);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        public Message EntityByTeamGET(string entityName, string teamIds)
        {
            this.domainManager.SetCurrentUser();
            Message message = this.domainManager.GetUserEntitiesByTeams(ctx, entityName, teamIds);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message EntitiesByNameByTeamGET(string entityNames, string teamIds)
        {
            this.domainManager.SetCurrentUser();
            Message message = this.domainManager.GetAllEntitiesByNameByTeam(ctx, entityNames, teamIds);
            return message;
        }

        /// <summary>
        /// Save an item to the specified entity list
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public Message EntityPOST(string entityName)
        {
            this.domainManager.SetCurrentUser();
            Message message = OperationContext.Current.RequestContext.RequestMessage;
            return this.domainManager.SaveEntity(ctx, entityName, message);
        }

        /// <summary>
        /// Update an item in the specified entity list
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Message EntityPUT(string entityName, string entityId)
        {
            this.domainManager.SetCurrentUser();
            Message msg = OperationContext.Current.RequestContext.RequestMessage;
            return this.domainManager.SaveEntity(ctx, entityName, msg);
        }

        public Message EntityDELETE(string entityName)
        {
            string response = "Entity DELETE not implemented.";
            return ctx.CreateJsonResponse<string>(response);
        }

        public Message EntityVersionsGET(string entityName, string entityKPID)
        {
            this.domainManager.SetCurrentUser();

            return this.domainManager.GetEntityVersionsById(ctx, entityName, entityKPID);
        }

    }
}
