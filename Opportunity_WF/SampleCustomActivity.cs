using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Opportunity_WF
{
    public class SampleCustomActivity : CodeActivity
    {



        protected override void Execute(CodeActivityContext context)
        {
            //Create the tracing service
            ITracingService tracingService = context.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext wfContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(wfContext.UserId);

            tracingService.Trace("Custom Workflow Activity");

            //Get Estimated closed Date
            DateTime estimatedDateTime = context.GetValue(this.EstimatedDateTime);

            //Get Today Date
            DateTime todayDate = context.GetValue(this.TodayDate);

            //Get Current DateTime
            //DateTime todayTime = DateTime.Now;
            int missedCount = 0;

            //Check if Estimated close date has elapsed
            if (todayDate > estimatedDateTime)
            {
                /*TODO:
                 * Step 1: check if missed commitment level count contains data
                 * Step 2: If yes, save data in a local variable. If no, set local variable to 0
                 * Step 3: Increment local varable by 1
                 * Step 4: Set local Variable to Workflow Output.
                 * Step 5: Get the name of the intiating user
                 * Step 6: Save the details of the intiating user to another entity in CRM.
                 */

                //Check commitment count
                int missedCommitmentCount = context.GetValue(this.MissedCommitmentCount);

                //Set an optionset to Missed Committment
                this.CommittmentLevel.Set(context, new OptionSetValue(4));


                //increment count
                missedCount = missedCommitmentCount + 1;


                //set a field to display missed commitment level count
                this.MissedCommitmentCount.Set(context, missedCount);
            }

        }



        #region CRM-Variables
        [RequiredArgument]
        [Input("Estimated Time Date")]
        public InArgument<DateTime> EstimatedDateTime { get; set; }

        [Input("Today Date")]
        public InArgument<DateTime> TodayDate { get; set; }

        [Input("Committment Level")]
        [Output("Commitment Level")]
        public InOutArgument<OptionSetValue> CommittmentLevel { get; set; }

        [Output("Missed Commitment Count")]
        [Input("Missed Commitment Count")]
        public InOutArgument<int> MissedCommitmentCount { get; set; }

        #endregion

    }
}
