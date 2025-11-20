import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register-modal',
  templateUrl: './register-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: false
})
export class RegisterModalComponent {
  @Output() close = new EventEmitter<void>();
  @Output() switchToLogin = new EventEmitter<void>();

  mode: 'form' | 'verify' = 'form';
  email: string = '';

  form: FormGroup = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    name: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  loading = false;
  error?: string;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.cdr.markForCheck();

    this.authService.register(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.email = this.form.value.email;

        // passaggio allo stato "verify"
        this.mode = 'verify';
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.error = 'Registrazione non riuscita. Riprova più tardi.';
        this.cdr.markForCheck();
      }
    });
  }
}
