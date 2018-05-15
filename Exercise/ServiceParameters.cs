using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Exercise
{
    public static class ServiceParameters
    {
        public static IOrganizationService _orgService;
        public static OrganizationServiceProxy _serviceProxy;
        public static Uri uri = new Uri(ConfigurationManager.AppSettings["urlName"]);
        public static Uri homeRealmUri = null;
        public static ClientCredentials deviceCredentials = null;
    }
}
