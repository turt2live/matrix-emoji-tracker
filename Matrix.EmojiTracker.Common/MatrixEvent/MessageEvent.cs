using System;
using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common.MatrixEvent
{
    public class MessageEvent : MatrixEvent
    {
        public string MessageType
        {
            get => Content.GetValue<string>("msgtype");
            set => Content.SetValue("msgtype", value);
        }

        public string Body
        {
            get => Content.GetValue<string>("body");
            set => Content.SetValue("body", value);
        }

        public MessageEvent(JObject baseJson) : base(baseJson)
        {
            if (EventType != MatrixEventType.RoomMessage) throw new ArgumentException("Expected a room message event");
        }
    }
}
