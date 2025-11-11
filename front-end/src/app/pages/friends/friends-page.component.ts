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
import { UserResponse } from 'src/app/core/models/user.models';
import { UserService } from 'src/app/core/services/user.service';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
    selector: 'app-friends-page',
    templateUrl: './friends-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class FriendsPageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private userService = inject(UserService);
  private readonly friendService = inject(FriendService);
  protected readonly searchControl = new FormControl<string>('', { nonNullable: true });

  protected readonly friends = signal<FriendUser[]>([]);
  protected readonly requests = signal<FriendRequest[]>([]);
  protected readonly requestType = signal<FriendRequestType>('Received');
  protected readonly searchResults = signal<UserResponse[]>([]);

  ngOnInit(): void {
    this.loadFriends();
    this.loadRequests();
    this.setupSearch();
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

  private setupSearch(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),           // aspetta 300ms dopo l’ultimo tasto
        distinctUntilChanged(),      // evita chiamate duplicate
        switchMap((term) => this.userService.getUsers(term.trim())) // chiama API
      )
      .subscribe({
        next: (users) => this.searchResults.set(users),
        error: () => this.searchResults.set([])
      });
  }

  searchUsers(username: string): void {
    this.userService.getUsers(username).subscribe((users) => {
      this.searchResults.set(users);
    });
  }

  addFriend(user: UserResponse): void {
    this.friendService
      .sendFriendRequest(user.username)
      .subscribe(() => this.searchResults.set([])); // pulisci la lista dopo invio
  }
}
