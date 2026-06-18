import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiEndpoints } from '../constants/api-endpoints';
import { PropuestaReporteItemDto, PropuestaReporteDetalleDto } from '../models/reporte.models';

@Injectable({ providedIn: 'root' })
export class ReporteService {
  private http = inject(HttpClient);
  private base = ApiEndpoints.reportes;

  getPropuestas(estado?: string, busqueda?: string, page = 1, pageSize = 100) {
    const params: Record<string, string> = { page: page.toString(), pageSize: pageSize.toString() };
    if (estado) params['estado'] = estado;
    if (busqueda) params['busqueda'] = busqueda;
    return this.http.get<PropuestaReporteItemDto[]>(`${this.base}/propuestas`, { params });
  }

  getPropuestaById(id: number) {
    return this.http.get<PropuestaReporteDetalleDto>(`${this.base}/propuestas/${id}`);
  }

  exportPdf(estado?: string, busqueda?: string) {
    const params: Record<string, string> = {};
    if (estado) params['estado'] = estado;
    if (busqueda) params['busqueda'] = busqueda;
    return this.http.get(`${this.base}/propuestas/export/pdf`, { params, responseType: 'blob' });
  }

  exportFormulariosPdf(estado?: string, busqueda?: string) {
    const params: Record<string, string> = {};
    if (estado)   params['estado']   = estado;
    if (busqueda) params['busqueda'] = busqueda;
    return this.http.get(`${this.base}/propuestas/export/pdf-formularios`, { params, responseType: 'blob' });
  }

  exportDetallePdf(id: number) {
    return this.http.get(`${this.base}/propuestas/${id}/export/pdf`, { responseType: 'blob' });
  }

}
