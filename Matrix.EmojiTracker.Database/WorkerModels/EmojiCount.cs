using System.ComponentModel.DataAnnotations.Schema;
using Matrix.EmojiTracker.Common.MatrixEvent;

namespace Matrix.EmojiTracker.Database.WorkerModels
{
    [Table("emoji_count")]
    public class EmojiCount
    {
        public static readonly string SourceMessage = MatrixEventType.RoomMessage;
        public static readonly string SourceReaction = MatrixEventType.Reaction;
        public static readonly string SourceUnknown = "io.t2bot.emojitracker.unknown";

        [Column("emoji")]
        public string Emoji { get; protected set; }

        [Column("source")]
        public string Source { get; protected set; }

        [Column("count")]
        public long Count { get; set; }

        protected EmojiCount() { } // For EF

        public EmojiCount(string emoji, string source)
        {
            Emoji = emoji;
            Source = source;
            Count = 0;
        }
    }
}
