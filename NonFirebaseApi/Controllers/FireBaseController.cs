using Microsoft.AspNetCore.Mvc;
using NonFirebaseApi.Clients;
using NonFirebaseApi.Models;
using System.Text.Json;
using System.IO;


namespace NonFirebaseApi.Controllers
{
    public class FireBaseController : ControllerBase
    {

        private readonly string _messageTokenListPath;

        private IHttpRequestSender _requestSender;

        public FireBaseController(IHttpRequestSender httpRequestSender)
        {
            _requestSender = httpRequestSender;
            _messageTokenListPath = "D:\\Code\\C#\\ynik\\NonFirebaseApi\\NonFirebaseApi\\MessageTokenList.txt";
        }


        [HttpPost("set-message-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequest([FromBody] MessageToken body)
        {
            if (body != null)
            {
                try
                {
                    SaveTockenToTxt(body.Token);
                    return Ok("Token has been added to List");
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error while adding to list");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Token is missing in body");
            }
        }

        private async Task SaveTockenToTxt(string token)
        {
            using (StreamWriter sw = new StreamWriter(_messageTokenListPath, true))
            {
                token = token.Replace("\n", " ").Replace("\t", " ");
                token = token.Trim();
                await sw.WriteLineAsync(token);
            }
        }

    }

    
}
