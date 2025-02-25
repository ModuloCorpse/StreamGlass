class ChatModule extends StreamGlassEventWebsocket
{
	#textColor;
	#size;
	#margin;

	#CreateBadges(user)
	{
		var chat_message_badges = document.createElement('span');
		chat_message_badges.className = 'chat_message_badges';
		if (user.hasOwnProperty('badges'))
		{
			var badges = user['badges'];
			for(var i = 0; i < badges.length; i++)
			{
				var badge = badges[i];
				if (badge.hasOwnProperty('image_url_1x'))
				{
					var url = badge['image_url_1x'];
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
		}
		return chat_message_badges;
	}

	#CreateChatMessageAuthor(user, color)
	{
		var chat_message_author = document.createElement('div');
		chat_message_author.className = 'chat_message_author';
		var chat_message_badges = this.#CreateBadges(user);
		var chat_message_author_name = document.createElement('span');
		chat_message_author_name.className = 'chat_message_author_name';
		chat_message_author_name.style.color = color;
		chat_message_author_name.style.fontSize = this.#size;
		chat_message_author_name.textContent = user['display_name'];
		if (user.hasOwnProperty('id'))
			chat_message_author_name.id = user['id'];
		chat_message_author.appendChild(chat_message_badges);
		chat_message_author.appendChild(chat_message_author_name);
		return chat_message_author;
	}

	#CreateTextSparator()
	{
		var chat_message_separator = document.createElement('span');
		chat_message_separator.className = 'chat_message_separator';
		chat_message_separator.textContent = ': ';
		chat_message_separator.style.color = this.#textColor;
		chat_message_separator.style.fontSize = this.#size;
		chat_message_separator.style.marginRight = this.#margin;
		return chat_message_separator;
	}

	#CreateEmote(content)
	{
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

	#CreateMessageText(content)
	{
		var chat_message_text = document.createElement('span');
		chat_message_text.className = 'chat_message_text';
		chat_message_text.textContent = content;
		chat_message_text.style.color = this.#textColor;
		chat_message_text.style.fontSize = this.#size;
		chat_message_text.style.marginRight = this.#margin;
		return chat_message_text;
	}

	#CreateMessageContent(data)
	{
		var chat_message_content = document.createElement('span');
		chat_message_content.className = 'chat_message_content';
		var chat_message_content_body = document.createElement('span');
		chat_message_content_body.className = 'chat_message_content_body';
		var message = data['message'];
		if (message.hasOwnProperty('sections'))
		{
			var sections = message['sections'];
			for(var i = 0; i < sections.length; i++)
			{
				var section = sections[i];
				if (section.hasOwnProperty('content') && section.hasOwnProperty('type'))
				{
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

	#OnTwitchChatMessage(data)
	{
		if (data.hasOwnProperty('message') && data.hasOwnProperty('user') && data.hasOwnProperty('color'))
		{
			var user = data['user'];
			if (user.hasOwnProperty('display_name'))
			{
				var chat_message = document.createElement('div');
				chat_message.className = 'chat_message';
				if (data.hasOwnProperty('id'))
					chat_message.id = data['id'];

				var chat_message_author = this.#CreateChatMessageAuthor(user, data['color']);
				var chat_message_separator = this.#CreateTextSparator();
				var chat_message_content = this.#CreateMessageContent(data);

				if (chat_message_content.hasChildNodes())
				{
					chat_message.appendChild(chat_message_author);
					chat_message.appendChild(chat_message_separator);
					chat_message.appendChild(chat_message_content);
					document.getElementById('chat').appendChild(chat_message);
				}
			}
		}
	}

	#OnLoadMessage(response)
	{
		try
		{
			var data = JSON.parse(response);
			if (data.hasOwnProperty('messages'))
			{
				var messages = data['messages'];
				for(var i = 0; i < messages.length; i++)
					this.#OnTwitchChatMessage(messages[i]);
			}
			if (data.hasOwnProperty('page'))
				super.Get('/twitch/all_message?page=' + data['page'], this.#OnLoadMessage.bind(this));
			else
			{
				document.getElementById("full_chat").style.display = 'block';
				super.UnholdEvents();
			}
		} catch(e)
		{
			console.error(`${e.name}: ${e.message}`);
			console.log(response);
		}
	}

	#OnTwitchChatClear(data)
	{
		var chat = document.getElementById('chat');
		while (chat.firstChild)
			chat.removeChild(chat.lastChild);
	}

	#OnTwitchChatUserClear(data)
	{
		var messageToRemove = document.getElementById(data);
		while (messageToRemove != null)
		{
			messageToRemove.parentElement.remove();
			messageToRemove = document.getElementById(data);
		}
	}

	#OnTwitchChatMessageClear(data)
	{
		document.getElementById(data).remove();
	}

	Init()
	{
		this.#textColor = super.GetParameterOr('color', '#ffffff');
		this.#size = super.GetParameterOr('size', '22');
		this.#margin = super.GetParameterOr('margin', '5');
		var chatDiv = document.getElementById('chat');
		if (super.HaveParameter("to_bottom"))
		{
			if (super.HaveParameter("reverse"))
			{
				chatDiv.style.top = 0;
				chatDiv.style.flexDirection = 'column-reverse';
			}
			else
			{
				//TODO
			}
		}
		else
		{
			if (super.HaveParameter("reverse"))
			{
				//TODO
			}
			else
			{
				chatDiv.style.bottom = 0;
				chatDiv.style.flexDirection = 'column';
			}
		}
		document.getElementById("full_chat").style.display = 'none';
		if (super.HaveParameter("limit"))
			super.Get('/twitch/all_message?limit=' + super.GetParameter("limit"), this.#OnLoadMessage.bind(this));
		else
			super.Get('/twitch/all_message', this.#OnLoadMessage.bind(this));
		super.HoldEvents();
		super.RegisterToEvent('twitch_overlay_chat_message', this.#OnTwitchChatMessage.bind(this));
		super.RegisterToEvent('twitch_chat_clear', this.#OnTwitchChatClear.bind(this));
		super.RegisterToEvent('twitch_chat_clear_user', this.#OnTwitchChatUserClear.bind(this));
		super.RegisterToEvent('twitch_chat_clear_message', this.#OnTwitchChatMessageClear.bind(this));
	}
}

var streamGlassModuleClient;

function OnLoad()
{
	streamGlassModuleClient = new ChatModule();
}