import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ChatMessage } from '../models/chat.models';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly messagesSubject = new BehaviorSubject<ChatMessage[]>([]);

  readonly messages$ = this.messagesSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  loadMessages(): Observable<ChatMessage[]> {
    return this.http
      .get<ChatMessage[]>(`${environment.apiUrl}/message/all`)
      .pipe(tap((messages) => this.messagesSubject.next(messages)));
  }

  snapshot(): ChatMessage[] {
    return this.messagesSubject.value;
  }
}
