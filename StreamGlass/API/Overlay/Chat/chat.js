class ChatModule
{
	#textColor;
	#size;
	#margin;
	#socket;
	#parameters;

	constructor() {
		this.#parameters = new URLSearchParams(window.location.search);
		this.#socket = new WebSocket('ws://' + location.host + '/overlay/chat/');
		this.#socket.onopen = this.#Init.bind(this);
		this.#socket.onmessage = this.#OnMessage.bind(this);
	}

	#SendMessage(type, payload) {
        var message = {
            'type': type,
            'payload': payload
        };
        this.#socket.send(JSON.stringify(message));
	}

	#GetParameterOr(name, value) {
		if (this.#parameters.has(name))
			return this.#parameters.get(name);
		return value;
	}

	#CreateBadges(data) {
		var chat_message_badges = document.createElement('span');
		chat_message_badges.className = 'chat_message_badges';
		if (data.hasOwnProperty('badges')) {
			var badges = data['badges'];
			for(var i = 0; i < badges.length; i++) {
				var url = badges[i];
				if (url.startsWith('url:'))
                    url = url.substring(4);
				var chat_message_badge_div = document.createElement('div');
				chat_message_badge_div.className = 'chat_message_badge_div';
				var chat_message_badge = document.createElement('img');
				chat_message_badge.className = 'chat_message_badge';
				chat_message_badge.src = url;
				chat_message_badge.style.width = this.#size;
				chat_message_badge.style.height = this.#size;
				chat_message_badge.style.marginLeft = this.#margin;
				chat_message_badge_div.appendChild(chat_message_badge);
				chat_message_badges.appendChild(chat_message_badge_div);
			}
		}
		return chat_message_badges;
	}

	#CreateChatMessageAuthor(data, user) {
		var chat_message_author = document.createElement('div');
		chat_message_author.className = 'chat_message_author';
		var chat_message_badges = this.#CreateBadges(data);
		var chat_message_author_name = document.createElement('span');
		chat_message_author_name.className = 'chat_message_author_name';
		if (user.hasOwnProperty('color')) {
			var color = user['color'];
			if (color.hasOwnProperty('r') && color.hasOwnProperty('g') && color.hasOwnProperty('b') && color.hasOwnProperty('a'))
				chat_message_author_name.style.color = 'rgba(' + color['r'] + ',' + color['g'] + ',' + color['b'] + ',' + color['a'] + ')';
		}
		chat_message_author_name.style.fontSize = this.#size;
		console.log(user);
		chat_message_author_name.textContent = user['name'];
		if (user.hasOwnProperty('id'))
			chat_message_author_name.id = user['id'];
		chat_message_author.appendChild(chat_message_badges);
		chat_message_author.appendChild(chat_message_author_name);
		return chat_message_author;
	}

	#CreateTextSparator() {
		var chat_message_separator = document.createElement('span');
		chat_message_separator.className = 'chat_message_separator';
		chat_message_separator.textContent = ': ';
		chat_message_separator.style.color = this.#textColor;
		chat_message_separator.style.fontSize = this.#size;
		chat_message_separator.style.marginRight = this.#margin;
		return chat_message_separator;
	}

	#CreateEmote(content) {
		if (content.startsWith('url:'))
			content = content.substring(4);
		var chat_message_emote_div = document.createElement('div');
		chat_message_emote_div.className = 'chat_message_emote_div';
		var chat_message_emote = document.createElement('img');
		chat_message_emote.className = 'chat_message_emote';
		chat_message_emote.src = content;
		chat_message_emote.style.width = this.#size;
		chat_message_emote.style.height = this.#size;
		chat_message_emote.style.marginRight = this.#margin;
		chat_message_emote_div.appendChild(chat_message_emote);
		return chat_message_emote_div;
	}

	#CreateMessageText(content) {
		var chat_message_text = document.createElement('span');
		chat_message_text.className = 'chat_message_text';
		chat_message_text.textContent = content;
		chat_message_text.style.color = this.#textColor;
		chat_message_text.style.fontSize = this.#size;
		chat_message_text.style.marginRight = this.#margin;
		return chat_message_text;
	}

	#CreateMessageContent(data) {
		var chat_message_content = document.createElement('span');
		chat_message_content.className = 'chat_message_content';
		var chat_message_content_body = document.createElement('span');
		chat_message_content_body.className = 'chat_message_content_body';
		var message = data['message'];
		if (message.hasOwnProperty('sections')) {
			var sections = message['sections'];
			for(var i = 0; i < sections.length; i++) {
				var section = sections[i];
				if (section.hasOwnProperty('content') && section.hasOwnProperty('type')) {
					var content = section['content'];
					var type = section['type'];
					if (type === 0) //Text
						chat_message_content_body.appendChild(this.#CreateMessageText(content));
					else if (type === 1 || type === 2) //1 = Image, 2 = Animated Image
						chat_message_content_body.appendChild(this.#CreateEmote(content));
				}
			}
		}
		if (chat_message_content_body.hasChildNodes())
			chat_message_content.appendChild(chat_message_content_body);
		return chat_message_content;
	}

	#OnChatMessage(data) {
		if (data.hasOwnProperty('message') && data.hasOwnProperty('user')) {
			var user = data['user'];
			if (user.hasOwnProperty('name')) {
				var chat_message = document.createElement('div');
				chat_message.className = 'chat_message';
				if (data.hasOwnProperty('id'))
					chat_message.id = data['id'];

				var chat_message_author = this.#CreateChatMessageAuthor(data, user);
				var chat_message_separator = this.#CreateTextSparator();
				var chat_message_content = this.#CreateMessageContent(data);

				if (chat_message_content.hasChildNodes()) {
					chat_message.appendChild(chat_message_author);
					chat_message.appendChild(chat_message_separator);
					chat_message.appendChild(chat_message_content);
					document.getElementById('chat').appendChild(chat_message);
				}
			}
		}
	}

	#OnLoadPage(data) {
		if (data.hasOwnProperty('messages')) {
			var messages = data['messages'];
			for (var i = 0; i < messages.length; i++)
				this.#OnChatMessage(messages[i]);
		}
		if (data.hasOwnProperty('page')) {
			var payload = { 'page': data['page'] };
			this.#SendMessage('page', payload);
		}
		else {
			document.getElementById("full_chat").style.display = 'block';
		}
	}

	#OnChatMessagesDelete(data) {
        var ids = data['ids'];
		for (var i = 0; i < ids.length; i++)
			document.getElementById(ids[i]).remove();
	}

	#Init() {
		this.#textColor = this.#GetParameterOr('color', '#ffffff');
		this.#size = this.#GetParameterOr('size', '22');
		this.#margin = this.#GetParameterOr('margin', '5');
		var chatDiv = document.getElementById('chat');
		if (this.#parameters.has("to_bottom")) {
			if (this.#parameters.has("reverse")) {
				chatDiv.style.top = 0;
				chatDiv.style.flexDirection = 'column-reverse';
			}
			else {
				//TODO
			}
		}
		else {
			if (this.#parameters.has("reverse")) {
				//TODO
			}
			else {
				chatDiv.style.bottom = 0;
				chatDiv.style.flexDirection = 'column';
			}
		}
		document.getElementById("full_chat").style.display = 'none';
		var payload = {};
		if (this.#parameters.has("limit"))
			payload = { 'limit': this.#parameters.get("limit") };
		this.#SendMessage('page', payload);
	}

	#OnMessage(event) {
		var eventJson = JSON.parse(event.data);
		if (eventJson.hasOwnProperty('type') && eventJson.hasOwnProperty('payload')) {
			var type = eventJson['type'];
			var payload = eventJson['payload'];
			switch (type) {
				case 'message':
					{
						if (payload.hasOwnProperty('message'))
							this.#OnChatMessage(payload['message']);
						break;
					}
				case 'delete':
					{
						this.#OnChatMessagesDelete(payload);
						break;
					}
				case 'page':
					{
						this.#OnLoadPage(payload);
						break;
					}
			}
		}
	}
}

var streamGlassModuleClient;

function OnLoad()
{
	streamGlassModuleClient = new ChatModule();
}