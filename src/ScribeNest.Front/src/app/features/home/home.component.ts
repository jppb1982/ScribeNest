import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PostsService } from '../../core/services/posts.service';
import { PostListItem } from '../../core/models/post';
import { Category } from '../../core/models/category';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  private api = inject(PostsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  q = signal<string>('');
  categoryId = signal<number | null>(null);
  categoryIdString = '';

  items = signal<PostListItem[]>([]);
  categories = signal<Category[]>([]);
  total = signal<number>(0);
  page = signal<number>(1);
  pageSize = signal<number>(5);
  totalPages = signal<number>(1);
  pages = signal<number[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.listCategories().subscribe({
      next: (cs) => this.categories.set(cs),
      error: () => this.error.set('No se pudieron cargar las categorías.'),
    });

    this.route.queryParamMap.subscribe((map) => {
      this.q.set(map.get('q') ?? '');
      const p = Number(map.get('page') ?? 1);
      this.page.set(Number.isFinite(p) && p >= 1 ? p : 1);

      const cat = map.get('categoryId');
      this.categoryId.set(cat ? Number(cat) : null);
      this.categoryIdString = cat ?? '';

      this.fetch();
    });

  }

  private updatePagination() {
    const tp = Math.max(1, Math.ceil(this.total() / this.pageSize()));
    this.totalPages.set(tp);
    this.pages.set(Array.from({ length: Math.min(tp, 10) }, (_, i) => i + 1));
  }

  applyFilters() {
    const queryParams: Record<string, string | number> = { page: 1 };
    if (this.q().trim()) queryParams['q'] = this.q().trim();
    if (this.categoryIdString) queryParams['categoryId'] = this.categoryIdString;

    this.router.navigate([], { relativeTo: this.route, queryParams });
  }

  goTo(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    const queryParams: Record<string, string | number> = { page: p };
    if (this.q().trim()) queryParams['q'] = this.q().trim();
    if (this.categoryId() != null) queryParams['categoryId'] = this.categoryId()!;
    this.router.navigate([], { relativeTo: this.route, queryParams });
  }

  clearFilters() {
    this.q.set('');
    this.categoryId.set(null);
    this.categoryIdString = '';
    this.router.navigate([], { relativeTo: this.route, queryParams: { page: 1 } });
  }

  setQ(val: string) {
    this.q.set(val);
  }

  private fetch() {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .listPosts(this.q(), this.page(), this.pageSize(), this.categoryId() ?? undefined)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.items.set(res.items);
          this.total.set(res.totalCount);
          this.updatePagination();
        },
        error: () => {
          this.items.set([]);
          this.total.set(0);
          this.updatePagination();
          this.error.set('No se pudieron cargar los artículos. Verificá que el backend esté corriendo.');
        },
      });
  }
}
