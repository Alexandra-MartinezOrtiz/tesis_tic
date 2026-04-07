import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  template: `
    <header class="bar">
      <strong>TIC-FIS</strong>
      <nav>
        <a routerLink="/propuestas">Propuestas</a>
        <a routerLink="/reportes">Reportes</a>
        <a routerLink="/auth/login">Auth</a>
        <a routerLink="/usuarios">Usuarios</a>
      </nav>
    </header>
    <main class="content">
      <router-outlet />
    </main>
  `,
  styles: [
    `
      .bar {
        display: flex;
        gap: 1rem;
        align-items: center;
        padding: 0.75rem 1rem;
        border-bottom: 1px solid #ccc;
      }
      nav {
        display: flex;
        gap: 0.75rem;
      }
      .content {
        padding: 1rem;
      }
    `,
  ],
})
export class MainLayoutComponent {}
