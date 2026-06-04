import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { AiSuggestionRequest, AiSuggestionResponse, PostDetail, PostListItem, PostUpsert } from '../models/post';
import { Category } from '../models/category';

@Injectable({ providedIn: 'root' })
export class PostsService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  listPosts(q = '', page = 1, pageSize = 5, categoryId?: number): Observable<PagedResult<PostListItem>> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (q?.trim()) params = params.set('q', q.trim());
    if (categoryId != null) params = params.set('categoryId', String(categoryId));
    return this.http.get<PagedResult<PostListItem>>(`${this.base}/posts`, { params });
  }

  getPost(id: number): Observable<PostDetail> {
    return this.http.get<PostDetail>(`${this.base}/posts/${id}`);
  }

  createPost(payload: PostUpsert): Observable<PostDetail> {
    return this.http.post<PostDetail>(`${this.base}/posts`, payload);
  }

  updatePost(id: number, payload: PostUpsert): Observable<void> {
    return this.http.put<void>(`${this.base}/posts/${id}`, payload);
  }

  deletePost(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/posts/${id}`);
  }

  listCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.base}/categories`);
  }

  createCategory(name: string): Observable<Category> {
    return this.http.post<Category>(`${this.base}/categories`, { name });
  }

  updateCategory(id: number, name: string): Observable<void> {
    return this.http.put<void>(`${this.base}/categories/${id}`, { name });
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/categories/${id}`);
  }

  generateAiSuggestions(payload: AiSuggestionRequest): Observable<AiSuggestionResponse> {
    return this.http.post<AiSuggestionResponse>(`${this.base}/ai-assistant/suggestions`, payload);
  }
}
