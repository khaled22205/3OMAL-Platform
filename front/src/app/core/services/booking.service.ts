import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import {
  BookingResponse,
  CreateBookingRequest,
  BookingFilterRequest,
} from '../models/booking.models';

@Injectable({ providedIn: 'root' })
export class BookingService extends BaseApiService {
  getById(id: number): Observable<BookingResponse> {
    return this.get<BookingResponse>(`/bookings/${id}`);
  }

  getMyBookings(filter: BookingFilterRequest): Observable<PagedResult<BookingResponse>> {
    const params: Record<string, string | number> = { page: filter.page, pageSize: filter.pageSize };
    if (filter.status) params['status'] = filter.status;
    if (filter.fromDate) params['fromDate'] = filter.fromDate;
    if (filter.toDate) params['toDate'] = filter.toDate;
    return this.get<PagedResult<BookingResponse>>('/bookings/my', params);
  }

  getWorkerBookings(workerProfileId: number, filter: BookingFilterRequest): Observable<PagedResult<BookingResponse>> {
    const params: Record<string, string | number> = { page: filter.page, pageSize: filter.pageSize };
    if (filter.status) params['status'] = filter.status;
    if (filter.fromDate) params['fromDate'] = filter.fromDate;
    if (filter.toDate) params['toDate'] = filter.toDate;
    return this.get<PagedResult<BookingResponse>>(`/bookings/worker/${workerProfileId}`, params);
  }

  create(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.post<BookingResponse>('/bookings', request);
  }

  accept(id: number): Observable<BookingResponse> {
    return this.patch<BookingResponse>(`/bookings/${id}/accept`, {});
  }

  reject(id: number, reason?: string): Observable<BookingResponse> {
    const path = reason ? `/bookings/${id}/reject?reason=${encodeURIComponent(reason)}` : `/bookings/${id}/reject`;
    return this.patch<BookingResponse>(path, {});
  }

  cancel(id: number, reason?: string): Observable<BookingResponse> {
    const path = reason ? `/bookings/${id}/cancel?reason=${encodeURIComponent(reason)}` : `/bookings/${id}/cancel`;
    return this.patch<BookingResponse>(path, {});
  }

  reschedule(id: number, newScheduledAt: string): Observable<BookingResponse> {
    return this.patch<BookingResponse>(
      `/bookings/${id}/reschedule?newScheduledAt=${encodeURIComponent(newScheduledAt)}`,
      {},
    );
  }

  markOnTheWay(id: number): Observable<BookingResponse> {
    return this.patch<BookingResponse>(`/bookings/${id}/on-the-way`, {});
  }

  startJob(id: number): Observable<BookingResponse> {
    return this.patch<BookingResponse>(`/bookings/${id}/start`, {});
  }

  pauseJob(id: number): Observable<BookingResponse> {
    return this.patch<BookingResponse>(`/bookings/${id}/pause`, {});
  }

  completeJob(id: number): Observable<BookingResponse> {
    return this.patch<BookingResponse>(`/bookings/${id}/complete`, {});
  }
}
