using System;
using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common.MatrixEvent
{
    public class MessageEvent : MatrixEvent
    {
        public string MessageType
        {
            get => ReadBase<string>("msgtype", Content);
            set => WriteBase("msgtype", value, Content);
        }

        public string Body
        {
            get => ReadBase<string>("body", Content);
            set => WriteBase("body", value, Content);
        }

        public MessageEvent(JObject baseJson) : base(baseJson)
        {
            if (EventType != MatrixEventType.RoomMessage) throw new ArgumentException("Expected a room message event");
        }
    }
}
