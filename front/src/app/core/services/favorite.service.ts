import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/api.models';
import { FavoriteResponse, AddFavoriteRequest } from '../models/favorite.models';

@Injectable({ providedIn: 'root' })
export class FavoriteService extends BaseApiService {
  getAll(page = 1, pageSize = 10): Observable<PagedResult<FavoriteResponse>> {
    return this.get<PagedResult<FavoriteResponse>>('/favorites', { page, pageSize });
  }

  add(request: AddFavoriteRequest): Observable<FavoriteResponse> {
    return this.post<FavoriteResponse>('/favorites', request);
  }

  remove(id: number): Observable<{ message: string }> {
    return this.delete<{ message: string }>(`/favorites/${id}`);
  }
}
