import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  standalone: false
})
export class LandingPageComponent {
  private readonly modalService = inject(ModalService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['openLogin'] === 'true') {
        this.modalService.openLogin();  // APRI IL MODAL LOGIN
      }
    });
  }

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
