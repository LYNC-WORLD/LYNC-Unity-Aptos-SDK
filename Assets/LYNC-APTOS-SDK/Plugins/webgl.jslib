mergeInto(LibraryManager.library, {
  WebGLLogin: function (url, websocketUrl, gameObjectName) {
    var _url = UTF8ToString(url);
    var _websocketUrl = UTF8ToString(websocketUrl);
    var _gameObjectName = UTF8ToString(gameObjectName);
    window.UnityWebSocket = new WebSocket(_websocketUrl);

    window.UnityWebSocket.onopen = function () {
      console.log("WebSocket connected.");
    };

    window.UnityWebSocket.onmessage = function (event) {
      var message = JSON.parse(event.data);
      console.log("received message = " + JSON.stringify(message));

      if (message.type == "id") {
        var windowName = "LYNC - Auth";
        var windowFeatures =
          "width=600,height=400,top=200,left=200,noopener=false,noreferrer=false,rel=opener";
        window.UnityWebSocket.id = message.id;
        window.open(
          _url + "&webGLOrigin=" + message.id,
          windowName,
          windowFeatures
        );
      }

      if (message.type == "broadcast") {
        SendMessage(_gameObjectName, "HandleWebGLMessage", message.data);
      }
    };

    window.UnityWebSocket.onclose = function () {
      console.log("WebSocket closed.");
    };

    window.UnityWebSocket.onerror = function () {
      console.log("WebSocket error occurred.");
    };
  },
});
