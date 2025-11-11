import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { ChatService } from '../../core/services/chat.service';
import { WebSocketService } from '../../core/services/websocket.service';
import {
  ConversationSummary,
  buildConversationSummaries,
  messagesForFriend
} from '../../core/models/chat.models';
import { AuthService } from '../../core/services/auth.service';
import { FriendService } from '../../core/services/friend.service';
import { FriendUser } from '../../core/models/friend.models';

@Component({
    selector: 'app-chat-page',
    templateUrl: './chat-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class ChatPageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly chatService = inject(ChatService);
  private readonly webSocketService = inject(WebSocketService);
  private readonly authService = inject(AuthService);
  private readonly friendService = inject(FriendService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly friends = signal<FriendUser[]>([]);
  protected readonly activeFriendId = signal<string | null>(null);

  private readonly currentUser = toSignal(this.authService.currentUser$, {
    initialValue: null
  });
  private readonly allMessages = toSignal(this.chatService.messages$, {
    initialValue: []
  });

  readonly conversations = computed<ConversationSummary[]>(() => {
    const user = this.currentUser();
    if (!user) {
      return [];
    }
    return buildConversationSummaries(
      this.allMessages()  ?? [],
      this.friends(),
      user.username
    );
  });

  readonly activeFriend = computed(() => {
    const id = this.activeFriendId();
    if (!id) {
      return null;
    }
    return this.friends().find((friend) => friend.friendId === id) ?? null;
  });

  readonly visibleMessages = computed(() => {
    const user = this.currentUser();
    const friend = this.activeFriend();
    if (!user || !friend) {
      return [];
    }
    return messagesForFriend(this.allMessages() ?? [], friend, user.username);
  });

  readonly activeConversationSummary = computed<ConversationSummary | null>(
    () => {
      const friend = this.activeFriend();
      if (!friend) {
        return null;
      }
      const conversation = this.conversations().find(
        (item) => item.friendId === friend.friendId
      );
      if (conversation) {
        return conversation;
      }
      return {
        friendId: friend.friendId,
        friendUsername: friend.friendUsername,
        unreadCount: 0
      } satisfies ConversationSummary;
    }
  );

  ngOnInit(): void {
    this.webSocketService.connect();

    this.friendService
      .getFriends()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((friends) => {
        this.friends.set(friends);
        this.ensureActiveFriend();
      });

    this.chatService
      .loadMessages()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();

    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const id = params.get('id');
        if (id) {
          this.activeFriendId.set(id);
        } else {
          this.ensureActiveFriend();
        }
      });

    this.webSocketService.incoming$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        switchMap(() => this.chatService.loadMessages())
      )
      .subscribe();

    this.destroyRef.onDestroy(() => this.webSocketService.disconnect());
  }

  onConversationSelected(conversation: ConversationSummary): void {
    this.activeFriendId.set(conversation.friendId);
    this.router.navigate(['/chat', conversation.friendId]);
  }

  onSendMessage(content: string): void {
    const activeFriend = this.activeFriend();
    if (!activeFriend) {
      return;
    }
    this.webSocketService.sendMessage({
      ToUserId: activeFriend.friendId,
      Content: content
    });
    this.chatService
      .loadMessages()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  private ensureActiveFriend(): void {
    const friends = this.friends();
    if (!friends.length) {
      return;
    }

    const routeId = this.route.snapshot.paramMap.get('id');
    if (routeId && friends.some((friend) => friend.friendId === routeId)) {
      this.activeFriendId.set(routeId);
      return;
    }

    const currentId = this.activeFriendId();
    if (currentId && friends.some((friend) => friend.friendId === currentId)) {
      return;
    }

    const first = friends[0];
    this.activeFriendId.set(first.friendId);
    this.router.navigate(['/chat', first.friendId], { replaceUrl: true });
  }
}
