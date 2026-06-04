import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PostsService } from '../../core/services/posts.service';
import { PostListItem } from '../../core/models/post';
import { Category } from '../../core/models/category';

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterLink],
  template: `
    <section class="mb-4">
      <p class="text-uppercase text-muted small mb-1">Admin dashboard</p>
      <h1 class="h3 mb-2">Panel de contenido</h1>
      <p class="text-muted mb-0">Resumen simple para mostrar pensamiento de producto, métricas y administración de contenido.</p>
    </section>

    <div *ngIf="error()" class="alert alert-danger">{{ error() }}</div>
    <div *ngIf="loading()" class="alert alert-info">Cargando métricas...</div>

    <div class="row g-3 mb-4" *ngIf="!loading()">
      <div class="col-md-4"><div class="card shadow-sm border-0"><div class="card-body"><p class="text-muted small mb-1">Posts</p><h2 class="h3 mb-0">{{ totalPosts() }}</h2></div></div></div>
      <div class="col-md-4"><div class="card shadow-sm border-0"><div class="card-body"><p class="text-muted small mb-1">Categorías</p><h2 class="h3 mb-0">{{ categories().length }}</h2></div></div></div>
      <div class="col-md-4"><div class="card shadow-sm border-0"><div class="card-body"><p class="text-muted small mb-1">Último post</p><h2 class="h6 mb-0">{{ latestPost()?.title || 'Sin datos' }}</h2></div></div></div>
    </div>

    <div class="row g-3">
      <div class="col-lg-7">
        <div class="card shadow-sm border-0">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h2 class="h5 mb-0">Últimos artículos</h2>
              <a class="btn btn-sm btn-outline-primary" routerLink="/admin/posts">Gestionar posts</a>
            </div>
            <ul class="list-group list-group-flush">
              <li class="list-group-item px-0" *ngFor="let p of posts()">
                <div class="fw-semibold">{{ p.title }}</div>
                <small class="text-muted">{{ p.category }} · {{ p.publishedAt | date: 'mediumDate' }}</small>
              </li>
            </ul>
          </div>
        </div>
      </div>
      <div class="col-lg-5">
        <div class="card shadow-sm border-0">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h2 class="h5 mb-0">Categorías</h2>
              <a class="btn btn-sm btn-outline-primary" routerLink="/admin/categories">Gestionar</a>
            </div>
            <span class="badge text-bg-light me-2 mb-2" *ngFor="let c of categories()">{{ c.name }}</span>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class AdminDashboardComponent implements OnInit {
  private api = inject(PostsService);
  posts = signal<PostListItem[]>([]);
  categories = signal<Category[]>([]);
  totalPosts = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({ posts: this.api.listPosts('', 1, 5), categories: this.api.listCategories() }).subscribe({
      next: ({ posts, categories }) => {
        this.posts.set(posts.items);
        this.totalPosts.set(posts.totalCount);
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar las métricas. Verificá que el backend esté corriendo.');
        this.loading.set(false);
      },
    });
  }

  latestPost(): PostListItem | null {
    return this.posts()[0] ?? null;
  }
}
