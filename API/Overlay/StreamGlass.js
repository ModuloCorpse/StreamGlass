function waitForSocketConnection(socket, callback) {
	setTimeout(function () {
		if (socket.readyState === 1) {
			if (callback != null)
				callback(socket);
		}
		else
			waitForSocketConnection(socket, callback);
	}, 5);
}

class StreamGlassModule {
	#eventDictionary = {};
	#socket;
	#parameters;

	Init() { }

	constructor(path, parameterLoadCallback = null) {
		this.#parameters = new URLSearchParams(window.location.search);
		if (path[0] != '/')
			path = '/' + path;
		this.#socket = new WebSocket('ws://' + location.host + '/overlay' + path);
		this.#socket.onopen = (event) => {
			this.Init();
		}
		this.#socket.onmessage = (event) => {
			var eventJson = JSON.parse(event.data);
			if (eventJson.hasOwnProperty('type') && eventJson.hasOwnProperty('data')) {
				var type = eventJson['type'];
				var data = eventJson['data'];
				switch (type) {
					case 'error':
						{
							if (data.hasOwnProperty('event')) {
								var event = data['event'];
								this.#eventDictionary.delete(event);
							}
							break;
						}
					case 'unsubscribed':
						{
							if (data.hasOwnProperty('event')) {
								var event = data['event'];
								this.#eventDictionary.delete(event);
							}
							break;
						}
					case 'event':
						{
							if (data.hasOwnProperty('event') && data.hasOwnProperty('data')) {
								var event = data['event'];
								var eventData = data['data'];
								if (this.#eventDictionary.hasOwnProperty(event))
									this.#eventDictionary[event](eventData);
							}
							break;
						}
				}
			}
		}
	}

	#Send(message) {
		waitForSocketConnection(this.#socket, function (socket) { socket.send(message); });
	}

	Get(path, handler) {
		fetch(path)
			.then((response) => { response.text().then((body) => handler(body)) })
	}

	SubscribeToEvent(eventType, handler) {
		this.#eventDictionary[eventType] = handler;
		this.#Send(JSON.stringify({ 'request': 'subscribe', 'event': eventType }));
	}

	UnregisterToEvent(eventType) {
		this.#Send(JSON.stringify({ 'request': 'unsubscribe', 'event': eventType }));
	}

	HaveParameter(name) {
		return this.#parameters.has(name);
	}

	GetParameter(name) {
		if (this.#parameters.has(name))
			return this.#parameters.get(name);
		return null;
	}

	GetParameterOr(name, value) {
		if (this.#parameters.has(name))
			return this.#parameters.get(name);
		return value;
	}
}