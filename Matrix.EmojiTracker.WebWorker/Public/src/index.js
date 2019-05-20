// CSS imports
require("milligram/dist/milligram.css");
require("./index.css");

// Actual app imports
var $ = require("jquery");

var EMOJI_MAP = {};
var ORDERED_EMOJI = [];
var EMOJI_IMG_MAP = {};
var SELECTORS_CACHE = {};

// This is incremented later on in the handler
var msgsIn = 0;
setInterval(() => {
    console.log("Handled " + msgsIn + " messages in ~1000ms");
    msgsIn = 0;
}, 1000);

$.get("/api/v1/values").then(vals => {
    EMOJI_MAP = vals;
    sortMap();
    renderEmoji();
    startWebsocket();
}).catch(err => {
    console.error(err);
    document.getElementById("emoji-container").innerHTML = "<p class='error'>Failed to load emoji</p>";
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
    msgsIn++;
    var parts = event.data.split("|");
    var symbol;
    for (var part of parts) {
        if (!symbol) {
            symbol = part;
            continue;
        }
        
        var count = Number(part);
        EMOJI_MAP[symbol] += count;
        updateCount(symbol, EMOJI_MAP[symbol]);
        symbol = null;
    }
}

function renderEmoji() {
    var html = "";
    for (var pair of ORDERED_EMOJI) {
        var symbol = pair.symbol;
        var count = pair.count;
        html += "<div class='emoji' id='emoji-" + symbol + "'>" + renderEmojiElement(symbol, count) + "</div>";
    }
    document.getElementById("emoji-container").innerHTML = html;
}

function updateCount(symbol, count) {
    if (!SELECTORS_CACHE[symbol]) {
        var parentSelector = document.getElementById("emoji-" + symbol);
        var selector = null;
        var children = parentSelector.childNodes;
        for (var child of children) {
            if (child.className === 'count') {
                selector = child;
                break;
            }
        }
        if (!selector) return;
        SELECTORS_CACHE[symbol] = {
            parent: parentSelector,
            child: selector,
        };
    }

    // Update the count
    SELECTORS_CACHE[symbol].child.textContent = count;

    // Animate
    var parent = SELECTORS_CACHE[symbol].parent;
    var classes = parent.className;
    if (classes.indexOf("animated") === -1) {
        classes = classes + " animated-1";
    }
    if (classes.endsWith("1")) {
        classes = classes.substring(0, classes.length - 1) + "2";
    } else {
        classes = classes.substring(0, classes.length - 1) + "1";
    }
    parent.className = classes;
}

function renderEmojiElement(symbol, count) {
    return "<span class='symbol'>" + EMOJI_IMG_MAP[symbol] + "</span><span class='count'>" + count + "</span>";
}
