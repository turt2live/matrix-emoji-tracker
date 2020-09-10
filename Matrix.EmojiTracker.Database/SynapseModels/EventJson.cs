using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Database.SynapseModels
{
    [Table("event_json")]
    public class EventJson
    {
        [Key]
        [Column("event_id")]
        public string EventId { get; set; }

        [Column("room_id")]
        public string RoomId { get; set; }

        [Column("internal_metadata")]
        public string InternalMetadata { get; set; }

        [Column("json")]
        public string RawJson { get; set; }

        [Column("format_version")]
        public int FormatVersion { get; set; }

        [NotMapped]
        public JObject Json => JObject.Parse(RawJson);
    }
}
