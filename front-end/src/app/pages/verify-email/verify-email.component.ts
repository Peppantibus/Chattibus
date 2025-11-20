import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  templateUrl: './verify-email.component.html',
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.Default
})
export class VerifyEmailComponent {
  token: string | null = null;
  loading = true;
  success = false;
  error = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.loading = false;
      this.error = true;
      return;
    }

    this.authService.verifyEmail(this.token).subscribe({
      next: (result) => {
        this.loading = false;
        this.success = result === true;
        this.error = result !== true;

        // redirect automatico dopo 2s alla landing con modal login aperto
        if (this.success) {
          setTimeout(() => {
            this.router.navigate(['/'], {
              queryParams: { openLogin: true }
            });
          }, 2000);
        }
      },
      error: () => {
        this.loading = false;
        this.error = true;
      }
    });
  }
}
