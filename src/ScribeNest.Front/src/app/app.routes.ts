import { Routes } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appProviders = [provideHttpClient(withFetch())];

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent) },
  { path: 'admin', loadComponent: () => import('./features/admin-dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent) },
  { path: 'admin/posts', loadComponent: () => import('./features/admin-posts/admin-posts.component').then((m) => m.AdminPostsComponent) },
  { path: 'admin/categories', loadComponent: () => import('./features/admin-categories/admin-categories.component').then((m) => m.AdminCategoriesComponent) },
  { path: 'ai-assistant', loadComponent: () => import('./features/ai-assistant/ai-assistant.component').then((m) => m.AiAssistantComponent) },
  { path: 'acerca-de', loadComponent: () => import('./features/about/about.component').then((m) => m.AboutComponent) },
  { path: 'post/:id', loadComponent: () => import('./features/post-detail/post-detail.component').then((m) => m.PostDetailComponent) },
  { path: '**', loadComponent: () => import('./features/not-found/not-found.component').then((m) => m.NotFoundComponent) },
];
