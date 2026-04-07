import { Routes } from '@angular/router';

export const PROPUESTAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/propuestas-home.component').then((m) => m.PropuestasHomeComponent),
  },
];
