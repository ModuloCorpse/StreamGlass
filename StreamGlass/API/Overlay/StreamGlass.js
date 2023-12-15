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

class StreamGlassOverlayExtension {
	#eventDictionary = {};
	#socket;
	#parameters;
	#isHoldingEvent = false;
	#heldEvents = [];

	Init() { }

	constructor(path) {
		this.#parameters = new URLSearchParams(window.location.search);
		if (path[0] != '/')
			path = '/' + path;
		this.#socket = new WebSocket('ws://' + location.host + '/overlay' + path);
		this.#socket.onopen = this.#OnOpen.bind(this);
		this.#socket.onmessage = this.#OnMessage.bind(this);
	}

	#OnOpen(event) {
		this.Init();
	}

	#HandleUnsubscribed(data) {
		if (data.hasOwnProperty('event')) {
			var event = data['event'];
			this.#eventDictionary.delete(event);
		}
	}

	#HandleEventData(data) {
		if (data.hasOwnProperty('event') && data.hasOwnProperty('data')) {
			var event = data['event'];
			var eventData = data['data'];
			if (this.#eventDictionary.hasOwnProperty(event))
				this.#eventDictionary[event](eventData);
		}
	}

	#HandleEvent(data) {
		if (this.#isHoldingEvent)
			this.#heldEvents.push(data);
		else
			this.#HandleEventData(data);
	}

	#OnMessage(event) {
		var eventJson = JSON.parse(event.data);
		if (eventJson.hasOwnProperty('type') && eventJson.hasOwnProperty('data')) {
			var type = eventJson['type'];
			var data = eventJson['data'];
			switch (type) {
				case 'error':
					{
						this.#HandleUnsubscribed(data);
						break;
					}
				case 'unsubscribed':
					{
						this.#HandleUnsubscribed(data);
						break;
					}
				case 'event':
					{
						this.#HandleEvent(data);
						break;
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

	SubscribeToCustomEvent(eventType, handler) {
		this.#eventDictionary[eventType] = handler;
		this.#Send(JSON.stringify({ 'request': 'custom-subscribe', 'event': eventType }));
	}

	UnregisterToCustomEvent(eventType) {
		this.#Send(JSON.stringify({ 'request': 'custom-unsubscribe', 'event': eventType }));
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

	HoldEvents() { this.#isHoldingEvent = true; }

	UnholdEvents() {
		while (this.#heldEvents.length != 0)
			this.#HandleEventData(this.#heldEvents.shift());
		this.#isHoldingEvent = false;
	}
}