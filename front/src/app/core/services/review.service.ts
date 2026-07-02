import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import {
  ReviewResponse,
  CreateReviewRequest,
  UpdateReviewRequest,
  WorkerReplyRequest,
} from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewService extends BaseApiService {
  getWorkerReviews(workerProfileId: number, page = 1, pageSize = 10): Observable<PagedResult<ReviewResponse>> {
    return this.get<PagedResult<ReviewResponse>>(
      `/reviews/worker/${workerProfileId}?page=${page}&pageSize=${pageSize}`,
    );
  }

  getById(id: number): Observable<ReviewResponse> {
    return this.get<ReviewResponse>(`/reviews/${id}`);
  }

  create(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.post<ReviewResponse>('/reviews', request);
  }

  update(id: number, request: UpdateReviewRequest): Observable<ReviewResponse> {
    return this.put<ReviewResponse>(`/reviews/${id}`, request);
  }

  reply(id: number, request: WorkerReplyRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/reviews/${id}/reply`, request);
  }

  remove(id: number): Observable<{ message: string }> {
    return super.delete<{ message: string }>(`/reviews/${id}`);
  }
}
