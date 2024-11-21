using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchmakingController : ControllerBase
    {
        private readonly ILogger<MatchmakingController> _logger;

        public MatchmakingController(ILogger<MatchmakingController> logger)
        {
            _logger = logger;
        }


    }
}
