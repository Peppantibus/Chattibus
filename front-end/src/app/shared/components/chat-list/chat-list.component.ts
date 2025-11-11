import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { ConversationSummary } from '../../../core/models/chat.models';

@Component({
    selector: 'app-chat-list',
    templateUrl: './chat-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class ChatListComponent {
  @Input() conversations: ConversationSummary[] | null | undefined;
  @Input() activeConversationId?: string | null;
  @Output() conversationSelected = new EventEmitter<ConversationSummary>();

  trackById(_: number, conversation: ConversationSummary): string {
    return conversation.friendId;
  }

  select(conversation: ConversationSummary): void {
    this.conversationSelected.emit(conversation);
  }
}
