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

interface ChatInteraction {
    actions: ChatAction[];
    message: string;
}

export { ChatActionType, ChatAction, ChatInteraction };