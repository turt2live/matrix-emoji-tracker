using System;
using System.Linq;
using Matrix.EmojiTracker.Common;
using Matrix.EmojiTracker.Database;
using Matrix.EmojiTracker.Filter.EventParser;
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

            // TODO: Read stream position from database somewhere
            string eventStreamPosition = StreamPosition.LATEST;
            StartReplicationAsync(eventStreamPosition);

            Console.ReadKey(true);
        }

        private static async void StartReplicationAsync(string eventStreamPosition)
        {
            var replication = new SynapseReplication();
            replication.ClientName = "EmojiTracker_FilterProc";
            replication.ServerName += Replication_ServerName;

            var synapseConfig = _config.GetSection("Synapse");

            await replication.Connect(synapseConfig.GetValue<string>("replicationHost"),
                synapseConfig.GetValue<int>("replicationPort"));

            var stream = replication.ResumeStream<EventStreamRow>(eventStreamPosition);
            stream.DataRow += Stream_DataRow;
        }

        private static void Stream_DataRow(object sender, EventStreamRow e)
        {
            if (e.Kind != EventStreamRow.RowKind.Event) return; // We don't parse state events
            if (!EmojiEventParser.CanParse(e.EventType)) return;

            using (var db = new SynapseDbContext(_config.GetConnectionString("synapse")))
            {
                var ev = db.EventsJson.SingleOrDefault(e2 => e2.RoomId == e.RoomId && e2.EventId == e.EventId);
                if (ev == null) return;

                var emoji = EmojiEventParser.CountEmoji(e.EventType, ev);
                log.Information("Found {0} emoji in {1}", emoji.Sum(p => p.Value), e.EventId);
            }
        }

        private static void Replication_ServerName(object sender, string e)
        {
            log.Information("Server name: " + e);
        }
    }
}
