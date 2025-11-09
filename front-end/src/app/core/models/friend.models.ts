export interface FriendUser {
  id: number;
  friendId: string;
  friendUsername: string;
  createdAt: string;
}

export type FriendRequestType = 'Sent' | 'Received';

export interface FriendRequest {
  id: number;
  senderUsername: string;
  receiverUsername: string;
  createdAt: string;
}
