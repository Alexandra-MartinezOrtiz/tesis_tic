import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, NgIf],
  template: `
    <header class="topbar" *ngIf="auth.isAuthenticated()">
      <a routerLink="/propuestas" class="topbar-brand">
        <div class="brand-shield">FIS</div>
        <div class="brand-text">
          <span class="brand-title">TIC-FIS</span>
          <span class="brand-sub">EPN · Facultad de Ingeniería en Sistemas</span>
        </div>
      </a>

      <nav class="topbar-nav">
        <a routerLink="/propuestas" routerLinkActive="active">Propuestas</a>
        <a routerLink="/reportes"   routerLinkActive="active">Reportes</a>
        <a routerLink="/usuarios"   routerLinkActive="active">Usuarios</a>
      </nav>

      <div class="topbar-user">
        <span class="user-email">{{ auth.currentUserEmail() }}</span>
        <button class="btn-logout" (click)="auth.logout()">Salir</button>
      </div>
    </header>

    <main>
      <router-outlet />
    </main>
  `,
})
export class MainLayoutComponent {
  auth = inject(AuthService);
}
