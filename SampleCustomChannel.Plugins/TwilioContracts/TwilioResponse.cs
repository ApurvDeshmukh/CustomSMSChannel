namespace SampleCustomChannel.Plugins
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TwilioResponse
    {
        [DataMember(Name = "sid")]
        public string Sid { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
