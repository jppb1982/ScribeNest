import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PostsService } from '../../core/services/posts.service';
import { PostDetail } from '../../core/models/post';

@Component({
  standalone: true,
  selector: 'app-post-detail',
  imports: [CommonModule, RouterLink],
  template: `
    <a class="btn btn-link p-0 mb-3" routerLink="/">← Volver</a>

    <ng-container *ngIf="post(); else loading">
      <h1 class="h3">{{ post()!.title }}</h1>
      <p class="text-muted small mb-1">
        Categoría: {{ post()!.category }} · {{ post()!.publishedAt | date: 'medium' }}
      </p>
      <hr />
      <div [innerHTML]="post()!.content"></div>
    </ng-container>

    <ng-template #loading>
      <div class="alert alert-info">Cargando...</div>
    </ng-template>
  `,
})
export class PostDetailComponent {
  private route = inject(ActivatedRoute);
  private api = inject(PostsService);

  post = signal<PostDetail | null>(null);

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (Number.isFinite(id)) {
      this.api.getPost(id).subscribe((p) => this.post.set(p));
    }
  }
}
