namespace NonFirebaseApi.Models
{
    public class HttpHeaders : Dictionary<string, string>
    {
        public void AddHeader(string key, string value)
        {
            this[key] = value;
        }
    }
}
