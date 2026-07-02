import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import {
  DashboardStatsResponse,
  UserManagementResponse,
  AdminUserUpdateRequest,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService extends BaseApiService {
  getDashboard(): Observable<DashboardStatsResponse> {
    return this.get<DashboardStatsResponse>('/admin/dashboard');
  }

  getUsers(page = 1, pageSize = 10, role?: string, search?: string): Observable<PagedResult<UserManagementResponse>> {
    const params: Record<string, string | number> = { page, pageSize };
    if (role) params['role'] = role;
    if (search) params['search'] = search;
    return this.get<PagedResult<UserManagementResponse>>('/admin/users', params);
  }

  getUser(id: number): Observable<UserManagementResponse> {
    return this.get<UserManagementResponse>(`/admin/users/${id}`);
  }

  updateUser(id: number, request: AdminUserUpdateRequest): Observable<{ message: string }> {
    return this.put<{ message: string }>(`/admin/users/${id}`, request);
  }

  lockUser(id: number, lockoutEnd?: string): Observable<{ message: string }> {
    const params = lockoutEnd ? `?lockoutEnd=${encodeURIComponent(lockoutEnd)}` : '';
    return this.post<{ message: string }>(`/admin/users/${id}/lock${params}`, {});
  }

  deleteUser(id: number): Observable<{ message: string }> {
    return this.delete<{ message: string }>(`/admin/users/${id}`);
  }

  exportBookings(from?: string, to?: string): Observable<Blob> {
    let url = '/admin/export/bookings';
    const params = [];
    if (from) params.push(`from=${encodeURIComponent(from)}`);
    if (to) params.push(`to=${encodeURIComponent(to)}`);
    if (params.length) url += '?' + params.join('&');
    return this.http.get(`${this.baseUrl}${url}`, { responseType: 'blob' });
  }

  exportUsers(role?: string): Observable<Blob> {
    const url = role
      ? `/admin/export/users?role=${encodeURIComponent(role)}`
      : '/admin/export/users';
    return this.http.get(`${this.baseUrl}${url}`, { responseType: 'blob' });
  }
}
