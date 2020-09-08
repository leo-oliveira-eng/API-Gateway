using Api.Gateway.Controllers.Default;
using Make.Magic.Challenge.Connector.Services.Contracts;
using Make.Magic.Challenge.Messages.RequestMessages;
using Make.Magic.Challenge.Messages.ResponseMessages;
using Messages.Core;
using Messages.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Gateway.Controllers.HarryPotter
{
    [ApiController, Route("api/[controller]")]
    public class CharacterController : BaseController
    {
        const string httpClientConfigurationName = "HarryPotterClientConfigurationName";

        IMakeMagicServiceConnector MakeMagicServiceConnector { get; }

        IDistributedCache DistributedCache { get; }

        public CharacterController(IMakeMagicServiceConnector makeMagicServiceConnector, IDistributedCache distributedCache)
        {
            MakeMagicServiceConnector = makeMagicServiceConnector ?? throw new ArgumentNullException(nameof(makeMagicServiceConnector));
            DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> CreateCharacterAsync([FromBody] CharacterRequestMessage requestMessage) 
            => await WithResponseAsync(() => MakeMagicServiceConnector.CreateCharacterAsync(requestMessage, httpClientConfigurationName));

        [HttpGet, Route("{code}")]
        public async Task<IActionResult> GetCharacterAsync(Guid code)
        {
            var key = $"{code}-character";

            var cache = await DistributedCache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cache))
                return Ok(JsonConvert.DeserializeObject<Response<CharacterResponseMessage>>(cache));

            var response = await MakeMagicServiceConnector.GetCharacterAsync(code, httpClientConfigurationName);

            if (!response.HasError)
            {
                await DistributedCache
                .SetStringAsync(key, JsonConvert.SerializeObject(response),
                    new DistributedCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromDays(2),
                        AbsoluteExpiration = DateTime.UtcNow.AddDays(4)
                    });

                return Ok(response);
            }

            if (response.Messages.Any(m => m.Type == MessageType.BusinessError))
                return BadRequest(response);

            return StatusCode(500, response);
        }

        [HttpGet, Route("")]
        public async Task<IActionResult> GetCharacterAsync([FromQuery] GetCharactersRequestMessage requestMessage)
            => await WithResponseAsync(() => MakeMagicServiceConnector.GetCharacterAsync(requestMessage, httpClientConfigurationName));

        [HttpPut, Route("{code}")]
        public async Task<IActionResult> UpdateCharacterAsync(Guid code, [FromBody] CharacterRequestMessage requestMessage)
            => await WithResponseAsync(() => MakeMagicServiceConnector.UpdateCharacterAsync(code, requestMessage, httpClientConfigurationName));

        [HttpDelete, Route("{code}")]
        public async Task<IActionResult> DeleteCharacterAsync(Guid code)
            => await WithResponseAsync(() => MakeMagicServiceConnector.DeleteCharacterAsync(code,httpClientConfigurationName));
    }
}
