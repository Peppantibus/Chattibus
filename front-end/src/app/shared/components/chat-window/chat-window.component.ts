import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
  inject
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ChatMessage, ConversationSummary } from '../../../core/models/chat.models';

@Component({
    selector: 'app-chat-window',
    templateUrl: './chat-window.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class ChatWindowComponent implements AfterViewInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  @Input() conversation?: ConversationSummary | null;
  @Input() messages: ChatMessage[] | null | undefined;
  @Output() messageSent = new EventEmitter<string>();
  @ViewChild('scroller') private readonly scroller?: ElementRef<HTMLDivElement>;

  form: FormGroup = this.fb.group({
    message: ['', [Validators.required, Validators.maxLength(2000)]]
  });



  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['messages']) {
      this.scrollToBottom();
    }
  }

  send(): void {
    if (this.form.invalid) {
      return;
    }
    const content = this.form.value.message.trim();
    if (!content) {
      return;
    }
    this.messageSent.emit(content);
    this.form.reset();
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.scroller) {
        const el = this.scroller.nativeElement;
        el.scrollTop = el.scrollHeight;
      }
    }, 0);
  }

  onEnter(event: KeyboardEvent): void {
    if (!event.shiftKey) {
      event.preventDefault(); 
      this.send();
    }
  }
}
