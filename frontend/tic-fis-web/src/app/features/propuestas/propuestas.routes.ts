import { Routes } from '@angular/router';

export const PROPUESTAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/propuestas-home.component').then((m) => m.PropuestasHomeComponent),
  },
  {
    path: 'nueva',
    loadComponent: () =>
      import('./pages/propuesta-form.component').then((m) => m.PropuestaFormComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/propuesta-detalle.component').then((m) => m.PropuestaDetalleComponent),
  },
  {
    path: ':id/editar',
    loadComponent: () =>
      import('./pages/propuesta-form.component').then((m) => m.PropuestaFormComponent),
  },
];
