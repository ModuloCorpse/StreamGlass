class StreamGlassEventWebsocket {
	#socket;
	#parameters;
	#eventDictionary = {};
	#heldEvents = [];
	#id = '';
	#isHoldingEvent = false;

	Init() { }

	constructor() {
		this.#parameters = new URLSearchParams(window.location.search);
		this.#socket = new WebSocket('ws://' + location.host + '/event');
		this.#socket.onmessage = this.#OnMessage.bind(this);
	}

	#HandleEventData(data) {
		if (data.hasOwnProperty('event') && data.hasOwnProperty('data')) {
			var event = data['event'];
			var eventData = data['data'];
			if (this.#eventDictionary.hasOwnProperty(event))
				this.#eventDictionary[event](eventData);
		}
	}

	#OnMessage(event) {
		var eventJson = JSON.parse(event.data);
		if (eventJson.hasOwnProperty('type') && eventJson.hasOwnProperty('data')) {
			var type = eventJson['type'];
			var data = eventJson['data'];
			switch (type) {
				case 'welcome':
					{
						if (data.hasOwnProperty('id')) {
							this.#id = data['id'];
							this.Init();
						}
					}
				case 'event':
					{
						if (this.#isHoldingEvent)
							this.#heldEvents.push(data);
						else
							this.#HandleEventData(data);
						break;
					}
			}
		}
	}

	Get(path, handler) {
		fetch(path).then((response) => { response.text().then((body) => handler(body)) })
	}

	RegisterToEvent(eventType, handler) {
		fetch('http://' + location.host + '/event/register/' + eventType, { method: "POST", body: this.#id })
			.then((response) => {
				if (response.status === 200)
					this.#eventDictionary[eventType] = handler;
			});
	}

	UnregisterToEvent(eventType) {
		fetch('http://' + location.host + '/event/unregister/' + eventType, { method: "POST", body: this.#id })
			.then((response) => {
				if (response.status === 200)
					this.#eventDictionary.delete(eventType);
			});
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