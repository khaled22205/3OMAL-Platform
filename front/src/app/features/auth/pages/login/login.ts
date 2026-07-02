import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div
      class="min-h-[80vh] flex items-center justify-center px-4 py-12 sm:px-6 lg:px-8 bg-slate-50 dark:bg-slate-900 transition-colors duration-300"
    >
      <div
        class="max-w-md w-full space-y-8 glass p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800"
      >
        <!-- Header -->
        <div class="text-center">
          <div
            class="mx-auto w-12 h-12 rounded-2xl bg-primary flex items-center justify-center text-white font-extrabold text-2xl shadow-lg shadow-primary/20"
          >
            ع
          </div>
          <h2 class="mt-6 text-3xl font-black text-slate-850 dark:text-white">تسجيل الدخول</h2>
          <p class="mt-2 text-sm text-slate-500 dark:text-slate-400">
            أو
            <a
              routerLink="/register"
              class="font-bold text-primary hover:text-primary-hover transition-colors"
              >إنشاء حساب جديد</a
            >
          </p>
        </div>

        @if (serverError) {
          <div
            class="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs font-bold text-right"
          >
            {{ serverError }}
          </div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
          <!-- Email Field -->
          <div class="space-y-1">
            <label for="email" class="text-sm font-bold text-slate-700 dark:text-slate-350"
              >البريد الإلكتروني</label
            >
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="your@email.com"
              class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary dark:focus:border-primary transition-colors text-right"
              [class.is-invalid]="form.controls.email.invalid && form.controls.email.touched"
            />
            @if (form.controls.email.invalid && form.controls.email.touched) {
              <small class="text-rose-500 text-xs font-bold pr-1 block">
                @if (form.controls.email.errors?.['required']) {
                  البريد الإلكتروني مطلوب
                }
                @if (form.controls.email.errors?.['email']) {
                  البريد الإلكتروني غير صحيح
                }
              </small>
            }
          </div>

          <!-- Password Field -->
          <div class="space-y-1">
            <div class="flex justify-between items-center">
              <label for="password" class="text-sm font-bold text-slate-700 dark:text-slate-350"
                >كلمة المرور</label
              >
            </div>
            <div class="relative">
              <input
                id="password"
                [type]="showPassword ? 'text' : 'password'"
                formControlName="password"
                placeholder="••••••••"
                class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary dark:focus:border-primary transition-colors text-right"
                [class.is-invalid]="
                  form.controls.password.invalid && form.controls.password.touched
                "
              />
              <button
                type="button"
                (click)="togglePassword()"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-xs font-bold text-slate-400 hover:text-primary cursor-pointer"
              >
                {{ showPassword ? 'إخفاء' : 'إظهار' }}
              </button>
            </div>
            @if (form.controls.password.invalid && form.controls.password.touched) {
              <small class="text-rose-500 text-xs font-bold pr-1 block">كلمة المرور مطلوبة</small>
            }
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            class="w-full py-3.5 px-4 bg-primary hover:bg-primary-hover text-white rounded-xl shadow-lg shadow-primary/20 hover:shadow-xl font-bold transition-all text-sm cursor-pointer disabled:opacity-50"
            [disabled]="form.invalid || auth.loading()"
          >
            @if (auth.loading()) {
              <span>جاري تسجيل الدخول...</span>
            } @else {
              تسجيل الدخول
            }
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [
    `
      .is-invalid {
        border-color: #f43f5e !important;
      }
    `,
  ],
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
          err.error?.message || err.error?.title || 'فشل تسجيل الدخول. حاول مرة أخرى.';
      },
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
