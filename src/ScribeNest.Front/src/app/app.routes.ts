// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appProviders = [provideHttpClient(withFetch())];

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'acerca-de',
    loadComponent: () => import('./features/about/about.component').then((m) => m.AboutComponent),
  },
  {
    path: 'post/:id',
    loadComponent: () =>
      import('./features/post-detail/post-detail.component').then((m) => m.PostDetailComponent),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./features/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
