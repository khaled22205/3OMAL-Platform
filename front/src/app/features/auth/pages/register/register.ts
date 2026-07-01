import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  return password && confirmPassword && password.value !== confirmPassword.value
    ? { passwordMismatch: true }
    : null;
}

interface ValidationProblemDetails {
  title?: string;
  errors?: Record<string, string[]>;
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export default class Register {
  private readonly fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      userType: ['Customer' as 'Customer' | 'Worker', [Validators.required]],
    },
    { validators: passwordMatchValidator },
  );

  showPassword = false;
  showConfirmPassword = false;
  serverError = '';
  successMessage = '';

  private readonly fieldNameMap: Record<string, string> = {
    firstName: 'firstName',
    lastName: 'lastName',
    email: 'email',
    phoneNumber: 'phoneNumber',
    password: 'password',
    userType: 'userType',
  };

  onSubmit(): void {
    if (this.form.invalid) return;

    this.serverError = '';
    this.successMessage = '';
    const { confirmPassword: _, ...request } = this.form.getRawValue();

    this.auth.register(request).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = res.message || 'Registration successful!';
          this.auth.navigateByRole();
        }
      },
      error: (err: HttpErrorResponse) => {
        const errorBody = err.error as Record<string, unknown> | null;

        if (!errorBody) {
          this.serverError = err.status === 0
            ? 'Network error. Please check your connection.'
            : `Server error (${err.status}). Please try again.`;
          return;
        }

        const errors = errorBody['errors'];

        if (errors && typeof errors === 'object' && !Array.isArray(errors)) {
          const dict = errors as Record<string, string[]>;
          const messages: string[] = [];
          for (const [field, fieldErrors] of Object.entries(dict)) {
            if (Array.isArray(fieldErrors)) {
              messages.push(...fieldErrors);
              const controlName = this.fieldNameMap[field];
              if (controlName) {
                this.form.controls[controlName as keyof typeof this.form.controls]?.markAsTouched();
              }
            }
          }
          this.serverError = messages.length > 0 ? messages.join('. ') : (errorBody['title'] as string) || 'Validation failed.';
          return;
        }

        if (Array.isArray(errors)) {
          this.serverError = (errors as string[]).join('. ');
          return;
        }

        this.serverError = (errorBody['message'] as string) || (errorBody['title'] as string) || `Registration failed (${err.status}). Please try again.`;
      },
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }
}
