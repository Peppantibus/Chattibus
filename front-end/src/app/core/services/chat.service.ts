import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ChatMessage } from '../models/chat.models';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  private readonly http = inject(HttpClient);
  readonly messages$ = this.messagesSubject.asObservable();


  loadMessages(): Observable<ChatMessage[]> {
    return this.http
      .get<ChatMessage[]>(`${environment.apiUrl}/message/all`)
      .pipe(tap((messages) => this.messagesSubject.next(messages)));
  }

  snapshot(): ChatMessage[] {
    return this.messagesSubject.value;
  }
}
