export interface RolDto {
  id: number;
  nombre: string;
  descripcion?: string;
}

export interface UsuarioListItemDto {
  id: number;
  nombres: string;
  apellidos: string;
  email: string;
  activo: boolean;
  roles: string[];
}

export interface UsuarioDetalleDto {
  id: number;
  nombres: string;
  apellidos: string;
  email: string;
  activo: boolean;
  roles: RolDto[];
}

export interface CreateUsuarioRequest {
  nombres: string;
  apellidos: string;
  email: string;
  password: string;
  rolIds?: number[];
}

export interface UpdateUsuarioRequest {
  nombres: string;
  apellidos: string;
  email: string;
  activo: boolean;
}

export interface AsignarRolesRequest {
  rolIds: number[];
}
