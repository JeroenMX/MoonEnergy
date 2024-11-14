enum ChatActionType {
    None = 0,
    Login = 1,
    Logout = 2,
    Render = 3
}

interface ChatAction {
    action: ChatActionType;
    name: string;
    contentAsJson: string;
}

interface ChatMessage {
    type: string;
    text: string;
}

interface ChatInteraction {
    actions: ChatAction[];
    messages: ChatMessage[];
}

interface ComponentDictionary {
    [key: string]: any;
}

export {ChatActionType, ChatAction, ChatMessage, ChatInteraction, ComponentDictionary};