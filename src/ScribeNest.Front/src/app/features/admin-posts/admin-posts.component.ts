import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostsService } from '../../core/services/posts.service';
import { Category } from '../../core/models/category';
import { PostListItem, PostUpsert } from '../../core/models/post';

const emptyForm = (): PostUpsert => ({ title: '', slug: '', content: '', tags: '', categoryId: null });

@Component({
  standalone: true,
  selector: 'app-admin-posts',
  imports: [CommonModule, FormsModule],
  template: `
    <section class="mb-4">
      <p class="text-uppercase text-muted small mb-1">Content admin</p>
      <h1 class="h3">Posts</h1>
      <p class="text-muted">CRUD completo de artículos consumiendo la API REST.</p>
    </section>

    <div *ngIf="message()" class="alert alert-success">{{ message() }}</div>
    <div *ngIf="error()" class="alert alert-danger">{{ error() }}</div>

    <form class="card border-0 shadow-sm mb-4" (ngSubmit)="save()">
      <div class="card-body">
        <h2 class="h5 mb-3">{{ editingId() ? 'Editar artículo' : 'Nuevo artículo' }}</h2>
        <div class="row g-3">
          <div class="col-md-6">
            <label class="form-label">Título</label>
            <input class="form-control" name="title" [(ngModel)]="form.title" (ngModelChange)="syncSlug()" required maxlength="120" />
          </div>
          <div class="col-md-6">
            <label class="form-label">Slug automático</label>
            <input class="form-control" name="slug" [(ngModel)]="form.slug" readonly tabindex="-1" placeholder="Se genera automáticamente desde el título" />
            <div class="form-text">Se calcula desde el título y el backend lo vuelve a normalizar antes de guardar. No se edita manualmente.</div>
          </div>
          <div class="col-md-6">
            <label class="form-label">Tags</label>
            <input class="form-control" name="tags" [(ngModel)]="form.tags" placeholder=".NET, Angular, Architecture" />
            <div class="form-text">Separados por coma.</div>
          </div>
          <div class="col-md-6">
            <label class="form-label">Categoría</label>
            <select class="form-select" name="categoryId" [(ngModel)]="form.categoryId" required>
              <option [ngValue]="null">Seleccionar...</option>
              <option *ngFor="let c of categories()" [ngValue]="c.id">{{ c.name }}</option>
            </select>
          </div>
          <div class="col-12">
            <label class="form-label">Contenido</label>
            <textarea class="form-control" rows="8" name="content" [(ngModel)]="form.content" required minlength="80"></textarea>
            <div class="form-text">Mínimo recomendado: 80 caracteres. Usá texto claro para que el excerpt y la IA mock funcionen bien.</div>
          </div>
        </div>
        <div class="mt-3">
          <button class="btn btn-primary me-2" type="submit">{{ editingId() ? 'Actualizar' : 'Crear' }}</button>
          <button class="btn btn-outline-secondary" type="button" (click)="reset()" *ngIf="editingId()">Cancelar</button>
        </div>
      </div>
    </form>

    <div class="card border-0 shadow-sm">
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h2 class="h5 mb-0">Artículos existentes</h2>
          <button class="btn btn-sm btn-outline-secondary" (click)="load()">Refrescar</button>
        </div>
        <div *ngIf="loading()" class="alert alert-info">Cargando...</div>
        <table class="table align-middle mb-0 admin-table" *ngIf="!loading()">
          <thead><tr><th>Título</th><th>Categoría</th><th>Fecha</th><th class="actions-column">Acciones</th></tr></thead>
          <tbody>
            <tr *ngFor="let p of posts()">
              <td data-label="Título"><div class="fw-semibold">{{ p.title }}</div><small class="text-muted d-block">{{ p.slug }}</small><span class="badge text-bg-light me-1" *ngFor="let tag of p.tags">{{ tag }}</span></td>
              <td data-label="Categoría">{{ p.category }}</td>
              <td data-label="Fecha">{{ p.publishedAt | date: 'mediumDate' }}</td>
              <td data-label="Acciones">
                <div class="admin-actions" aria-label="Acciones del post">
                  <button class="btn btn-sm btn-outline-secondary action-btn action-edit" type="button" (click)="edit(p)" title="Editar" aria-label="Editar post">
                    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false"><path fill="currentColor" d="M4 17.3V21h3.7L18.6 10.1 14.9 6.4 4 17.3ZM20.7 8c.4-.4.4-1 0-1.4l-2.3-2.3a1 1 0 0 0-1.4 0l-1.8 1.8 3.7 3.7L20.7 8Z" /></svg>
                    <span class="visually-hidden">Editar</span>
                  </button>
                  <button class="btn btn-sm btn-outline-danger action-btn action-delete" type="button" (click)="remove(p)" title="Eliminar" aria-label="Eliminar post">
                    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false"><path fill="currentColor" d="M6 19c0 1.1.9 2 2 2h8a2 2 0 0 0 2-2V7H6v12ZM8 9h8v10H8V9Zm7.5-5-1-1h-5l-1 1H5v2h14V4h-3.5Z" /></svg>
                    <span class="visually-hidden">Eliminar</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
})
export class AdminPostsComponent implements OnInit {
  private api = inject(PostsService);
  posts = signal<PostListItem[]>([]);
  categories = signal<Category[]>([]);
  editingId = signal<number | null>(null);
  loading = signal(false);
  message = signal<string | null>(null);
  error = signal<string | null>(null);
  form: PostUpsert = emptyForm();

  ngOnInit(): void {
    this.api.listCategories().subscribe({ next: (items) => this.categories.set(items) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.listPosts('', 1, 50).subscribe({
      next: (res) => { this.posts.set(res.items); this.loading.set(false); },
      error: () => { this.error.set('No se pudieron cargar los posts.'); this.loading.set(false); },
    });
  }

  save(): void {
    this.message.set(null);
    this.error.set(null);
    if (!this.form.title.trim() || !this.form.content.trim() || !this.form.categoryId) {
      this.error.set('Título, contenido y categoría son obligatorios.');
      return;
    }

    const payload: PostUpsert = {
      title: this.form.title.trim(),
      slug: this.form.slug.trim(),
      content: this.form.content.trim(),
      tags: this.form.tags.trim(),
      categoryId: this.form.categoryId,
    };

    const request = this.editingId()
      ? this.api.updatePost(this.editingId()!, payload)
      : this.api.createPost(payload);

    request.subscribe({
      next: () => { this.message.set('Artículo guardado correctamente.'); this.reset(); this.load(); },
      error: (err) => this.error.set(this.extractError(err)),
    });
  }

  edit(p: PostListItem): void {
    this.api.getPost(p.id).subscribe({
      next: (detail) => {
        this.editingId.set(detail.id);
        this.form = { title: detail.title, slug: this.toSlug(detail.title), content: detail.content, tags: detail.tags?.join(', ') ?? '', categoryId: detail.categoryId };
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: () => this.error.set('No se pudo cargar el post para editar.'),
    });
  }

  remove(p: PostListItem): void {
    if (!confirm(`Eliminar el artículo ${p.title}?`)) return;
    this.api.deletePost(p.id).subscribe({
      next: () => { this.message.set('Artículo eliminado.'); this.load(); },
      error: () => this.error.set('No se pudo eliminar el artículo.'),
    });
  }

  reset(): void {
    this.editingId.set(null);
    this.form = emptyForm();
  }

  syncSlug(): void {
    this.form.slug = this.toSlug(this.form.title);
  }

  private toSlug(value: string): string {
    return value
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  }

  private extractError(err: any): string {
    if (typeof err?.error === 'string') return err.error;
    if (err?.error?.errors) return Object.values(err.error.errors).flat().join(' ');
    return 'No se pudo guardar el artículo.';
  }
}
