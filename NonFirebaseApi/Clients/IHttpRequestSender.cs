

using NonFirebaseApi.Models;

namespace NonFirebaseApi.Clients
{
    public interface IHttpRequestSender
    {
        Task<HttpResponseMessage> SendRequest(string url, string method, string requestBody = null, HttpHeaders customHeaders = null);
    }
}
