import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

const authExcludedPaths = ['/auth/login', '/auth/register'];

function isAuthRequest(url: string): boolean {
  return authExcludedPaths.some((path) => url.includes(path));
}

function addTokenHeader(
  request: HttpRequest<unknown>,
  token: string,
): HttpRequest<unknown> {
  return request.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
}

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => {
  const authService = inject(AuthService);

  if (isAuthRequest(req.url)) {
    return next(req);
  }

  const token = authService.getAccessToken();
  if (token) {
    req = addTokenHeader(req, token);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthRequest(req.url)) {
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    }),
  );
};

function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((res) => {
        isRefreshing = false;
        if (res.accessToken) {
          refreshTokenSubject.next(res.accessToken);
          return next(addTokenHeader(req, res.accessToken));
        }
        authService.logout();
        return throwError(() => new Error('Token refresh failed'));
      }),
      catchError((err) => {
        isRefreshing = false;
        authService.logout();
        return throwError(() => err);
      }),
    );
  }

  return refreshTokenSubject.pipe(
    filter((token) => token !== null),
    switchMap((token) => next(addTokenHeader(req, token!))),
  );
}
