namespace SampleCustomChannel.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class DeliveryReport
    {
        [DataMember]
        public Guid ChannelDefinitionId { get; set; }

        [DataMember]
        public string RequestId { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public IDictionary<string, object> StatusDetails { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string OrganizationId { get; internal set; }
    }
}
