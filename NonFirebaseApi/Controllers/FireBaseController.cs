using Microsoft.AspNetCore.Mvc;
using NonFirebaseApi.Clients;
using NonFirebaseApi.Models;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NonFirebaseApi.Controllers
{
    public class FireBaseController : ControllerBase
    {

        private  string _messageTokenListPath;
        private  string _allMessagesListPath;

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
        [HttpPost("set-messageTokenListPath")]
        public async Task<IActionResult> MessageTokenListPath([FromBody] string body)
        {
            _messageTokenListPath = body;
            return Ok($"_messageTokenListPath = {body}");
        }
        [HttpPost("set-allMessagesListPath")]
        public async Task<IActionResult> AllMessagesListPath([FromBody] string body)
        {
            _allMessagesListPath = body;
            return Ok($"_allMessagesListPath = {body}");
        }

        [HttpGet("all-messages")]
        public async Task<IActionResult> GetAllMessages()
        {
            using (StreamReader sr = new StreamReader(_allMessagesListPath))
            {
                var content = await sr.ReadToEndAsync();
                return Ok(content);
            }
        }


        [HttpPost("set-message-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetMessageToken([FromBody] MessageToken body)
        {
            if (body.Token != null)
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
                    List<string> tokenList = await GetTokenList();

                    if (tokenList == new List<string> { })
                    {
                        return StatusCode(500, "No tokens in List");
                    }

                    List<string> errorList = new List<string> { };

                    foreach (var token in tokenList)
                    {
                        var response = await SendMessageToSever(message.Text, token);


                        var responseContent = await response.Content.ReadAsStringAsync();

                        dynamic jsonObject = JsonConvert.DeserializeObject(responseContent);
                        int failureValue = jsonObject.failure;
                        if (failureValue == 1)
                        {
                            errorList.Add(token);
                        }
                    }
                    if (errorList.Count > 0)
                    {
                        foreach (var wrongTocken in errorList)
                        {
                            tokenList.Remove(wrongTocken);
                        }
                        using (StreamWriter sw = new StreamWriter(_messageTokenListPath))
                        {
                            var json = JsonConvert.SerializeObject(tokenList);
                            await sw.WriteAsync(json);
                        }
                    }

                    _ = await SaveMessageToTxt(message.Text);
                    return StatusCode(200, "Messages sended");


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


        

        private async Task<List<string>> GetTokenList()
        {
            using (StreamReader sr = new StreamReader(_messageTokenListPath))
            {

                var content = await sr.ReadToEndAsync();
                if (string.IsNullOrEmpty(content))
                {
                    return new List<string> { };
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<string>>(content);
                }
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
                    title = "Увага!",
                    subtitle = "Just kidding, but not really",
                    text = "Sorry to bother you I meant, please pick an option below..",
                    clickAction = "GENERAL",
                    badge = "1",
                    sound = "default",
                    body = text
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

        private async Task<bool> SaveMessageToTxt(string message)
        {
            List<string> messageList;
            using (StreamReader sr = new StreamReader(_allMessagesListPath))
            {
                var content = await sr.ReadToEndAsync();
                if (string.IsNullOrEmpty(content))
                {
                    messageList = new List<string>();
                }
                else
                {
                    messageList = JsonConvert.DeserializeObject<List<string>>(content);
                }
            }
            message = message.Replace("\n", " ").Replace("\t", " ");
            message = message.Trim();

            using (StreamWriter sw = new StreamWriter(_allMessagesListPath))
            {
                messageList.Add(message);
                var json = JsonConvert.SerializeObject(messageList);

                await sw.WriteAsync(json);
                return true;
            }
        }

    }

    
}
