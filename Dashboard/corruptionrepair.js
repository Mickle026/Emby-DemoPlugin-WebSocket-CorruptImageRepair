define([], function () {
    return function (view) {
        var btn, logBox, progress;

        function log(msg) {
            var ts = new Date().toLocaleTimeString();
            logBox.value += "[" + ts + "] " + msg + "\n";
            logBox.scrollTop = logBox.scrollHeight;
        }

        function onWebSocketMessage(e, msg) {
            if (msg && msg.MessageType === "WebSocket") {
                try {
                    var data = JSON.parse(msg.Data || "{}");

                    if (data.Type === "pong") {
                        log("🏓 Pong from server!");
                    } else if (data.Type && data.Type.includes("Progress")) {
                        const regex = /(\d+)\/(\d+)/;
                        const match = data.Type.match(regex);
                        if (match) {
                            const current = parseInt(match[1], 10);
                            const total = parseInt(match[2], 10);
                            const percentage = (current / total) * 100;
                            console.log("Progress:", current, "/", total, "=", percentage + "%");
                            view.querySelector("#scanProgress").value = percentage || 0;
                        }
                    } else if (data.Type && data.Type.toLowerCase() === "scan complete!") {
                        log("✅ Scan complete!");
                    } else {
                        log("ℹ️ Message from server: " + (data.Type || "unknown"));
                    }

                    log("✅ Response: " + JSON.stringify(data));
                } catch (ex) {
                    log("⚠️ Raw: " + (msg.Data || ""));
                }
            }
        }


        view.addEventListener("viewshow", function () {
            btnPing = view.querySelector("#btnPing");
            btnScan = view.querySelector("#btnScan");
            btnStopScan = view.querySelector("#btnStopScan");
            logBox = view.querySelector("#logBox");

            log("WebSocketTest loaded, ready.");

            // ✅ subscribe to WebSocket channel using Events.on
            Events.on(ApiClient, "message", onWebSocketMessage);

            btnPing.addEventListener("click", function () {
                log("Sending ping → WebSocket…");
                WebSocketReply("ping");
            });
            btnScan.addEventListener("click", function () {
                log("Sending startscan → WebSocket…");
                WebSocketReply("startscan");
            });
            btnStopScan.addEventListener("click", function () {
                log("Sending stopscan → WebSocket…");
                WebSocketReply("stopscan");
            });
            function WebSocketReply(msg) {
                var payload = {
                    Type: msg,
                    UserId: ApiClient.getCurrentUserId() || "",
                    Payload: "Hello from dashboard"
                };

                // must stringify to avoid deserialization error on server
                ApiClient.sendMessage("WebSocket", JSON.stringify(payload));
            }

        });

        view.addEventListener("viewdestroy", function () {
            // ✅ clean up listener when leaving page
            Events.off(ApiClient, "message", onWebSocketMessage);
        });
    };
});
