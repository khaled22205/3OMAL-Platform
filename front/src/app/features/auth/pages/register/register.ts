import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { MockDataService } from '../../../../core/services/mock-data.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  return password && confirmPassword && password.value !== confirmPassword.value
    ? { passwordMismatch: true }
    : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-[90vh] flex items-center justify-center px-4 py-12 sm:px-6 lg:px-8 bg-slate-50 dark:bg-slate-900 transition-colors duration-300">
      <div class="max-w-xl w-full space-y-8 glass p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800">
        
        <!-- Header -->
        <div class="text-center">
          <div class="mx-auto w-12 h-12 rounded-2xl bg-primary flex items-center justify-center text-white font-extrabold text-2xl shadow-lg shadow-primary/20">
            ع
          </div>
          <h2 class="mt-6 text-3xl font-black text-slate-850 dark:text-white">إنشاء حساب جديد</h2>
          <p class="mt-2 text-sm text-slate-500 dark:text-slate-400">
            لديك حساب بالفعل؟ 
            <a routerLink="/login" class="font-bold text-primary hover:text-primary-hover transition-colors">تسجيل الدخول</a>
          </p>
        </div>

        @if (serverError) {
          <div class="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs font-bold text-right">{{ serverError }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
          
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">الاسم الأول</label>
              <input type="text" formControlName="firstName" placeholder="محمد" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right" [class.is-invalid]="form.controls.firstName.invalid && form.controls.firstName.touched">
              @if (form.controls.firstName.invalid && form.controls.firstName.touched) {
                <small class="text-rose-500 text-xs font-bold pr-1 block">الاسم الأول مطلوب</small>
              }
            </div>
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">الاسم الأخير</label>
              <input type="text" formControlName="lastName" placeholder="أحمد" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right" [class.is-invalid]="form.controls.lastName.invalid && form.controls.lastName.touched">
              @if (form.controls.lastName.invalid && form.controls.lastName.touched) {
                <small class="text-rose-500 text-xs font-bold pr-1 block">الاسم الأخير مطلوب</small>
              }
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">البريد الإلكتروني</label>
              <input type="email" formControlName="email" placeholder="your@email.com" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right" [class.is-invalid]="form.controls.email.invalid && form.controls.email.touched">
              @if (form.controls.email.invalid && form.controls.email.touched) {
                <small class="text-rose-500 text-xs font-bold pr-1 block">
                  @if (form.controls.email.errors?.['required']) { البريد الإلكتروني مطلوب }
                  @if (form.controls.email.errors?.['email']) { البريد الإلكتروني غير صحيح }
                </small>
              }
            </div>
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">رقم الهاتف</label>
              <input type="tel" formControlName="phoneNumber" placeholder="010XXXXXXXX" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right ltr" [class.is-invalid]="form.controls.phoneNumber.invalid && form.controls.phoneNumber.touched">
              @if (form.controls.phoneNumber.invalid && form.controls.phoneNumber.touched) {
                <small class="text-rose-500 text-xs font-bold pr-1 block">رقم الهاتف مطلوب</small>
              }
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">كلمة المرور</label>
              <input type="password" formControlName="password" placeholder="••••••••" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right" [class.is-invalid]="form.controls.password.invalid && form.controls.password.touched">
              @if (form.controls.password.invalid && form.controls.password.touched) {
                <small class="text-rose-500 text-xs font-bold pr-1 block">
                  @if (form.controls.password.errors?.['required']) { كلمة المرور مطلوبة }
                  @if (form.controls.password.errors?.['minlength']) { 6 أحرف على الأقل }
                </small>
              }
            </div>
            <div class="space-y-1">
              <label class="text-xs font-bold text-slate-500 dark:text-slate-400">تأكيد كلمة المرور</label>
              <input type="password" formControlName="confirmPassword" placeholder="••••••••" class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right" [class.is-invalid]="form.controls.confirmPassword.invalid && form.controls.confirmPassword.touched">
            </div>
          </div>
          @if (form.errors?.['passwordMismatch'] && form.controls.confirmPassword.touched) {
            <small class="text-rose-500 text-xs font-bold pr-1 block -mt-4">كلمة المرور غير متطابقة</small>
          }

          <!-- Account Type Toggle -->
          <div class="flex p-1 bg-slate-100 dark:bg-slate-800 rounded-xl">
            <button type="button" (click)="form.controls.userType.setValue('Customer')"
              [class]="form.controls.userType.value === 'Customer' ? 'w-1/2 py-3 bg-white dark:bg-slate-900 text-slate-900 dark:text-white shadow-md' : 'w-1/2 py-3 text-slate-500 dark:text-slate-400 hover:text-slate-950'"
              class="rounded-lg font-bold text-sm transition-all cursor-pointer">
              أنا عميل (أبحث عن صنايعي)
            </button>
            <button type="button" (click)="form.controls.userType.setValue('Worker')"
              [class]="form.controls.userType.value === 'Worker' ? 'w-1/2 py-3 bg-white dark:bg-slate-900 text-slate-900 dark:text-white shadow-md' : 'w-1/2 py-3 text-slate-500 dark:text-slate-400 hover:text-slate-950'"
              class="rounded-lg font-bold text-sm transition-all cursor-pointer">
              أنا صنايعي (أريد تقديم خدمات)
            </button>
          </div>

          <!-- Submit Button -->
          <button type="submit" class="w-full py-3.5 px-4 bg-primary hover:bg-primary-hover text-white rounded-xl shadow-lg shadow-primary/20 hover:shadow-xl font-bold transition-all text-sm cursor-pointer disabled:opacity-50" [disabled]="form.invalid || auth.loading()">
            @if (auth.loading()) {
              <span>جاري إنشاء الحساب...</span>
            } @else {
              إنشاء حساب
            }
          </button>
          
        </form>

      </div>
    </div>
  `,
  styles: [`
    .is-invalid { border-color: #f43f5e !important; }
  `]
})
export default class Register {
  private readonly fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);
  protected readonly mockData = inject(MockDataService);

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

  private readonly fieldNameMap: Record<string, string> = {
    firstName: 'firstName',
    lastName: 'lastName',
    email: 'email',
    phoneNumber: 'phoneNumber',
    password: 'password',
  };

  onSubmit(): void {
    if (this.form.invalid) return;
    this.serverError = '';
    const { confirmPassword: _, ...request } = this.form.getRawValue();

    this.auth.register(request).subscribe({
      next: (res) => {
        if (res.success) {
          this.auth.navigateByRole();
        }
      },
      error: (err: HttpErrorResponse) => {
        const errorBody = err.error as Record<string, unknown> | null;
        if (!errorBody) {
          this.serverError = err.status === 0 ? 'خطأ في الشبكة. تحقق من اتصالك.' : `خطأ في الخادم (${err.status}).`;
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
          this.serverError = messages.length > 0 ? messages.join('. ') : (errorBody['title'] as string) || 'فشل التحقق من البيانات.';
          return;
        }
        this.serverError = (errorBody['message'] as string) || (errorBody['title'] as string) || `فشل التسجيل (${err.status}).`;
      },
    });
  }
}
