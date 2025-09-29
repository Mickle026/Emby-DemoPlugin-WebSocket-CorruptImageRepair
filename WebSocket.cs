using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Emby.CorruptImageRepair.ImageDetection;

namespace Emby.WebSocket
{
    // Emby auto-discovers this because it implements IWebSocketListener
    public class WebSocketHandler : IWebSocketListener
    {
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;
        private readonly ILibraryManager _library;

        public string Name => "WebSocket";
        private bool stop = false;
        public WebSocketHandler(IJsonSerializer json, ILogger logger, ILibraryManager library)
        {
            _json = json;
            _logger = logger;
            _library = library;
        }

        public async Task ProcessMessage(WebSocketMessageInfo message)
        {
            _logger.Info("[WebSocket] Incoming raw: {0}", message.Data);

            JsonMessage msg;
            try
            {
                msg = _json.DeserializeFromString<JsonMessage>(message.Data);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("[WebSocket] Failed to parse JSON", ex);
                return;
            }

            switch (msg?.Type)
            {
                case null:
                    _logger.Warn("[WebSocket] Message type is null");
                    return;
                case "ping":
                    await Reply(message, "pong.",msg);
                    return;
                case "startscan":
                    await startscan(message, msg);
                    return;
                case "stopscan":
                    stop = true;
                    return;
                default:
                    _logger.Warn("[WebSocket] Unknown message type: {0}", msg.Type);
                    return;
            }
            
           

        }
        async void startscan_simulation(WebSocketMessageInfo message, JsonMessage msg)
        {

            for (int i = 1; i <= 5; i++)
            {
                await Reply(message, $"Progress: {i}", msg);

                await Task.Delay(1000); // simulate time passing
            }
            await Reply(message, $"Scan Complete!" , msg, "Dummy scan finished");
        }

        async Task startscan(WebSocketMessageInfo message, JsonMessage msg)
        {
            stop = false;
            var items = _library.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Movie", "Series", "Episode", "MusicAlbum", "MusicArtist", "Book", "MusicVideo", "BoxSet" },
                Recursive = true
            });

            int total = items.Count();
            int index = 0;
            
            foreach (var item in items)
            {
                while (stop)
                {
                    await Reply(message,
                        "Scan Stopped!",
                        msg,
                        "Image corruption scan stopped by user.");
                    return;
                }

                index++;

                foreach (var img in item.ImageInfos)
                {
                    bool fileExists = System.IO.File.Exists(img.Path);
                    if (!fileExists)
                    {
                        await Reply(message,
                            $"Progress {index}/{total}",
                            msg,
                            $"{item.Name} → {img.Type}: NO FILE");
                        continue;
                    }
                    else
                    {
                        bool isValid = ValidImage(img.Path);   // true = good, false = bad
                        string status = isValid ? "OK" : "CORRUPT";

                        await Reply(message,
                            $"Progress {index}/{total}",
                            msg,
                            $"{item.Name} → {img.Type}: {status}");
                    }

                }
            }

            await Reply(message,
                "Scan Complete!",
                msg,
                "Non-destructive image corruption scan finished.");
        }




        async Task Reply(WebSocketMessageInfo message, string txtreply, JsonMessage msg,string Payload="")
        {
            _logger.Info("[WebSocket] Got PING from {0}", msg.UserId);

            if (string.IsNullOrEmpty(Payload))
                Payload = "Server time: " + DateTime.UtcNow.ToString("HH:mm:ss");

            var response = new JsonMessage
            {
                Type = txtreply,
                UserId = msg.UserId,
                Payload = Payload
            };

            var json = _json.SerializeToString(response);

            var reply = new WebSocketMessage<string>
            {
                MessageType = Name,
                Data = json
            };

            await message.Connection.SendAsync(reply, CancellationToken.None);
            _logger.Info("[WebSocket] Sent {0} back to client {1}", txtreply, message.Connection.Id);
        }

    }
}
