import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { AuthUser } from '../../../core/models/auth.models';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class HeaderComponent {
  @Output() loginRequested = new EventEmitter<void>();
  @Output() registerRequested = new EventEmitter<void>();

  readonly currentUser$: Observable<AuthUser | null> = this.authService.currentUser$;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  openLogin(): void {
    this.loginRequested.emit();
  }

  openRegister(): void {
    this.registerRequested.emit();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
