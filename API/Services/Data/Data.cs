using Microsoft.SharePoint.Client.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.API.Interfaces;
using Amazon.Kingpin.WCF2.Exceptions;
using Amazon.Kingpin.WCF2.Repositories;
using Amazon.Kingpin.WCF2.Data.Access;
using Amazon.Kingpin.WCF2.Classes.Lookup;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Microsoft.SharePoint.Administration;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;
using Amazon.Kingpin.WCF2.Http;


namespace Amazon.Kingpin.WCF2.API.Services.Data
{
    [BasicHttpBindingServiceMetadataExchangeEndpointAttribute]
    [AspNetCompatibilityRequirementsAttribute(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [DataContractAttribute]
    public partial class Data : IData
    {
        private WebOperationContext ctx = WebOperationContext.Current;
        private DomainManager domainManager;

        /// <summary>
        /// Default Ctor
        /// </summary>
        public Data()
        {
            this.ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
            this.domainManager = new DomainManager();
        }
    }
}
