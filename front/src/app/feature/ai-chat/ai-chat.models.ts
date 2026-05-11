export interface CompletedMessage {
  readonly prompt: string;
  readonly answer: string;
  readonly reasoning: string;
}

export interface ChatSession {
  readonly id: string;
  readonly displayName: string;
  readonly createdAt: string;
}

export interface ChatMessage {
  readonly id: string;
  readonly userPrompt: string;
  readonly answer: string;
  readonly createdAt: string;
}
