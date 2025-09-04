namespace SampleCustomChannel.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Extensions;
    using Microsoft.Xrm.Sdk.Query;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;

    // Sample plugin that sends a WhatsApp message using Twilio
    public class OutboundPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.Get<ITracingService>();
            tracingService.Trace("Executing outbound sample channel plugin");
            var pluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();

            // "payload" attribute is required by contract
            var payload = pluginExecutionContext.InputParameters["payload"] as string;
            tracingService.Trace(payload);

            var organizationService = serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);

            var payloadObject = JsonUtils.Deserialize<Payload>(payload);
            var credentials = this.GetCredentials(organizationService, payloadObject.ChannelDefinitionId, payloadObject.From);

            var responseString = SendTwilioRequest(credentials.AccountId, credentials.Token, payloadObject, tracingService);
            tracingService.Trace(responseString);

            var twilioResponse = JsonUtils.Deserialize<TwilioResponse>(responseString);

            // Saving request id with message id to be able to find it for delivery reports
            organizationService.Create(new Entity("cr65f_requestmessagemapping")
            {
                ["cr65f_messageid"] = twilioResponse.Sid,
                ["cr65f_requestid"] = payloadObject.RequestId
            });

            var responseObject = new Response()
            {
                ChannelDefinitionId = payloadObject.ChannelDefinitionId,
                MessageId = twilioResponse.Sid,
                RequestId = payloadObject.RequestId,
                // Translate provider statuses to Marketing statuses
                Status = twilioResponse.Status == "queued" ? "Sent" : "SendingFailed",
                StatusDetails = null
            };

            // "response" attribute is required by contract
            pluginExecutionContext.OutputParameters["response"] = JsonUtils.Serialize(responseObject);
        }

        private Credentials GetCredentials(IOrganizationService organizationService, Guid channelDefinitionId, string from)
        {
            var channelInstances = organizationService.RetrieveMultiple(new QueryExpression("msdyn_channelinstance")
            {
                ColumnSet = new ColumnSet("msdyn_extendedentityid"),
                Criteria = new FilterExpression()
                {
                    Conditions = {
                        new ConditionExpression("msdyn_channeldefinitionid", ConditionOperator.Equal, channelDefinitionId),
                        new ConditionExpression("msdyn_contactpoint", ConditionOperator.Equal, from)
                    }
                }
            });

            // Only one channel instance can exist for give definition id and contact point
            var extendedChannelInstance = channelInstances.Entities[0].GetAttributeValue<EntityReference>("msdyn_extendedentityid");

            var sampleChannelInstance = organizationService.Retrieve(extendedChannelInstance.LogicalName, extendedChannelInstance.Id, new ColumnSet("cr65f_accountid", "cr65f_authtoken"));
            var twilioSid = sampleChannelInstance.GetAttributeValue<string>("cr65f_accountid");
            var twilioToken = sampleChannelInstance.GetAttributeValue<string>("cr65f_authtoken");

            return new Credentials
            {
                AccountId = twilioSid,
                Token = twilioToken,
            };
        }

        private static string SendTwilioRequest(string twilioSid, string twilioToken, Payload payloadObject, ITracingService tracingService)
        {
            var request = WebRequest.CreateHttp($"https://api.twilio.com/2010-04-01/Accounts/{twilioSid}/Messages.json");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{twilioSid}:{twilioToken}"));
            request.Headers.Add(HttpRequestHeader.Authorization, $"Basic {basicAuth}");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("From", $"whatsapp:{payloadObject.From}");
            queryString.Add("To", $"whatsapp:{payloadObject.To}");

            // Your message parts here
            queryString.Add("Body", $"{payloadObject.Message["text"]}");
            string postdata = queryString.ToString();
            var message = Encoding.UTF8.GetBytes(postdata);

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(message, 0, postdata.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            } catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    using (var response = (HttpWebResponse)exception.Response)
                    using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var content = streamReader.ReadToEnd();
                        tracingService.Trace("Failed to call twilio with: {0}", content);
                    }
                }

                throw;
            }
        }

        private class Credentials
        {
            public string AccountId { get; set; }

            public string Token { get; set; }
        }
    }
}
