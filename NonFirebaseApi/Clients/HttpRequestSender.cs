using NonFirebaseApi.Clients;
using NonFirebaseApi.Models;
using System;
using System.Net.Http;

using System.Threading.Tasks;

public class HttpRequestSender: IHttpRequestSender
{
    private readonly HttpClient _client;

    public HttpRequestSender()
    {
        _client = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendRequest(string url, string method, string requestBody = null,
        HttpHeaders customHeaders = null)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Set custom headers if provided
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Set request body if provided
        if (!string.IsNullOrEmpty(requestBody))
        {
            request.Content = new StringContent(requestBody);
        }

        // Send the request and get the response
        var response = await _client.SendAsync(request);

        return response;
    }
}

