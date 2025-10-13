import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { PostDetail, PostListItem } from '../models/post';
import { Category } from '../models/category';

@Injectable({ providedIn: 'root' })
export class PostsService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  /* 
    Lista de posts con búsqueda, categoría y paginación.
    GET /api/posts?q=&categoryId=&page=&pageSize=
   */
  listPosts(
    q = '',
    page = 1,
    pageSize = 5,
    categoryId?: number,
  ): Observable<PagedResult<PostListItem>> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));

    if (q?.trim()) params = params.set('q', q.trim());
    if (categoryId != null) params = params.set('categoryId', String(categoryId));

    return this.http.get<PagedResult<PostListItem>>(`${this.base}/posts`, {
      params,
    });
  }

  /*
    Detalle de un post por id.
    GET /api/posts/{id}
   */
  getPost(id: number): Observable<PostDetail> {
    return this.http.get<PostDetail>(`${this.base}/posts/${id}`);
  }

  /*
    Listado de categorías.
    GET /api/categories
   */
  listCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.base}/categories`);
  }
}
