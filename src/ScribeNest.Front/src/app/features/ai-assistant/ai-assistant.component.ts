import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostsService } from '../../core/services/posts.service';
import { AiSuggestionResponse } from '../../core/models/post';

@Component({
  standalone: true,
  selector: 'app-ai-assistant',
  imports: [CommonModule, FormsModule],
  template: `
    <section class="mb-4">
      <p class="text-uppercase text-muted small mb-1">AI-inspired feature</p>
      <h1 class="h3">Asistente de contenido IA Mock</h1>
      <p class="text-muted">Evalúa consistencia editorial y genera sugerencias locales de resumen, excerpt y tags. No consume APIs pagas y está desacoplado para reemplazarlo por un proveedor real.</p>
    </section>

    <div *ngIf="error()" class="alert alert-danger">{{ error() }}</div>

    <div class="row g-4">
      <div class="col-lg-6">
        <form class="card border-0 shadow-sm" (ngSubmit)="generate()">
          <div class="card-body">
            <label class="form-label">Título</label>
            <input class="form-control mb-3" name="title" [(ngModel)]="title" required />
            <label class="form-label">Contenido</label>
            <textarea class="form-control mb-3" rows="10" name="content" [(ngModel)]="content" required></textarea>
            <button class="btn btn-primary" type="submit" [disabled]="loading()">{{ loading() ? 'Generando...' : 'Generar sugerencias' }}</button>
          </div>
        </form>
      </div>

      <div class="col-lg-6">
        <div class="card border-0 shadow-sm" *ngIf="result(); else empty">
          <div class="card-body">
            <h2 class="h5">Resultado</h2>
            <div [class]="result()!.warnings?.length ? 'alert alert-warning' : 'alert alert-success'">
              <strong>Consistencia editorial: {{ result()!.editorialConsistency || (result()!.warnings?.length ? 'Media' : 'Alta') }}</strong>
              <ul class="mb-0 mt-2" *ngIf="(result()!.editorialNotes?.length || result()!.warnings?.length)">
                <li *ngFor="let note of (result()!.editorialNotes?.length ? result()!.editorialNotes : result()!.warnings)">{{ note }}</li>
              </ul>
            </div>
            <p class="mb-1 text-muted small">Resumen</p>
            <p>{{ result()!.summary }}</p>
            <p class="mb-1 text-muted small">Excerpt</p>
            <p>{{ result()!.excerpt }}</p>
            <p class="mb-1 text-muted small">Tags sugeridos por IA</p>
            <span class="badge text-bg-light me-2" *ngFor="let tag of result()!.suggestedTags">{{ tag }}</span>
            <p class="small text-muted mt-2">La IA no modifica tags automáticamente; solo sugiere cambios.</p>
            <p class="mb-1 mt-3 text-muted small">Explain for juniors</p>
            <p>{{ result()!.explainForJuniors }}</p>
          </div>
        </div>
        <ng-template #empty>
          <div class="alert alert-secondary">Completá el formulario para generar sugerencias.</div>
        </ng-template>
      </div>
    </div>
  `,
})
export class AiAssistantComponent {
  private api = inject(PostsService);
  title = 'Consuming a .NET API from Angular';
  content = 'Angular communicates with the backend through an HTTP service. This keeps components focused on presentation and delegates API calls to a reusable service. The backend exposes paginated endpoints with filters and DTOs.';
  result = signal<AiSuggestionResponse | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  generate(): void {
    this.error.set(null);
    this.result.set(null);
    if (!this.title.trim() || !this.content.trim()) {
      this.error.set('Título y contenido son obligatorios.');
      return;
    }
    this.loading.set(true);
    this.api.generateAiSuggestions({ title: this.title, content: this.content, category: '', tags: '' }).subscribe({
      next: (res) => { this.result.set(res); this.loading.set(false); },
      error: () => { this.error.set('No se pudieron generar sugerencias. Verificá que el backend esté corriendo.'); this.loading.set(false); },
    });
  }
}
