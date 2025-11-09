import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register-modal',
  templateUrl: './register-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() switchToLogin = new EventEmitter<void>();

  form: FormGroup = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  loading = false;
  error?: string;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = undefined;

    this.authService.register(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.close.emit();
      },
      error: () => {
        this.loading = false;
        this.error = 'Registrazione non riuscita. Riprova più tardi.';
      }
    });
  }
}
