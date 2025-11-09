import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { FriendService } from '../../core/services/friend.service';
import {
  FriendRequest,
  FriendRequestType,
  FriendUser
} from '../../core/models/friend.models';

@Component({
  selector: 'app-friends-page',
  templateUrl: './friends-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FriendsPageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  protected readonly friends = signal<FriendUser[]>([]);
  protected readonly requests = signal<FriendRequest[]>([]);
  protected readonly requestType = signal<FriendRequestType>('Received');

  constructor(private readonly friendService: FriendService) {}

  ngOnInit(): void {
    this.loadFriends();
    this.loadRequests();
  }

  refresh(): void {
    this.loadFriends();
    this.loadRequests();
  }

  switchType(type: FriendRequestType): void {
    if (this.requestType() === type) {
      return;
    }
    this.requestType.set(type);
    this.loadRequests();
  }

  accept(request: FriendRequest): void {
    this.friendService
      .acceptRequest(request.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadFriends();
        this.loadRequests();
      });
  }

  decline(request: FriendRequest): void {
    this.friendService
      .declineRequest(request.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadRequests());
  }

  private loadFriends(): void {
    this.friendService
      .getFriends()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((friends) => this.friends.set(friends));
  }

  private loadRequests(): void {
    const type = this.requestType();
    this.friendService
      .getFriendRequests(type)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((requests) => this.requests.set(requests));
  }
}
