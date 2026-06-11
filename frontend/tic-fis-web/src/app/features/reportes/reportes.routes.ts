import { Routes } from '@angular/router';

export const REPORTES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/reportes-home.component').then((m) => m.ReportesHomeComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/reporte-detalle.component').then((m) => m.ReporteDetalleComponent),
  },
];
