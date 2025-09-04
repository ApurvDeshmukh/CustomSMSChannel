namespace SampleCustomChannel.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Extensions;
    using Microsoft.Xrm.Sdk.Query;

    using SampleCustomChannel.Plugins.TwilioContracts;

    using System;
    using System.Collections.Generic;


    // This is a plugin for sending delivery report to Channel Definition Delivery report API
    // which should be called by a proxy service that processes incoming notifications from serice provider
    public class DeliveryReportPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.Get<ITracingService>();
            tracingService.Trace("Executing delivery report plugin");
            var pluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();
            var payload = pluginExecutionContext.InputParameters["payload"] as string;
            tracingService.Trace(payload);
            var organizationService = serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);

            var twilioDeliverReport = JsonUtils.Deserialize<TwilioDeliveryReport>(payload);

            // Only processing "delivered" status for demo purpose
            if (twilioDeliverReport.MessageStatus == "delivered")
            {
                // Find request id by message id
                var requestId = organizationService.RetrieveMultiple(new QueryExpression("cr65f_requestmessagemapping")
                {
                    ColumnSet = new ColumnSet("cr65f_requestid"),
                    Criteria = new FilterExpression()
                    {
                        Conditions = { new ConditionExpression("cr65f_messageid", ConditionOperator.Equal, twilioDeliverReport.MessageSid) }
                    }
                }).Entities[0].GetAttributeValue<string>("cr65f_requestid");

                var deliveryReport = new DeliveryReport()
                {
                    ChannelDefinitionId = Guid.Parse("b1b25a46-6da0-4c08-9cf3-505e613c8e30"),
                    // Twilio specific prefix
                    From = twilioDeliverReport.From.Replace("whatsapp:", ""),
                    MessageId = twilioDeliverReport.MessageSid,
                    RequestId = requestId,
                    Status = "Delivered",
                    OrganizationId = pluginExecutionContext.OrganizationId.ToString(),
                    StatusDetails = new Dictionary<string, object>()
                };

                var notificatonPayload = JsonUtils.Serialize(deliveryReport);
                tracingService.Trace("Notification payload: {0}", notificatonPayload);

                // Execution of Channel Definitions Notification API
                var response = organizationService.Execute(new OrganizationRequest("msdyn_D365ChannelsNotification")
                {
                    Parameters = {
                        { "notificationPayLoad", notificatonPayload }
                    }
                });

                // Using it for debugging purpose
                pluginExecutionContext.OutputParameters["response"] = response.Results["responseMessage"];
            }
        }
    }
}
