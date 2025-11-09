import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { combineLatest, map, shareReplay } from 'rxjs';

import { ChatService } from '../../core/services/chat.service';
import { FriendService } from '../../core/services/friend.service';
import {
  ConversationSummary,
  buildConversationSummaries
} from '../../core/models/chat.models';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePageComponent implements OnInit {
  conversations$ = combineLatest([
    this.friendService.getFriends().pipe(shareReplay(1)),
    this.chatService.messages$,
    this.authService.currentUser$
  ]).pipe(
    map(([friends, messages, user]) => {
      if (!user) {
        return [] as ConversationSummary[];
      }
      return buildConversationSummaries(messages, friends, user.username);
    })
  );

  constructor(
    private readonly chatService: ChatService,
    private readonly friendService: FriendService,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.chatService.loadMessages().subscribe();
  }
}
