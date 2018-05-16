using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace GetProjectManagers
{

    public sealed class ProjectManager : CodeActivity
    {

        [Input("Project")]
        [ReferenceTarget("msdyn_project")]
        public InArgument<EntityReference> ProjectRecord { get; set; }

        [Input("Record Id")]
        public InArgument<string> RecordGuid { get; set; }

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

            string email = null;
            EntityCollection ec = GetRelatedUser(service, context);
            foreach (var item in ec.Entities)
            {
                email += item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString() +";";
            }

            List<EntityReference> efr = new List<EntityReference>();
            foreach (var item in ec.Entities)
            {
                Guid userId = new Guid(item.GetAttributeValue<AliasedValue>("prj.systemuserid").Value.ToString());
                efr.Add(new EntityReference("systemuser", userId));
            }

            users.Set(context, email);
            projManagers.Set(context, efr);
        }

        [Output("Project Managers")]
        public OutArgument<string> users { get; set; }

        [ReferenceTarget("systemuser")]
        [Output("Project Managers")]
        public OutArgument<EntityReference> projManagers { get; set; }

        public EntityCollection GetRelatedUser(IOrganizationService _service, CodeActivityContext context)
        {
            QueryExpression query = new QueryExpression(ProjectRecord.Get(context).LogicalName);
            string[] cols = { "msdyn_subject" };
            query.ColumnSet.Columns.AddRange(cols);
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition(ProjectRecord.Get(context).Id.ToString(), ConditionOperator.Equal, RecordGuid.Get(context).ToString());

            query.LinkEntities.Add(new LinkEntity("msdyn_project", "systemuser", "msdyn_projectid", "new_proj_userid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("internalemailaddress", "systemuserid");
            query.LinkEntities[0].EntityAlias = "prj";

            EntityCollection ec = _service.RetrieveMultiple(query);
            return ec;
        }
    }
}
