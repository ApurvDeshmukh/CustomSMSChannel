namespace SampleCustomChannel.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    [DataContract]
    public class Response
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
        public Dictionary<string, object> StatusDetails { get; set; }
    }
}
