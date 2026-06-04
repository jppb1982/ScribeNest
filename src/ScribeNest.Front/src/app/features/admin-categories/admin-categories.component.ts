import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostsService } from '../../core/services/posts.service';
import { Category } from '../../core/models/category';

@Component({
  standalone: true,
  selector: 'app-admin-categories',
  imports: [CommonModule, FormsModule],
  template: `
    <section class="mb-4">
      <p class="text-uppercase text-muted small mb-1">Content admin</p>
      <h1 class="h3">Categorías</h1>
      <p class="text-muted">CRUD simple para organizar los artículos técnicos.</p>
    </section>

    <div *ngIf="message()" class="alert alert-success">{{ message() }}</div>
    <div *ngIf="error()" class="alert alert-danger">{{ error() }}</div>

    <form class="card border-0 shadow-sm mb-4" (ngSubmit)="save()">
      <div class="card-body">
        <label class="form-label">Nombre</label>
        <div class="input-group">
          <input class="form-control" name="name" [(ngModel)]="name" required maxlength="60" placeholder="Ej: Architecture" />
          <button class="btn btn-primary" type="submit">{{ editingId() ? 'Actualizar' : 'Crear' }}</button>
          <button class="btn btn-outline-secondary" type="button" (click)="reset()" *ngIf="editingId()">Cancelar</button>
        </div>
      </div>
    </form>

    <div class="card border-0 shadow-sm">
      <div class="card-body">
        <table class="table align-middle mb-0 admin-table">
          <thead><tr><th>Nombre</th><th class="actions-column">Acciones</th></tr></thead>
          <tbody>
            <tr *ngFor="let c of categories()">
              <td data-label="Nombre">{{ c.name }}</td>
              <td class="post-list-actions" data-label="Acciones">
                <div class="admin-actions compact">
                <button class="btn btn-sm btn-outline-secondary action-btn action-edit" type="button" (click)="edit(c)" title="Editar" aria-label="Editar categoría">
                  <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false"><path fill="currentColor" d="M4 17.3V21h3.7L18.6 10.1 14.9 6.4 4 17.3ZM20.7 8c.4-.4.4-1 0-1.4l-2.3-2.3a1 1 0 0 0-1.4 0l-1.8 1.8 3.7 3.7L20.7 8Z" /></svg>
                  <span class="visually-hidden">Editar</span>
                </button>
                <button class="btn btn-sm btn-outline-danger action-btn action-delete" type="button" (click)="remove(c)" title="Eliminar" aria-label="Eliminar categoría">
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
export class AdminCategoriesComponent implements OnInit {
  private api = inject(PostsService);
  categories = signal<Category[]>([]);
  editingId = signal<number | null>(null);
  message = signal<string | null>(null);
  error = signal<string | null>(null);
  name = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.listCategories().subscribe({
      next: (items) => this.categories.set(items),
      error: () => this.error.set('No se pudieron cargar las categorías.'),
    });
  }

  save(): void {
    this.message.set(null);
    this.error.set(null);
    const trimmed = this.name.trim();
    if (!trimmed) { this.error.set('El nombre es obligatorio.'); return; }

    const request = this.editingId()
      ? this.api.updateCategory(this.editingId()!, trimmed)
      : this.api.createCategory(trimmed);

    request.subscribe({
      next: () => { this.message.set('Categoría guardada correctamente.'); this.reset(); this.load(); },
      error: (err) => this.error.set(err?.error || 'No se pudo guardar la categoría.'),
    });
  }

  edit(c: Category): void {
    this.editingId.set(c.id);
    this.name = c.name;
  }

  remove(c: Category): void {
    if (!confirm(`Eliminar la categoría ${c.name}?`)) return;
    this.api.deleteCategory(c.id).subscribe({
      next: () => { this.message.set('Categoría eliminada.'); this.load(); },
      error: (err) => this.error.set(err?.error || 'No se pudo eliminar. Si tiene posts asociados, no se puede borrar.'),
    });
  }

  reset(): void {
    this.editingId.set(null);
    this.name = '';
  }
}
