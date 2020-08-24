using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dapr.Demo.PS
{
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly DaprClient _client;

        public MessageController(ILogger<MessageController> logger, DaprClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet]
        [Route("api/test")]
        public async Task<IActionResult> Test()
        {
            return Ok("Hello world");
        }


        [HttpPost]
        [Route("api/message")]
        public async Task<IActionResult> ReceiveMessage([FromBody] Message message)
        {
            _logger.LogInformation($"Message with id {message.Id.ToString()} received!");

            //Validate message received
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.PostAsync(
                   "http://localhost:3500/v1.0/publish/messagetopic",
                   new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
                   );

                _logger.LogInformation($"Message with id {message.Id.ToString()} published with status {result.StatusCode}!");
            }

            return Ok();
        }

        [HttpPost]
        [Route("api/messagecontext")]
        public async Task<IActionResult> ReceiveMessageContext([FromBody] Message message)
        {
            _logger.LogInformation($"Message with id {message.Id.ToString()} received!");

            //await _client.SaveStateAsync<Message>("messagetopic", message.Id.ToString(), message);
            await _client.PublishEventAsync<Message>("messagetopic", message);

            var message2 = _client.GetStateAsync<Message>("messagetopic", "53E1D29A-7556-40AE-9877-E2251BCBAA5E");

            _logger.LogInformation($"Message with id {message.Id.ToString()} saved!");

            return Ok();
        }

        [Topic("messagetopic")]
        [HttpPost]
        [Route("messagetopic")]
        public async Task<IActionResult> ProcessOrder([FromBody] Message message)
        {
            //Process message placeholder
            _logger.LogInformation($"Message with id {message.Id.ToString()} processed!");
            return Ok();
        }
    }
}
