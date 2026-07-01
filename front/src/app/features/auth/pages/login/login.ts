import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export default class Login {
  private readonly fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  showPassword = false;
  serverError = '';

  onSubmit(): void {
    if (this.form.invalid) return;

    this.serverError = '';
    const { email, password } = this.form.getRawValue();

    this.auth.login({ email, password }).subscribe({
      next: (res) => {
        if (res.success) {
          this.auth.navigateByRole();
        }
      },
      error: (err) => {
        this.serverError =
          err.error?.message || err.error?.title || 'Login failed. Please try again.';
      },
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
