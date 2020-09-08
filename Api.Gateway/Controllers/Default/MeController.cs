using Microsoft.AspNetCore.Mvc;

namespace Api.Gateway.Controllers.Default
{
    [ApiController, Route("api/[controller]")]
    public class MeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { name = "Api Gateway", version = "1.0" });
    }
}
