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
        [Input("From:")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> FromUser { get; set; }

        [Input("To:")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> ToUser { get; set; }

        [Input("Subject:")]
        public InArgument<string> Subject { get; set; }


        [Input("Record Id")]
        [ReferenceTarget("msdyn_project")]
        public InArgument<EntityReference> RecordGuid { get; set; }



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
            Guid recordGuid = wfContext.PrimaryEntityId;

            //Get List of PMs in a project
            Dictionary<Guid, string> pmDetails = new Dictionary<Guid, string>();
            EntityCollection ec = GetRelatedUser(service, recordGuid);
            foreach (var item in ec.Entities)
            {
                Guid pmGuid = (Guid)item.GetAttributeValue<AliasedValue>("prj.systemuserid").Value;
                string pmEmail = item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString();
                pmDetails.Add(pmGuid, pmEmail);
            }

            //Send Email to PM
            SendEmail(context, pmDetails, wfContext, service);
        }

        public EntityCollection GetRelatedUser(IOrganizationService _service, Guid recordGuid)
        {
            QueryExpression query = new QueryExpression("msdyn_project");
            string[] cols = { "msdyn_subject" };
            //query.ColumnSet = new ColumnSet(cols);
            query.ColumnSet.Columns.AddRange(cols);
            //query.Criteria = new FilterExpression();
            //query.Criteria.AddCondition(recordGuid.ToString(), ConditionOperator.Equal, recordGuid.ToString());

            query.LinkEntities.Add(new LinkEntity("msdyn_project", "systemuser", "msdyn_projectid", "new_proj_userid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("internalemailaddress", "systemuserid");
            query.LinkEntities[0].EntityAlias = "prj";

            EntityCollection ec = _service.RetrieveMultiple(query);
            //foreach (var item in ec.Entities)
            //{
                //var user = item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString();
                //var project = item.Attributes["msdyn_subject"].ToString();
                //yield return item.GetAttributeValue<AliasedValue>("prj.internalemailaddress").Value.ToString();
            //}

            return ec;
        }

        public void SendEmail(CodeActivityContext context, Dictionary<Guid, string> pmDetails, IWorkflowContext wfContext, IOrganizationService _service)
        {
            //From: From User
            //To: List of PM

            ActivityParty fromParty = new ActivityParty()
            {
                PartyId = new EntityReference("systemuser", FromUser.Get(context).Id)
            };

            ActivityParty toParty = new ActivityParty()
            {
                PartyId = new EntityReference(ToUser.Get(context).LogicalName, ToUser.Get(context).Id)
            };

            List<ActivityParty> ccActivityPartyList = new List<ActivityParty>();
            foreach (var item in pmDetails)
            {
                ActivityParty ccParty = new ActivityParty
                {
                    PartyId = new EntityReference(ToUser.Get(context).LogicalName, item.Key)
                };
                ccActivityPartyList.Add(ccParty);
            }

            Email email = new Email
            {
                To = new ActivityParty[] { toParty },
                From = new ActivityParty[] { fromParty },
                Subject = Subject.Get(context),
                Description = "Dear User <br /> The timeframe to complete project task has elapsed.",
                DirectionCode = true,
                Cc = ccActivityPartyList,
                RegardingObjectId = new EntityReference("msdyn_project", wfContext.PrimaryEntityId),
            };

            Guid _emailId = _service.Create(email);

            SendEmailRequest sendEmailreq = new SendEmailRequest
            {
                EmailId = _emailId,
                TrackingToken = "",
                IssueSend = true
            };

            SendEmailResponse sendEmailresp = (SendEmailResponse)_service.Execute(sendEmailreq);
        }
    }
}


