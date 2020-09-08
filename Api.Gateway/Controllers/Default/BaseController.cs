using Messages.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Api.Gateway.Controllers.Default
{
    public abstract class BaseController : ControllerBase
    {
        protected async Task<IActionResult> WithResponseAsync<TResponseMessage>(Func<Task<Response<TResponseMessage>>> func)
        {
            var response = await func.Invoke();
            return StatusCode((int)response.StatusCode, response);
        }

        protected async Task<IActionResult> WithResponseAsync(Func<Task<Response>> func)
        {
            var response = await func.Invoke();
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
