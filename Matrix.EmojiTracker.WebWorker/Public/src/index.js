// CSS imports
require("milligram/dist/milligram.css");
require("./index.css");

// Actual app imports
var $ = require("jquery");

var EMOJI_MAP = {};
var ORDERED_EMOJI = [];
var EMOJI_IMG_MAP = {};

$.get("/api/v1/values").then(vals => {
    EMOJI_MAP = vals;
    sortMap();
    renderEmoji();
    startWebsocket();
}).catch(err => {
    console.error(err);
    $(".emoji-container").html("<p class='error'>Failed to load emoji</p>");
});

function sortMap() {
    var values = [];
    for (var symbol of Object.keys(EMOJI_MAP)) {
        values.push({ symbol: symbol, count: EMOJI_MAP[symbol] });
        EMOJI_IMG_MAP[symbol] = twemoji.parse(symbol, { className: 'emoji-img' });
    }
    values.sort((a, b) => b.count - a.count);
    ORDERED_EMOJI = values;
}

function startWebsocket() {
    var socket = new WebSocket("ws://" + window.location.host + "/api/v1/values");
    socket.addEventListener('message', e => handleWsMessage(e));
}

function handleWsMessage(event) {
    var parts = event.data.split("|");
    var symbol = parts[0];
    var count = Number(parts[1]);
    EMOJI_MAP[symbol] += count;
    updateCount(symbol, EMOJI_MAP[symbol]);
    console.log("Updated " + symbol);
}

function renderEmoji() {
    var html = "";
    for (var pair of ORDERED_EMOJI) {
        var symbol = pair.symbol;
        var count = pair.count;
        var encSymbol = safeTagsReplace(symbol);
        html += "<div class='emoji' id='emoji-" + encSymbol + "'>" + renderEmojiElement(symbol, count) + "</div>";
    }
    $(".emoji-container").html(html);
}

function updateCount(symbol, count) {
    var encSymbol = safeTagsReplace(symbol);
    $("#emoji-" + encSymbol).html(renderEmojiElement(symbol, count));
}

function renderEmojiElement(symbol, count) {
    return "<span class='symbol'>" + EMOJI_IMG_MAP[symbol] + "</span><span class='count'>" + count + "</span>";
}

// UTILS

var tagsToReplace = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;'
};

function replaceTag(tag) {
    return tagsToReplace[tag] || tag;
}

function safeTagsReplace(str) {
    return str.replace(/[&<>]/g, replaceTag);
}