using System.Collections.Generic;
using System.Threading.Tasks;
using Matrix.EmojiTracker.WebWorker.Services;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.EmojiTracker.WebWorker.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/v1/values
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, long>>> Get()
        {
            var context = ControllerContext.HttpContext;
            var isSocket = context.WebSockets.IsWebSocketRequest;

            if (isSocket)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                await WebSocketTracker.RegisterSocket(context, socket);
            }

            return EmojiCache.Map();
        }
    }
}
