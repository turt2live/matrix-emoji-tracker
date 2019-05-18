# matrix-emoji-tracker

Graphs the most commonly used emoji from the perspective of a Matrix homeserver. Largely inspired 
by https://github.com/mroth/emojitracker (thanks mroth!)

Similar to the original emojitracker, this is designed to deal with large volumes of traffic and the 
unique challenges of Matrix. Due to the federated nature of Matrix, this cannot possibly represent
the whole Matrix universe but can represent what one homeserver sees.

This is intended to be run against a high-traffic homeserver and is not built for smaller environments.
Visit [#emojitracker:t2bot.io](https://matrix.to/#/#emojitracker:t2bot.io) on Matrix for discussion and
support.

t2bot.io's perspective of emoji can be seen at https://emoji.t2bot.io

## System components

`Matrix.EmojiTracker.Filter` is a connector for Synapse to hook into the replication stream and process
events, filtering them down into the central queue. Although Matrix (and Synapse) support application
services (overpowered bots which get their own stream), Synapse currently doesn't operate well when
the application service expects to receive a ton of events. Instead, the project hooks into the
replication stream to get an unfiltered list of things the homeserver sees for processing.

## Running / Building

TODO
