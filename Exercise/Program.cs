using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using static Exercise.Utilty;
using static Exercise.ServiceParameters;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk;
using RelianceXRM;

namespace Exercise
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientCredentials credentials = GetCredentials();
            //Force connection to TLS1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (_serviceProxy = new OrganizationServiceProxy(uri, homeRealmUri, credentials, deviceCredentials))
            {
                _serviceProxy.EnableProxyTypes();
                _orgService = _serviceProxy;

                ProjectManager2(_serviceProxy);

                Console.ReadLine();

            }
        }

        public static void ProjectManager()
        {
            QueryExpression q = new QueryExpression("msdyn_projecttask");
            string[] cols = { "msdyn_subject", "createdon" };
            q.ColumnSet.Columns.AddRange(cols);

            //link to project entity
            q.LinkEntities.Add(new LinkEntity("msdyn_projecttask", "msdyn_project", "msdyn_projecttaskid", "msdyn_projectid", JoinOperator.Inner));
            q.ColumnSet.Columns.AddRange("msdyn_project", "msdyn_subject");
            q.LinkEntities[0].EntityAlias = "prj";

            //link to user
            q.LinkEntities[0].AddLink("systemuser", "msdyn_projectid", "systemuserid", JoinOperator.Inner);
            q.LinkEntities[0].LinkEntities[0].EntityAlias = "prjManager";
            q.LinkEntities[0].LinkEntities[0].Columns = new ColumnSet("domainname", "internalemailaddress");

            EntityCollection ec = _serviceProxy.RetrieveMultiple(q);

            foreach (var item in ec.Entities)
            {
                var user = item.GetAttributeValue<AliasedValue>("prjManager.domainname").Value.ToString();
                var email = item.GetAttributeValue<AliasedValue>("prjManager.internalemailaddress").Value.ToString();

                Console.WriteLine($"User: {user},  Email: {email}");

            }
        }

        public static void ProjectManager2(IOrganizationService _service)
        {
            //QueryExpression q = new QueryExpression("systemuser");
            //string[] cols = { "domainname", "internalemailaddress" };
            //q.ColumnSet.Columns.AddRange(cols);

            ////link to project entity
            //q.LinkEntities.Add(new LinkEntity("systemuser", "msdyn_project", "new_proj_userId", "msdyn_projectid", JoinOperator.Inner));
            //q.ColumnSet.Columns.AddRange("domainname", "internalemailaddress");
            //q.LinkEntities[0].EntityAlias = "prj";

            //EntityCollection ec = _serviceProxy.RetrieveMultiple(q);

            //foreach (var item in ec.Entities)
            //{
            //    var user = item.GetAttributeValue<AliasedValue>("prj.domainname").Value.ToString();
            //    var email = item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString();

            //    Console.WriteLine($"User: {user},  Email: {email}");

            //}

            //using (RelianceInfoServiceContext svc = new RelianceInfoServiceContext(_service))
            //{
            //    var project = (from u in svc.SystemUserSet
            //                   join p in svc.msdyn_projectSet on
            //                   u.new_proj_userId.Id equals p.Id
            //                   select new
            //                   {
            //                       user = u.FullName,
            //                       email = u.InternalEMailAddress,
            //                       name = p.msdyn_subject
            //                   }).ToList();
            //    project.ForEach(a => Console.WriteLine($"Name: {a.name}, User: {a.user}, Email: {a.email}"));

            //}

            QueryExpression query = new QueryExpression("msdyn_project");
            string[] cols = { "msdyn_subject" };
            //query.ColumnSet = new ColumnSet(cols);
            query.ColumnSet.Columns.AddRange(cols);

            query.LinkEntities.Add(new LinkEntity("msdyn_project", "systemuser", "msdyn_projectid", "new_proj_userid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("internalemailaddress");
            query.LinkEntities[0].EntityAlias = "prj";

            EntityCollection ec = _service.RetrieveMultiple(query);
            foreach (var item in ec.Entities)
            {
                var user = item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString();
                var project = item.Attributes["msdyn_subject"].ToString();
            }

            //foreach (var item in project)
            //{
            //    Console.WriteLine($"Project Name: {item.name}");
            //    var user = (from u in svc.SystemUserSet
            //                join t in svc.CreateQuery("proj-user") on 
            //               where u.Id == item.users.Id
            //                select new
            //               {
            //                   fullname = u.GetAttributeValue<AliasedValue>("domainname").Value.ToString(),
            //                   email = u.GetAttributeValue<AliasedValue>("internalemailaddress").Value.ToString(),
            //               }).ToList();
            //    user.ForEach(u => Console.WriteLine($"Name: {u.fullname}, Email: {u.email}"));
            //}
        }
    }
}

