import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { FriendRequest, FriendRequestType, FriendUser } from '../models/friend.models';

@Injectable({
  providedIn: 'root'
})
export class FriendService {

  private readonly http = inject(HttpClient);

  getFriends(): Observable<FriendUser[]> {
    return this.http.get<FriendUser[]>(`${environment.apiUrl}/friend/all`);
  }

  getFriendRequests(type: FriendRequestType): Observable<FriendRequest[]> {
    const params = new HttpParams().set('type', type);
    return this.http.get<FriendRequest[]>(
      `${environment.apiUrl}/friendrequest/list`,
      { params }
    );
  }

  sendFriendRequest(username: string): Observable<void> {
    const params = new HttpParams().set('username', username);
    return this.http.post<void>(
      `${environment.apiUrl}/friendrequest/send`,
      null,
      { params }
    );
  }

  acceptRequest(requestId: number): Observable<void> {
    return this.http.put<void>(
      `${environment.apiUrl}/friendrequest/${requestId}/accept`,
      {}
    );
  }

  declineRequest(requestId: number): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiUrl}/friendrequest/${requestId}`
    );
  }

  deleteFriend(friendId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/friend/${friendId}`);
  }
}
