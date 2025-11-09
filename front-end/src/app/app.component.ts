import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject
} from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AuthService } from './core/services/auth.service';
import { ModalService } from './core/services/modal.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  readonly title = 'Chattibus';
  readonly activeAuthModal$ = this.modalService.authModal$;

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly authService: AuthService,
    private readonly modalService: ModalService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.authService.restoreSession();

    if (this.authService.isAuthenticated() && this.router.url === '/') {
      this.router.navigate(['/home']);
    }

    this.authService.currentUser$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((user) => {
        if (user && this.router.url === '/') {
          this.router.navigate(['/home']);
        }
      });
  }

  closeAuthModal(): void {
    this.modalService.closeAuthModal();
  }

  openLogin(): void {
    this.modalService.openLogin();
  }

  openRegister(): void {
    this.modalService.openRegister();
  }
}
