using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.CorruptImageRepair
{
    [Route("/ImageRepair", "POST", Summary = "Handle websocket test commands")]
    public class ImageRepairRequest
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
    }

    public class ImageRepairController : IService
    {
        private readonly ISessionManager _sessionManager;

        public ImageRepairController(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public async Task<object> Post(ImageRepairRequest request)
        {
            if (string.Equals(request.Name, "ping", StringComparison.OrdinalIgnoreCase))
            {
                await _sessionManager.SendMessageToUserDeviceSessions(
                    request.DeviceId,
                    "imagecorruptrepair",
                    "PONG from server",
                    CancellationToken.None
                );
            }
            else if (string.Equals(request.Name, "startscan", StringComparison.OrdinalIgnoreCase))
            {
                await _sessionManager.SendMessageToUserDeviceSessions(
                    request.DeviceId,
                    "imagecorruptrepair",
                    "Scan requested (stub)",
                    CancellationToken.None
                );
            }

            return "ok";
        }
    }
}
