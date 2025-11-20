import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ResetPasswordComponent {
  form: FormGroup;
  token: string | null = null;

  loading = false;
  error?: string;
  success?: string;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    this.form = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.error = 'Token non valido.';
      return;
    }

    this.authService.validatePasswordResetToken(this.token).subscribe({
      next: (isValid) => {
        if (!isValid) {
          this.error = 'Il link non è più valido.';
        }
      },
      error: () => {
        this.error = 'Il link non è più valido.';
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.token) return;

    const { password, confirmPassword } = this.form.value;

    this.loading = true;
    this.error = undefined;
    this.success = undefined;

    this.authService.resetPassword({
      token: this.token,
      password,
      confirmPassword
    }).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Password aggiornata con successo.';
      },
      error: () => {
        this.loading = false;
        this.error = 'Errore durante il reset della password.';
      }
    });
  }
}
