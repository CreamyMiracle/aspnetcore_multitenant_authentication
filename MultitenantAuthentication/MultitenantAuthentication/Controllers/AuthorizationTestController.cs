using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultitenantAuthentication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationTestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "You are authorized";
        }
    }
}