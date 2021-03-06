# matrix-emoji-tracker

Graphs the most commonly used emoji from the perspective of a Matrix homeserver. Largely inspired 
by the idea of https://github.com/mroth/emojitracker, and associated blog posts (thanks mroth!) - 
this project is a ground-up implementation of the idea, designed more for Matrix.

Similar to the original emojitracker, this is designed to deal with large volumes of traffic and the 
unique challenges of Matrix. Due to the federated nature of Matrix, this cannot possibly represent
the whole Matrix universe but can represent what one homeserver sees.

This is intended to be run against a high-traffic homeserver and is not built for smaller environments.
Visit [#emojitracker:t2bot.io](https://matrix.to/#/#emojitracker:t2bot.io) on Matrix for discussion and
support.

<!--
t2bot.io's perspective of emoji can be seen at https://emoji.t2bot.io
-->

## System components

`Matrix.EmojiTracker.Filter` is a connector for Synapse to hook into the replication stream and process
events, filtering them down into the central queue. Although Matrix (and Synapse) support application
services (overpowered bots which get their own stream), Synapse currently doesn't operate well when
the application service expects to receive a ton of events. Instead, the project hooks into the
replication stream to get an unfiltered list of things the homeserver sees for processing.

`Matrix.EmojiTracker.Persist` takes data off the central queue and persists it to the database. It 
additionally periodically updates the cached values with an updated count, pushing the update to the 
other clients.

`Matrix.EmojiTracker.WebWorker` handles web client connections and aggregates updates for streaming
over web sockets. In high-load environments, it is recommended to have multiple of these to spread 
out the requests. This also serves the frontend and provides the simple API.

## Running / Building

TODO
