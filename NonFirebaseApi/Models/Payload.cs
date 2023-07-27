namespace NonFirebaseApi.Models
{
    public class Payload
    {
        public Notification notification { get; set; }
        public bool contentAvailable { get; set; }
        public Data data { get; set; }
        public string priority { get; set; }
        public string to { get; set; }
    }
}
