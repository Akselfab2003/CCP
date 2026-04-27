//import * as signalR from "@microsoft/signalr";

class ChatBot {
    constructor() {
        this.connection = null;
        this.init();
        this.activeConversationID = null;
    }

    init() {
        this.createChatToggle();
        this.getSession();
        // Only create chat window when opened for performance
        document.getElementById('chatToggle').addEventListener('click', () => this.toggleChat());
    }

    getSession() {
        const sessionUrl = "https://localhost:7127/session";
        fetch(sessionUrl, {
            method: "POST",
            credentials: "include",
        }).catch(error => {
            console.error("Error fetching session:", error);
        });
    }

    toggleChat() {
        let chatWindow = document.getElementById("chatWindow");
        const chatToggle = document.getElementById("chatToggle");
        if (!chatWindow) {
            this.createChatWindow();
            this.connectToChatHub();
            this.createChatConversation();
            chatWindow = document.getElementById("chatWindow");
        }
        chatWindow.classList.toggle("open");
        chatToggle.classList.toggle("open");
    }

    createChatToggle() {
        const home = document.body;
        const root = document.createElement("button");
        root.id = "chatToggle";
        root.setAttribute("aria-label", "Åbn chat");

        const icon = document.createElement("span");
        icon.id = "chatBadge";
        root.appendChild(icon);

        // SVG icons
        root.appendChild(this.createChatIcon());
        root.appendChild(this.createCloseIcon());

        home.appendChild(root);
    }

    createChatIcon() {
        const icon_chat = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        icon_chat.classList.add("icon-chat");
        icon_chat.setAttribute("width", "22");
        icon_chat.setAttribute("height", "22");
        icon_chat.setAttribute("viewBox", "0 0 24 24");
        icon_chat.setAttribute("fill", "none");
        icon_chat.setAttribute("stroke", "currentColor");
        icon_chat.setAttribute("stroke-width", "2.2");
        icon_chat.setAttribute("stroke-linecap", "round");
        icon_chat.setAttribute("stroke-linejoin", "round");
        const path_chat = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path_chat.setAttribute("d", "M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z");
        icon_chat.appendChild(path_chat);
        return icon_chat;
    }

    createCloseIcon() {
        const icon_close = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        icon_close.classList.add("icon-close");
        icon_close.setAttribute("width", "20");
        icon_close.setAttribute("height", "20");
        icon_close.setAttribute("viewBox", "0 0 24 24");
        icon_close.setAttribute("fill", "none");
        icon_close.setAttribute("stroke", "currentColor");
        icon_close.setAttribute("stroke-width", "2.5");
        icon_close.setAttribute("stroke-linecap", "round");
        icon_close.setAttribute("stroke-linejoin", "round");
        const line1 = document.createElementNS("http://www.w3.org/2000/svg", "line");
        line1.setAttribute("x1", "18");
        line1.setAttribute("y1", "6");
        line1.setAttribute("x2", "6");
        line1.setAttribute("y2", "18");
        icon_close.appendChild(line1);
        const line2 = document.createElementNS("http://www.w3.org/2000/svg", "line");
        line2.setAttribute("x1", "6");
        line2.setAttribute("y1", "6");
        line2.setAttribute("x2", "18");
        line2.setAttribute("y2", "18");
        icon_close.appendChild(line2);
        return icon_close;
    }

    createChatHeader() {
        const header = document.createElement("div");
        header.classList.add("chat-header");

        const avatar = document.createElement("div");
        avatar.classList.add("chat-header-avatar");
        avatar.textContent = "B";
        header.appendChild(avatar);

        const info = document.createElement("div");
        info.classList.add("chat-header-info");
        const chat_header_name = document.createElement("div");
        chat_header_name.textContent = "Chatbot";
        chat_header_name.classList.add("chat-header-name");
        info.appendChild(chat_header_name);

        const chat_header_status = document.createElement("div");
        chat_header_status.classList.add("chat-header-status");
        const status_indicator = document.createElement("span");
        status_indicator.classList.add("status-dot");
        chat_header_status.appendChild(status_indicator);
        chat_header_status.appendChild(document.createTextNode("Online"));
        info.appendChild(chat_header_status);

        header.appendChild(info);
        return header;
    }

    createChatMessages() {
        const messages = document.createElement("div");
        messages.classList.add("chat-messages");
        messages.id = "chatMessages";
        return messages;
    }

    showTypingIndicator() {
        const messages = document.getElementById("chatMessages");
        if (!messages || messages.querySelector('.typing-indicator')) return;
        const typingIndicator = document.createElement("div");
        typingIndicator.classList.add("typing-indicator");
        for (let i = 0; i < 3; i++) {
            const dot = document.createElement("div");
            dot.classList.add("typing-dot");
            typingIndicator.appendChild(dot);
        }
        messages.appendChild(typingIndicator);
        messages.scrollTop = messages.scrollHeight;
    }

    hideTypingIndicator() {
        const messages = document.getElementById("chatMessages");
        if (!messages) return;
        const indicator = messages.querySelector('.typing-indicator');
        if (indicator) {
            messages.removeChild(indicator);
        }
    }

    addMessage({ text, role = "user", time = null }) {
        const messages = document.getElementById("chatMessages");
        if (!messages) return;
        const msg = document.createElement("div");
        msg.classList.add("msg", role);
        msg.textContent = text;
        const msg_meta = document.createElement("div");
        msg_meta.classList.add("msg-meta");
        msg_meta.textContent = time || this.getCurrentTime();
        msg.appendChild(msg_meta);
        messages.appendChild(msg);
        messages.scrollTop = messages.scrollHeight;
    }

    getCurrentTime() {
        const now = new Date();
        return now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    createChatInput() {
        const form = document.createElement("form");
        form.classList.add("chat-input-area");
        form.id = "chatForm";
        const input = document.createElement("input");
        input.type = "text";
        input.placeholder = "write a message...";
        input.id = "chatInput";
        form.appendChild(input);

        const button = document.createElement("button");
        button.type = "submit";
        button.ariaLabel = "Send Message";
        const send_icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        send_icon.setAttribute("width", "22");
        send_icon.setAttribute("height", "22");
        send_icon.setAttribute("viewBox", "0 0 24 24");
        send_icon.setAttribute("fill", "none");
        send_icon.setAttribute("stroke", "currentColor");
        send_icon.setAttribute("stroke-width", "2.2");
        send_icon.setAttribute("stroke-linecap", "round");
        send_icon.setAttribute("stroke-linejoin", "round");
        const path1 = document.createElementNS("http://www.w3.org/2000/svg", "line");
        path1.setAttribute("x1", "22");
        path1.setAttribute("y1", "2");
        path1.setAttribute("x2", "11");
        path1.setAttribute("y2", "13");
        send_icon.appendChild(path1);
        const path2 = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
        path2.setAttribute("points", "22 2 15 22 11 13 2 9 22 2");
        send_icon.appendChild(path2);
        button.appendChild(send_icon);
        form.appendChild(button);

        button.addEventListener("click", (e) => {
            e.preventDefault();
            const message = input.value.trim();
            if (message) {
                this.sendMessage(message);
                input.value = "";
            }
        });

        return form;
    }

    createChatWindow() {
        const root = document.createElement("div");
        root.id = "chatWindow";
        root.setAttribute("role", "dialog");
        root.setAttribute("aria-label", "Chat");
        root.appendChild(this.createChatHeader());
        root.appendChild(this.createChatMessages());
        root.appendChild(this.createChatInput());
        document.body.appendChild(root);
    }

    getSessionIdFromCookie() {
        const cookies = document.cookie.split(";").map(cookie => cookie.trim());
        const sessionIdCookie = cookies.find(cookie => cookie.startsWith("SessionId="));
        return sessionIdCookie ? sessionIdCookie.split("=")[1] : null;
    }

    sendMessage(message) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            this.connection.invoke("SendMessageToChatBot", this.activeConversationID, message)
                .catch(err => console.error("Error sending message:", err));
        }
        this.addMessage({ text: message, role: "user" });
    }

    connectToChatHub() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7127/chatHub", {
                accessTokenFactory: () => {
                    const sessionId = this.getSessionIdFromCookie();
                    console.log("Retrieved session ID from cookie:", sessionId);
                    return sessionId || "";
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        this.connection.on("test", msg => {
            console.log("Received message from hub:", msg);
        });

        this.connection.onreconnected(cid => {
            console.log("Reconnected to hub with connection ID:", cid);
        });


        this.connection.on("ChatMessage", message => {
            console.log("Received new message:", message);
            this.hideTypingIndicator();
            this.addMessage({ text: message, role: "bot" });
        });

        this.connection.on("ReceiveTyping", conversationID => {
            console.log("Received typing indicator for conversation", conversationID);
            this.showTypingIndicator();
        });

        this.connection.on("ReceiveMessage", (conversationID, message) => {
            console.log("Received message for conversation", conversationID, ":", message);
            this.hideTypingIndicator();
            this.addMessage({ text: message, role: "bot" });
        });

        this.connection.start();
    }

    // Creates a new conversation and sets the activeConversationID
    async createChatConversation() {
        const conversationUrl = "https://localhost:7127/chat/createConversation";
        var conversationID = await fetch(conversationUrl, {
            method: "POST",
            credentials: "include",
        }).then(async response => {
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }
            console.log("Conversation created successfully");
            var data = await response.json();
            let conversationID = data["conversationID"];
            console.log("Conversation ID:", conversationID);
            return conversationID;
        })
            .catch(error => {
                console.error("Error creating conversation:", error);
            });
        this.activeConversationID = conversationID;
    }

}

window.addEventListener('DOMContentLoaded', () => {
    new ChatBot();
});

