import {
  HttpInterceptorFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from '../services/error-handler.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandler = inject(ErrorHandlerService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Let the auth interceptor handle 401 (token refresh) without showing a toast
      if (error.status === 401) {
        return throwError(() => error);
      }

      errorHandler.handle(error);
      return throwError(() => error);
    }),
  );
};
