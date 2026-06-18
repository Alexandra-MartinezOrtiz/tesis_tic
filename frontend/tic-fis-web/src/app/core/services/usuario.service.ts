import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiEndpoints } from '../constants/api-endpoints';
import {
  UsuarioListItemDto,
  UsuarioDetalleDto,
  CreateUsuarioRequest,
  UpdateUsuarioRequest,
} from '../models/usuario.models';

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private http = inject(HttpClient);
  private base = ApiEndpoints.usuarios;

  getAll() {
    return this.http.get<UsuarioListItemDto[]>(this.base);
  }

  getById(id: number) {
    return this.http.get<UsuarioDetalleDto>(`${this.base}/${id}`);
  }

  create(request: CreateUsuarioRequest) {
    return this.http.post(this.base, request);
  }

  update(id: number, request: UpdateUsuarioRequest) {
    return this.http.put(`${this.base}/${id}`, request);
  }

  asignarRoles(id: number, rolIds: number[]) {
    return this.http.post(`${this.base}/${id}/roles`, { rolIds });
  }
}
