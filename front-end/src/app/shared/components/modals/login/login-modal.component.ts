import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login-modal',
  templateUrl: './login-modal.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  standalone: false
})
export class LoginModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() switchToRegister = new EventEmitter<void>();

  mode: 'login' | 'forgot' = 'login';

  form: FormGroup = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  forgotForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  loading = false;
  error?: string;
  success?: string;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService
  ) {}

  // LOGIN SUBMIT
  submit(): void {
    if (this.mode !== 'login') return;

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;

    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.close.emit();
      },
      error: () => {
        this.loading = false;
        this.error = 'Credenziali non valide. Riprova.';
      }
    });
  }

  // SWITCH → FORGOT
  showForgot(): void {
    this.mode = 'forgot';
    this.error = undefined;
    this.success = undefined;
  }

  // SWITCH → LOGIN
  backToLogin(): void {
    this.mode = 'login';
    this.error = undefined;
    this.success = undefined;
  }

  // RECOVERY SUBMIT
  submitForgot(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;

    const { email } = this.forgotForm.value;

    this.authService.requestPasswordRecovery(email).subscribe({
      next: (msg) => {
        this.loading = false;
        this.success = msg;
      },
      error: () => {
        this.loading = false;
        this.success = "Se l'email è registrata, ti abbiamo inviato un link per il reset.";
      }
    });
  }
}
