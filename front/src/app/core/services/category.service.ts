import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { CategoryResponse, CategoryTreeResponse, CategoryRequest } from '../models/category.models';

@Injectable({ providedIn: 'root' })
export class CategoryService extends BaseApiService {
  getAll(): Observable<CategoryResponse[]> {
    return this.get<CategoryResponse[]>('/categories');
  }

  getTree(): Observable<CategoryTreeResponse[]> {
    return this.get<CategoryTreeResponse[]>('/categories/tree');
  }

  getById(id: number): Observable<CategoryResponse> {
    return this.get<CategoryResponse>(`/categories/${id}`);
  }

  create(request: CategoryRequest): Observable<CategoryResponse> {
    return this.post<CategoryResponse>('/categories', request);
  }

  update(id: number, request: CategoryRequest): Observable<CategoryResponse> {
    return this.put<CategoryResponse>(`/categories/${id}`, request);
  }

  remove(id: number): Observable<{ message: string }> {
    return this.delete<{ message: string }>(`/categories/${id}`);
  }

  toggleActive(id: number): Observable<{ message: string }> {
    return this.patch<{ message: string }>(`/categories/${id}/toggle-active`, {});
  }
}
