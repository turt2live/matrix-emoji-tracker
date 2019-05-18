using Newtonsoft.Json;

namespace Matrix.EmojiTracker.Common.PubSub
{
    public class IncrementCommand
    {
        public string Emoji { get; set; }
        public int Amount { get; set; }
        public string SourceType { get; set; }

        public static string Make(string emoji, int amount, string sourceType)
        {
            return JsonConvert.SerializeObject(new IncrementCommand
            {
                Emoji = emoji,
                Amount = amount,
                SourceType = sourceType,
            });
        }

        public static IncrementCommand Parse(string json)
        {
            return JsonConvert.DeserializeObject<IncrementCommand>(json);
        }
    }
}
