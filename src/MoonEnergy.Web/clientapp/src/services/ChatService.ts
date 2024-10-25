import {ChatInteraction} from "../components/Models.ts";

export async function InitChat(sessionId: string): Promise<ChatInteraction[]> {
    const response = await fetch('/api/chat/init', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(sessionId),
    });

    return await response.json();
}

export async function SendChat(sessionId: string, message: string): Promise<ChatInteraction> {
    const payload = {
        message,
        sessionId
    };

    const response = await fetch('/api/chat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
    });

    if (!response.ok) {
        throw new Error('API request failed');
    }

    return await response.json();
}