import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PostsService } from '../../core/services/posts.service';
import { PostDetail } from '../../core/models/post';

@Component({
  standalone: true,
  selector: 'app-post-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './post-detail.component.html',
  styleUrls: ['./post-detail.component.scss'],
})
export class PostDetailComponent {
  private route = inject(ActivatedRoute);
  private api = inject(PostsService);

  post = signal<PostDetail | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isFinite(id) || id <= 0) {
      this.error.set('El identificador del artículo no es válido.');
      return;
    }

    this.loading.set(true);
    this.api
      .getPost(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (p) => this.post.set(p),
        error: () => this.error.set('No se pudo cargar el artículo solicitado.'),
      });
  }
}
