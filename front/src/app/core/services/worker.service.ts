import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import {
  WorkerProfileResponse,
  WorkerProfileRequest,
  WorkerSearchRequest,
  WorkerSummaryResponse,
  WorkerStatusRequest,
  WorkerAvailabilityRequest,
  WorkerAvailabilityResponse,
  WorkerPortfolioRequest,
  WorkerPortfolioResponse,
} from '../models/worker.models';

@Injectable({ providedIn: 'root' })
export class WorkerService extends BaseApiService {
  search(request: WorkerSearchRequest): Observable<PagedResult<WorkerSummaryResponse>> {
    const params = {} as Record<string, string | number | boolean>;
    if (request.searchTerm) params['searchTerm'] = request.searchTerm;
    if (request.categoryId) params['categoryId'] = request.categoryId;
    if (request.minRating) params['minRating'] = request.minRating;
    if (request.maxPrice) params['maxPrice'] = request.maxPrice;
    if (request.city) params['city'] = request.city;
    if (request.area) params['area'] = request.area;
    if (request.minExperience) params['minExperience'] = request.minExperience;
    if (request.availableNow !== undefined) params['availableNow'] = request.availableNow;
    if (request.sortBy) params['sortBy'] = request.sortBy;
    params['page'] = request.page;
    params['pageSize'] = request.pageSize;
    return this.get<PagedResult<WorkerSummaryResponse>>('/workers/search', params);
  }

  getById(id: number): Observable<WorkerProfileResponse> {
    return this.get<WorkerProfileResponse>(`/workers/${id}`);
  }

  getMyProfile(): Observable<WorkerProfileResponse> {
    return this.get<WorkerProfileResponse>('/workers/profile');
  }

  updateProfile(request: WorkerProfileRequest): Observable<WorkerProfileResponse> {
    return this.put<WorkerProfileResponse>('/workers/profile', request);
  }

  updateAvailability(request: WorkerStatusRequest): Observable<{ message: string }> {
    return this.patch<{ message: string }>(`/workers/availability`, request);
  }

  addAvailabilitySlot(request: WorkerAvailabilityRequest): Observable<WorkerAvailabilityResponse> {
    return this.post<WorkerAvailabilityResponse>('/workers/availability/slots', request);
  }

  removeAvailabilitySlot(availabilityId: number): Observable<{ message: string }> {
    return this.delete<{ message: string }>(`/workers/availability/slots/${availabilityId}`);
  }

  addPortfolioItem(request: WorkerPortfolioRequest): Observable<WorkerPortfolioResponse> {
    return this.post<WorkerPortfolioResponse>('/workers/portfolio', request);
  }

  removePortfolioItem(portfolioItemId: number): Observable<{ message: string }> {
    return this.delete<{ message: string }>(`/workers/portfolio/${portfolioItemId}`);
  }
}
