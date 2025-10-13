import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
      <div class="container">
        <a
          class="navbar-brand fw-bold"
          routerLink="/"
          [routerLinkActiveOptions]="{ exact: true }"
          routerLinkActive="active"
          >ScribeNest</a
        >
        <button
          class="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#nav"
        >
          <span class="navbar-toggler-icon"></span>
        </button>
        <div id="nav" class="collapse navbar-collapse">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item">
              <a
                class="nav-link"
                routerLink="/"
                [routerLinkActiveOptions]="{ exact: true }"
                routerLinkActive="active"
                >Home</a
              >
            </li>
            <li class="nav-item">
              <a class="nav-link" routerLink="/acerca-de" routerLinkActive="active">Acerca de</a>
            </li>
          </ul>
        </div>
      </div>
    </nav>
    <main class="container my-4">
      <router-outlet></router-outlet>
    </main>
    <footer class="text-center text-muted small mb-3">© ScribeNest</footer>
  `,
})
export class AppComponent {}
