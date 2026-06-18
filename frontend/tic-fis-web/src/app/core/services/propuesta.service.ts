import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiEndpoints } from '../constants/api-endpoints';
import {
  PropuestaListItemDto,
  PropuestaDetailDto,
  CreatePropuestaRequest,
  UpdatePropuestaRequest,
  TransicionRequest,
} from '../models/propuesta.models';

@Injectable({ providedIn: 'root' })
export class PropuestaService {
  private http = inject(HttpClient);
  private base = ApiEndpoints.propuestas;

  getAll() {
    return this.http.get<PropuestaListItemDto[]>(this.base);
  }

  getById(id: number) {
    return this.http.get<PropuestaDetailDto>(`${this.base}/${id}`);
  }

  create(request: CreatePropuestaRequest) {
    return this.http.post<{ id: number }>(this.base, request);
  }

  update(id: number, request: UpdatePropuestaRequest) {
    return this.http.put(`${this.base}/${id}`, request);
  }

  enviarRevision(id: number, body: TransicionRequest = {}) {
    return this.http.post(`${this.base}/${id}/enviar-revision`, body);
  }

  aprobar(id: number, body: TransicionRequest = {}) {
    return this.http.post(`${this.base}/${id}/aprobar`, body);
  }

  rechazar(id: number, body: TransicionRequest = {}) {
    return this.http.post(`${this.base}/${id}/rechazar`, body);
  }

  pendiente(id: number, body: TransicionRequest = {}) {
    return this.http.post(`${this.base}/${id}/pendiente`, body);
  }
}
