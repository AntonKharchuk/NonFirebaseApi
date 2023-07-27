using Microsoft.AspNetCore.Mvc;
using NonFirebaseApi.Clients;
using NonFirebaseApi.Models;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;

namespace NonFirebaseApi.Controllers
{
    public class FireBaseController : ControllerBase
    {

        private readonly string _messageTokenListPath;
        private readonly string _allMessagesListPath;

        private IHttpRequestSender _requestSender;

        public FireBaseController(IHttpRequestSender httpRequestSender)
        {
            _requestSender = httpRequestSender;
            _messageTokenListPath = "D:\\Code\\C#\\ynik\\NonFirebaseApi\\NonFirebaseApi\\MessageTokenList.txt";
            _allMessagesListPath = "D:\\Code\\C#\\ynik\\NonFirebaseApi\\NonFirebaseApi\\AllMessages.txt";
        }
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return Ok("Well come to API");
        }


        [HttpPost("set-message-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetMessageToken([FromBody] MessageToken body)
        {
            if (body != null)
            {
                try
                {
                    var addedNewToken = await SaveTockenToTxt(body.Token);
                    if (addedNewToken)
                        return Ok("Token has been added to List");
                    else
                        return Ok("Token is allready in the List");

                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error while adding to list\n" + e.Message);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Token is missing in body");
            }
        }




        [HttpPost("send-message")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message != null)
            {
                try
                {
                    
                    using (StreamReader sr = new StreamReader(_messageTokenListPath))
                    {
                        List<string> tokenList;

                        var content = await sr.ReadToEndAsync();
                        if (string.IsNullOrEmpty(content))
                        {
                            return StatusCode(500, "No tokens in List");

                        }
                        else
                        {
                            tokenList = JsonConvert.DeserializeObject<List<string>>(content);
                            foreach (var token in tokenList)
                            {
                               var response = await SendMessageToSever(message.Text, token);
                            }
                            return StatusCode(200, "Messages sended");
                        }
                    }


                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error while sending\n"+e.Message);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Message is missing in body");
            }
        }
        private async Task<HttpResponseMessage> SendMessageToSever(string text, string to)
        {
            // Create custom headers
            var customHeaders = new HttpHeaders();
            customHeaders.Add("Content-Type", "application/json");
            customHeaders.Add("Authorization", "key=AAAAQvTfT2U:APA91bHwVMCCZj8NC7hfk5ITNi1CYnT2Au-PVRazTCG4_rHRUt7QWG59lw_zhQl7xtqjOkRpmExwbkI6byC0q82Bet2sq_vvwDGurjpYUaKVulb61mrkVMh0p0vAPryyrg9oN8C3GpKi");

            var payload = new Payload
            {
                notification = new Notification
                {
                    title = "I DEMAND YOUR ATTENTION :)" + text,
                    subtitle = "Just kidding, but not really",
                    text = "Sorry to bother you I meant, please pick an option below..",
                    clickAction = "GENERAL",
                    badge = "1",
                    sound = "default"
                },
                contentAvailable = true,
                data = new Data
                {
                    foo = "bar"
                },
                priority = "High",
                to = to
            };//add to from txt

            string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);


            // Send the request with headers and body
            var response = await _requestSender.SendRequest("https://fcm.googleapis.com/fcm/send", "POST", jsonString, customHeaders);


            var content = await response.Content.ReadAsStringAsync();   

            return response;
        }

        private async Task<bool> SaveTockenToTxt(string token)
        {
            List<string> tokenList;
            using (StreamReader sr = new StreamReader(_messageTokenListPath))
            {
                var content = await sr.ReadToEndAsync();
                if (string.IsNullOrEmpty(content))
                {
                    tokenList = new List<string>();
                }
                else
                {
                    tokenList = JsonConvert.DeserializeObject<List<string>>(content);
                }
            }
            token = token.Replace("\n", " ").Replace("\t", " ");
            token = token.Trim();

            if (tokenList.Contains(token))
            {
                return false;
            }

            using (StreamWriter sw = new StreamWriter(_messageTokenListPath))
            {
                
                tokenList.Add(token);
                var json = JsonConvert.SerializeObject(tokenList);

                await sw.WriteAsync(json);
                return true;
            }
        }

    }

    
}
