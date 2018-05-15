using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Exercise
{
    public static class Utilty
    {
        public static ClientCredentials GetCredentials()
        {
            var appSettings = ConfigurationManager.AppSettings;

            ClientCredentials cred = new ClientCredentials();
            cred.UserName.UserName = appSettings["username"]; //"jide@trandemo.onmicrosoft.com";
            cred.UserName.Password = appSettings["password"]; //"pass@word1";
            return cred;
        }
    }
}
