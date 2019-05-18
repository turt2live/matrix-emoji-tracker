using System;
using System.Collections.Generic;
using System.Linq;
using Matrix.EmojiTracker.Common;
using Matrix.EmojiTracker.Common.Emoji;
using Matrix.EmojiTracker.Common.MatrixEvent;
using Matrix.EmojiTracker.Database;
using Matrix.SynapseInterop.Replication;
using Matrix.SynapseInterop.Replication.DataRows;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Matrix.EmojiTracker.Filter
{
    internal class Program
    {
        private static ILogger log;
        private static IConfiguration _config;

        static void Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.default.json", true, true)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Logger.Setup(_config.GetSection("Logging"));
            log = Log.ForContext<Program>();

            StartReplicationAsync();

            Console.ReadKey(true);
        }

        private static async void StartReplicationAsync()
        {
            var replication = new SynapseReplication();
            replication.ClientName = "EmojiTracker_FilterProc";
            replication.ServerName += Replication_ServerName;

            var synapseConfig = _config.GetSection("Synapse");

            await replication.Connect(synapseConfig.GetValue<string>("replicationHost"),
                synapseConfig.GetValue<int>("replicationPort"));

            var stream = replication.BindStream<EventStreamRow>();
            stream.DataRow += Stream_DataRow;
        }

        private static void Stream_DataRow(object sender, EventStreamRow e)
        {
            if (e.Kind != EventStreamRow.RowKind.Event) return; // We don't parse state events
            if (e.EventType != MatrixEventType.RoomMessage && e.EventType != MatrixEventType.Reaction)
                return; // We don't parse events which aren't visible

            using (var db = new SynapseDbContext(_config.GetConnectionString("synapse")))
            {
                var ev = db.EventsJson.SingleOrDefault(e2 => e2.RoomId == e.RoomId && e2.EventId == e.EventId);
                if (ev == null) return;

                IEnumerable<KeyValuePair<string, int>> emoji = new KeyValuePair<string, int>[0];
                if (e.EventType == MatrixEventType.RoomMessage)
                {
                    var mtxEvent = new MessageEvent(ev.Json);
                    if (mtxEvent.MessageType != MessageEventType.Text &&
                        mtxEvent.MessageType != MessageEventType.Emote) return;

                    emoji = mtxEvent.Body.GetEmoji();
                }
                else if (e.EventType == MatrixEventType.Reaction)
                {
                    var mtxEvent = new MatrixEvent(ev.Json);
                    if (!mtxEvent.HasRelationship) return;

                    var relationship = mtxEvent.Relationship;
                    if (!relationship.IsV2Relation ||
                        relationship.RelationType != EventRelationship.Annotation) return;

                    emoji = relationship.AggregationKey.GetEmoji();
                }

                log.Information("Found {0} emoji in {1}", emoji.Sum(p => p.Value), e.EventId);
            }
        }

        private static void Replication_ServerName(object sender, string e)
        {
            log.Information("Server name: " + e);
        }
    }
}
