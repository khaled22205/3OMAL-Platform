import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import { ServiceRequest, ServiceResponse } from '../models/service.models';

@Injectable({ providedIn: 'root' })
export class WorkerServiceService extends BaseApiService {
  search(searchTerm?: string, categoryId?: number, page = 1, pageSize = 10): Observable<PagedResult<ServiceResponse>> {
    const params: Record<string, string | number> = { page, pageSize };
    if (searchTerm) params['searchTerm'] = searchTerm;
    if (categoryId) params['categoryId'] = categoryId;
    return this.get<PagedResult<ServiceResponse>>('/services', params);
  }

  getById(id: number): Observable<ServiceResponse> {
    return this.get<ServiceResponse>(`/services/${id}`);
  }

  getByWorker(workerProfileId: number): Observable<ServiceResponse[]> {
    return this.get<ServiceResponse[]>(`/services/worker/${workerProfileId}`);
  }

  create(request: ServiceRequest): Observable<ServiceResponse> {
    return this.post<ServiceResponse>('/services', request);
  }

  update(id: number, request: ServiceRequest): Observable<ServiceResponse> {
    return this.put<ServiceResponse>(`/services/${id}`, request);
  }

  remove(id: number): Observable<{ message: string }> {
    return super.delete<{ message: string }>(`/services/${id}`);
  }

  toggleActive(id: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.baseUrl}/services/${id}/toggle-active`, {});
  }
}
