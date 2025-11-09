import { ChangeDetectionStrategy, Component } from '@angular/core';

import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingPageComponent {
  constructor(
    private readonly modalService: ModalService,
    private readonly authService: AuthService
  ) {}

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
