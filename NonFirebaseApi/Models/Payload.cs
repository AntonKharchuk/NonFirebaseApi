namespace NonFirebaseApi.Models
{
    public class Payload
    {
        public Notification Notification { get; set; }
        public bool ContentAvailable { get; set; }
        public Data Data { get; set; }
        public string Priority { get; set; }
        public string To { get; set; }
    }
}
