import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'propuestas' },
  {
    path: 'propuestas',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/propuestas/propuestas.routes').then((m) => m.PROPUESTAS_ROUTES),
  },
  {
    path: 'reportes',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/reportes/reportes.routes').then((m) => m.REPORTES_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'usuarios',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/usuarios/usuarios.routes').then((m) => m.USUARIOS_ROUTES),
  },
  { path: '**', redirectTo: 'propuestas' },
];
