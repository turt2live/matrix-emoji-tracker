using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Matrix.EmojiTracker.Common.Emoji
{
    public static class EmojiData
    {
        private static IEnumerable<string> _knownSymbols;

        public static IEnumerable<string> KnownSymbols
        {
            get
            {
                if (_knownSymbols == null) GenerateKnownSymbols();
                return _knownSymbols;
            }
        }

        public static void GenerateKnownSymbols()
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string rawJson = client.DownloadString("https://unpkg.com/emoji.json@latest/emoji-compact.json");

            List<string> parsed = JsonConvert.DeserializeObject<List<string>>(rawJson);
            _knownSymbols = parsed;
        }
    }
}
