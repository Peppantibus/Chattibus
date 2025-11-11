import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';

@Component({
    selector: 'app-landing-page',
    templateUrl: './landing-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class LandingPageComponent {
  private readonly modalService = inject(ModalService);
  private readonly authService = inject(AuthService);

  openLogin(): void {
    this.modalService.openLogin();
  }

  openRegister(): void {
    this.modalService.openRegister();
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }
}
