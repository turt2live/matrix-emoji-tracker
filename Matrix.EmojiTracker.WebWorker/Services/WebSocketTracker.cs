using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Matrix.EmojiTracker.WebWorker.Services
{
    public static class WebSocketTracker
    {
        private static readonly List<Tuple<WebSocket, CancellationToken>> Sockets = new List<Tuple<WebSocket, CancellationToken>>();

        public static Task RegisterSocket(HttpContext context, WebSocket socket)
        {
            var token = context.RequestAborted;
            Sockets.Add(new Tuple<WebSocket, CancellationToken>(socket, token));
            return Task.Delay(-1, token);
        }

        public static async void BroadcastEmojiChange(string emoji, int delta)
        {
            var message = $"{emoji}|{delta}";
            var buf = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));

            var toRemove = new List<Tuple<WebSocket, CancellationToken>>();
            foreach (var pair in Sockets)
            {
                var socket = pair.Item1;

                if (socket.State != WebSocketState.Open || pair.Item2.IsCancellationRequested)
                {
                    toRemove.Add(pair);
                    continue;
                }

                try
                {
                    await socket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    if (e is WebSocketException) toRemove.Add(pair);
                }
            }

            foreach (var pair in toRemove)
            {
                Sockets.Remove(pair);
            }
        }
    }
}
