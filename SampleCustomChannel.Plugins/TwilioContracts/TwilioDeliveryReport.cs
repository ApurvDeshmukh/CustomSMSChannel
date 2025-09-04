namespace SampleCustomChannel.Plugins.TwilioContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    [DataContract]
    public class TwilioDeliveryReport
    {
        [DataMember]
        public string SmsSid { get; set; }
        [DataMember]
        public string SmsStatus { get; set; }
        [DataMember]
        public string MessageStatus { get; set; }
        [DataMember]
        public string ChannelToAddress { get; set; }
        public string To { get; set; }
        [DataMember]
        public string ChannelPrefix { get; set; }
        [DataMember]
        public string MessageSid { get; set; }
        [DataMember]
        public string AccountSid { get; set; }
        [DataMember]
        public string StructuredMessage { get; set; }
        [DataMember]
        public string From { get; set; }
        [DataMember]
        public string ApiVersion { get; set; }
        [DataMember]
        public string ChannelInstallSid { get; set; }
    }
}
