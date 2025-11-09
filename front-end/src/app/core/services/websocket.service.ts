import { Injectable } from '@angular/core';
import { filter, Subject } from 'rxjs';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { IncomingWsMessage, OutgoingWsMessage } from '../models/chat.models';

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  private socket$?: WebSocketSubject<IncomingWsMessage | OutgoingWsMessage>;
  private readonly incomingSubject = new Subject<IncomingWsMessage>();

  readonly incoming$ = this.incomingSubject.asObservable();

  constructor(private readonly authService: AuthService) {}

  connect(): void {
    if (this.socket$ || !this.authService.isAuthenticated()) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    const wsUrl = `${environment.wsUrl}?token=${encodeURIComponent(token)}`;

    this.socket$ = webSocket<IncomingWsMessage | OutgoingWsMessage>({
      url: wsUrl,
      deserializer: ({ data }) => JSON.parse(data),
      serializer: (value) => JSON.stringify(value)
    });

    this.socket$
      .pipe(
        filter((message): message is IncomingWsMessage =>
          typeof message === 'object' &&
          message !== null &&
          'Content' in message &&
          'ToUserId' in message
        )
      )
      .subscribe({
        next: (message) => this.incomingSubject.next(message),
        error: () => this.disconnect(),
        complete: () => this.disconnect()
      });
  }

  disconnect(): void {
    this.socket$?.complete();
    this.socket$ = undefined;
  }

  sendMessage(message: OutgoingWsMessage): void {
    this.connect();
    this.socket$?.next(message);
  }
}
