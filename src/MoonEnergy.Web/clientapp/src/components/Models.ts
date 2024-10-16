enum ChatActionType {
    None = 0,
    Login = 1,
    Logout = 2,
    Render = 3
}

interface ChatAction {
    action: ChatActionType;
    actionContentAsJson?: string;
}

interface ChatMessage
{
    type: string;
    text: string;
}

interface ChatInteraction {
    actions: ChatAction[];
    messages: ChatMessage[];
}

export { ChatActionType, ChatAction, ChatInteraction };