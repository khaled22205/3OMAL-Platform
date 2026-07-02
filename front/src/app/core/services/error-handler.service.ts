import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from './toast.service';

export interface ParsedApiError {
  message: string;
  errors: string[];
  statusCode: number;
}

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  private toast = inject(ToastService);

  handle(error: HttpErrorResponse): ParsedApiError {
    const parsed = this.parseError(error);

    if (error.status === 0) {
      this.toast.show('تعذر الاتصال بالخادم. تحقق من اتصالك بالإنترنت.', 'error', 5000);
    } else if (error.status >= 500) {
      this.toast.show('حدث خطأ في الخادم. الرجاء المحاولة لاحقاً.', 'error', 5000);
    } else if (error.status === 403) {
      this.toast.show('ليس لديك صلاحية للوصول إلى هذه الصفحة.', 'error', 4000);
    } else if (error.status === 404) {
      this.toast.show('البيانات المطلوبة غير موجودة.', 'warning', 3000);
    } else if (error.status === 422 || error.status === 400) {
      if (parsed.errors.length > 0) {
        this.toast.show(parsed.errors[0], 'warning', 4000);
      } else if (parsed.message) {
        this.toast.show(parsed.message, 'warning', 4000);
      }
    } else if (error.status === 409) {
      this.toast.show(parsed.message || 'تعارض في البيانات.', 'warning', 3000);
    } else if (error.status === 429) {
      this.toast.show('طلبات كثيرة جداً. الرجاء الانتظار قليلاً.', 'warning', 4000);
    } else if (parsed.message) {
      this.toast.show(parsed.message, 'error', 4000);
    }

    return parsed;
  }

  private parseError(error: HttpErrorResponse): ParsedApiError {
    const errors: string[] = [];
    let message = '';

    if (error.error instanceof ErrorEvent) {
      message = error.error.message || 'خطأ في الشبكة';
      return { message, errors: [message], statusCode: 0 };
    }

    const body = error.error;

    if (body) {
      if (typeof body === 'string') {
        message = body;
      } else if (body.message) {
        message = body.message;
      }
      if (body.errors) {
        if (Array.isArray(body.errors)) {
          errors.push(...body.errors.filter((e: unknown) => typeof e === 'string'));
        } else if (typeof body.errors === 'object') {
          for (const key of Object.keys(body.errors)) {
            const val = body.errors[key];
            if (Array.isArray(val)) {
              errors.push(...val);
            } else if (typeof val === 'string') {
              errors.push(val);
            }
          }
        }
      }
    }

    if (!message) {
      switch (error.status) {
        case 400: message = 'بيانات غير صالحة.'; break;
        case 401: message = 'يرجى تسجيل الدخول أولاً.'; break;
        case 403: message = 'ليس لديك صلاحية.'; break;
        case 404: message = 'غير موجود.'; break;
        case 409: message = 'تعارض في البيانات.'; break;
        case 422: message = 'بيانات غير صالحة.'; break;
        case 429: message = 'طلبات كثيرة.'; break;
        case 500: message = 'خطأ داخلي في الخادم.'; break;
        case 503: message = 'الخدمة غير متاحة مؤقتاً.'; break;
        default: message = `خطأ غير متوقع (${error.status}).`; break;
      }
    }

    return { message, errors, statusCode: error.status };
  }
}
