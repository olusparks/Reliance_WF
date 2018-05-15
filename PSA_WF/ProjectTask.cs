using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RelianceXRM;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace PSA_WF
{

    public sealed class ProjectTask : CodeActivity
    {
        [Input("From Id")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> FromUser { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            //Create the tracing service
            ITracingService tracingService = context.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext wfContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(wfContext.UserId);

            tracingService.Trace("Custom Workflow Activity");

            //using (RelianceInfoServiceContext svc = new RelianceInfoServiceContext(service))
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
            //}

        }

        public EntityCollection getRelatedUser(IOrganizationService _service)
        {
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

            return ec;
        }
    }
}


