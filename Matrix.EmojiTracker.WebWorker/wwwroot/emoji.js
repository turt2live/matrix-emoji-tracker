(function () {
    var socket = new WebSocket("ws://" + window.location.host + "/api/v1/values");
    socket.addEventListener('message',
        (event) => {
            console.log(event.data);
        });
})();