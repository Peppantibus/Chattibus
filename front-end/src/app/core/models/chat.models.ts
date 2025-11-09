import { FriendUser } from './friend.models';

export interface ChatMessage {
  id: number;
  content: string;
  senderUsername: string;
  receivedUsername: string;
  sentAt: string;
  isMine: boolean;
  isRead: boolean;
}

export interface ConversationSummary {
  friendId: string;
  friendUsername: string;
  lastMessagePreview?: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface IncomingWsMessage {
  ToUserId: string;
  Content: string;
}

export type OutgoingWsMessage = IncomingWsMessage;

export function messagesForFriend(
  messages: ChatMessage[],
  friend: FriendUser,
  currentUsername: string
): ChatMessage[] {
  return messages
    .filter((message) => {
      const isFromFriend =
        message.senderUsername === friend.friendUsername &&
        message.receivedUsername === currentUsername;
      const isFromMe =
        message.senderUsername === currentUsername &&
        message.receivedUsername === friend.friendUsername;
      return isFromFriend || isFromMe;
    })
    .sort(
      (a, b) =>
        new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
    );
}

export function buildConversationSummaries(
  messages: ChatMessage[],
  friends: FriendUser[],
  currentUsername: string
): ConversationSummary[] {
  return friends
    .map((friend) => {
      const conversationMessages = messagesForFriend(
        messages,
        friend,
        currentUsername
      );
      const lastMessage = conversationMessages.at(-1);
      const unreadCount = conversationMessages.filter(
        (message) => !message.isMine && !message.isRead
      ).length;

      return {
        friendId: friend.friendId,
        friendUsername: friend.friendUsername,
        lastMessagePreview: lastMessage?.content,
        lastMessageAt: lastMessage?.sentAt,
        unreadCount
      } satisfies ConversationSummary;
    })
    .sort((a, b) => {
      const aTime = a.lastMessageAt
        ? new Date(a.lastMessageAt).getTime()
        : 0;
      const bTime = b.lastMessageAt
        ? new Date(b.lastMessageAt).getTime()
        : 0;
      return bTime - aTime;
    });
}
