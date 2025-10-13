import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
  categoryIdString = ''; // para el <select>

  items = signal<PostListItem[]>([]);
  categories = signal<Category[]>([]);
  total = signal<number>(0);
  page = signal<number>(1);
  pageSize = signal<number>(5);
  totalPages = signal<number>(1);
  pages = signal<number[]>([]);

  ngOnInit(): void {
    
    this.api.listCategories().subscribe((cs) => this.categories.set(cs));

    this.route.queryParamMap.subscribe((map) => {
      this.q.set(map.get('q') ?? '');
      const p = Number(map.get('page') ?? 1);
      this.page.set(Number.isFinite(p) && p >= 1 ? p : 1);

      const cat = map.get('categoryId');
      this.categoryId.set(cat ? Number(cat) : null);
      this.categoryIdString = cat ?? '';

      this.fetch(); 
    });

    // recomputa el paginador
    effect(() => {
      const tp = Math.max(1, Math.ceil(this.total() / this.pageSize()));
      this.totalPages.set(tp);
      this.pages.set(Array.from({ length: Math.min(tp, 10) }, (_, i) => i + 1));
    });
  }

  applyFilters() {
    // solo navego; fetch() lo dispara la suscripción a query params
    const queryParams: any = { page: 1 }; // resetea página
    if (this.q().trim()) queryParams.q = this.q().trim();
    if (this.categoryIdString) queryParams.categoryId = this.categoryIdString;

    this.router.navigate([], { relativeTo: this.route, queryParams });
  }

  goTo(p: number) {
    // Cambia de página si el número es válido
    if (p < 1 || p > this.totalPages()) return;
    const queryParams: any = { page: p };
    if (this.q().trim()) queryParams.q = this.q().trim();
    if (this.categoryId() != null) queryParams.categoryId = this.categoryId();
    this.router.navigate([], { relativeTo: this.route, queryParams });
  }

  // helper para [(ngModel)] con signals
  setQ(val: string) {
    this.q.set(val);
  }

  private fetch() {
    // Consulta los posts según los filtros actuales
    this.api
      .listPosts(this.q(), this.page(), this.pageSize(), this.categoryId() ?? undefined)
      .subscribe((res) => {
        this.items.set(res.items);
        this.total.set(res.totalCount);
      });
  }
}
