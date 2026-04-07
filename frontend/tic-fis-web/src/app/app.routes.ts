import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'propuestas' },
  {
    path: 'propuestas',
    loadChildren: () =>
      import('./features/propuestas/propuestas.routes').then((m) => m.PROPUESTAS_ROUTES),
  },
  {
    path: 'reportes',
    loadChildren: () =>
      import('./features/reportes/reportes.routes').then((m) => m.REPORTES_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'usuarios',
    loadChildren: () =>
      import('./features/usuarios/usuarios.routes').then((m) => m.USUARIOS_ROUTES),
  },
  { path: '**', redirectTo: 'propuestas' },
];
