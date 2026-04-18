window.onload = function() {
    LoadChatBot();
};

function getSession(){
    const sessionUrl = "https://localhost:7127/session";

    fetch(sessionUrl, {
        method: "POST",
        credentials: "include",
    }).catch(error => {
        console.error("Error fetching session:", error);
    });
}

function LoadChatBot() {
    const chatWindow = document.createElement('div');
    const chatToggle = document.createElement('button');
    CreateChatToggle();
    CreateChatWindow();
    getSession();

}

function ToggleChat() {
    const chatWindow = document.getElementById("chatWindow");
    const chatToggle = document.getElementById("chatToggle");
    chatWindow.classList.toggle("open");
    chatToggle.classList.toggle("open");

}

function CreateChatToggle() {
    const home = document.querySelector("body");
    const root = document.createElement("button");
    root.id = "chatToggle";
    root.setAttribute("aria-label", "Åbn chat");
    
    const icon = document.createElement("span");
    icon.id = "chatBadge"
    root.appendChild(icon);


    var icon_chat = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    icon_chat.classList.add("icon-chat");
    icon_chat.setAttribute("width", "22");
    icon_chat.setAttribute("height", "22");
    icon_chat.setAttribute("viewBox", "0 0 24 24");
    icon_chat.setAttribute("fill", "none");
    icon_chat.setAttribute("stroke", "currentColor");
    icon_chat.setAttribute("stroke-width", "2.2");
    icon_chat.setAttribute("stroke-linecap", "round");
    icon_chat.setAttribute("stroke-linejoin", "round");
    var path_chat = document.createElementNS("http://www.w3.org/2000/svg", "path");
    path_chat.setAttribute("d", "M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z");
    icon_chat.appendChild(path_chat);

    
    var icon_close = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    icon_close.classList.add("icon-close");
    icon_close.setAttribute("width", "20");
    icon_close.setAttribute("height", "20");
    icon_close.setAttribute("viewBox", "0 0 24 24");
    icon_close.setAttribute("fill", "none");
    icon_close.setAttribute("stroke", "currentColor");
    icon_close.setAttribute("stroke-width", "2.5");
    icon_close.setAttribute("stroke-linecap", "round");
    icon_close.setAttribute("stroke-linejoin", "round");
    var line1 = document.createElementNS("http://www.w3.org/2000/svg", "line");
    line1.setAttribute("x1", "18");
    line1.setAttribute("y1", "6");
    line1.setAttribute("x2", "6");
    line1.setAttribute("y2", "18");
    icon_close.appendChild(line1);
    var line2 = document.createElementNS("http://www.w3.org/2000/svg", "line");
    line2.setAttribute("x1", "6");
    line2.setAttribute("y1", "6");
    line2.setAttribute("x2", "18");
    line2.setAttribute("y2", "18");
    icon_close.appendChild(line2);

    root.appendChild(icon_chat);
    root.appendChild(icon_close);

    root.addEventListener("click", function() {
        ToggleChat();
    });

    home.appendChild(root);
}


function CreateChatHeader() {
    const header = document.createElement("div");
    header.classList.add("chat-header");

    const avatar = document.createElement("div");
    avatar.classList.add("chat-header-avatar");
    avatar.textContent = "B";
    header.appendChild(avatar);
    const info = document.createElement("div");
    info.classList.add("chat-header-info");
    var chat_header_name = document.createElement("div");
    chat_header_name.textContent = "Chatbot";
    chat_header_name.classList.add("chat-header-name");
    info.appendChild(chat_header_name);
    var chat_header_status = document.createElement("div");
    chat_header_status.classList.add("chat-header-status");
    
    var status_indicator = document.createElement("span");
    status_indicator.classList.add("status-dot");
    chat_header_status.appendChild(status_indicator);
    chat_header_status.appendChild(document.createTextNode("Online"));
    info.appendChild(chat_header_status);
    
    header.appendChild(info);
    return header;
}

function CreateChatMessages(){
    var messages = document.createElement("div");
    messages.classList.add("chat-messages");
    messages.id = "chatMessages";

    var typing_indicator = document.createElement("div");
    typing_indicator.classList.add("typing-indicator");
    for (let i = 0; i < 4; i++) {
        var dot = document.createElement("div");
        dot.classList.add("typing-dot");
        typing_indicator.appendChild(dot);
    }
    messages.appendChild(typing_indicator);

    var msg = document.createElement("div");
    msg.classList.add("msg", "user");
    msg.textContent = "Hej, hvordan har du det?";
    var msg_meta = document.createElement("div");
    msg_meta.classList.add("msg-meta");
    msg_meta.textContent = "12:00";
    msg.appendChild(msg_meta);
    messages.appendChild(msg);
    return messages;
}

function CreateChatInput() {
    var form = document.createElement("form");
    form.classList.add("chat-input-area");
    form.id = "chatForm";
        var input = document.createElement("input");
        input.type = "text";
        input.placeholder = "write a message...";
        input.id = "chatInput";
        form.appendChild(input);

        var button = document.createElement("button");
        button.type = "submit";
        button.ariaLabel = "Send Message";
        var send_icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        send_icon.setAttribute("width", "22");
        send_icon.setAttribute("height", "22");
        send_icon.setAttribute("viewBox", "0 0 24 24");
        send_icon.setAttribute("fill", "none");
        send_icon.setAttribute("stroke", "currentColor");
        send_icon.setAttribute("stroke-width", "2.2");
        send_icon.setAttribute("stroke-linecap", "round");
        send_icon.setAttribute("stroke-linejoin", "round");
        var path1 = document.createElementNS("http://www.w3.org/2000/svg", "line");
        path1.setAttribute("x1", "22");
        path1.setAttribute("y1", "2");
        path1.setAttribute("x2", "11");
        path1.setAttribute("y2", "13");
        send_icon.appendChild(path1);
        var path2 = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
        path2.setAttribute("points", "22 2 15 22 11 13 2 9 22 2");
        send_icon.appendChild(path2);
        button.appendChild(send_icon);
        form.appendChild(button);

        return form;
}

function CreateChatWindow() {
    const root = document.createElement("div");
    root.id = "chatWindow";
    root.setAttribute("role", "dialog");
    root.setAttribute("aria-label", "Chat");
    const header = CreateChatHeader();
    root.appendChild(header);

    const messages = CreateChatMessages();
    root.appendChild(messages);
    var form = CreateChatInput();
    root.appendChild(form);
    document.body.appendChild(root);
}

