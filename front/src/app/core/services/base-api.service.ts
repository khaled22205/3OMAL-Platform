import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WrappedResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BaseApiService {
  protected http = inject(HttpClient);
  protected readonly baseUrl = environment.apiUrl;

  protected get<T>(path: string, params?: HttpParams | Record<string, string | number | boolean | readonly (string | number | boolean)[]>): Observable<T> {
    return this.http
      .get<WrappedResponse<T>>(`${this.baseUrl}${path}`, { params })
      .pipe(map((r) => r.data));
  }

  protected post<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .post<WrappedResponse<T>>(`${this.baseUrl}${path}`, body)
      .pipe(map((r) => r.data));
  }

  protected put<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .put<WrappedResponse<T>>(`${this.baseUrl}${path}`, body)
      .pipe(map((r) => r.data));
  }

  protected delete<T>(path: string): Observable<T> {
    return this.http
      .delete<WrappedResponse<T>>(`${this.baseUrl}${path}`)
      .pipe(map((r) => r.data));
  }

  protected patch<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .patch<WrappedResponse<T>>(`${this.baseUrl}${path}`, body)
      .pipe(map((r) => r.data));
  }
}
