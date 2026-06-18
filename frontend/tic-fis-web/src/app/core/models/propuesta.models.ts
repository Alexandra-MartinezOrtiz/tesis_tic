export interface PropuestaListItemDto {
  id: number;
  codigo: string;
  titulo: string;
  estadoActual: string;
  fechaUltimaActualizacion: string;
  activa: boolean;
  estudiantesPropuestos?: number;
}

export interface EstudianteAsignadoDto {
  estudianteId: number;
  nombreCompleto: string;
  email: string;
  fechaAsignacion: string;
}

export interface ObservacionDto {
  id: number;
  texto: string;
  creadoPorUsuarioId: number;
  creadoEn: string;
}

export interface PropuestaDetailDto {
  id: number;
  codigo: string;
  titulo: string;
  descripcion?: string;
  problema?: string;
  objetivoGeneral?: string;
  alcance?: string;
  docenteId: number;
  docenteUsuarioIdReferencia: number;
  estadoActual: string;
  fechaEnvio?: string;
  fechaUltimaActualizacion: string;
  activa: boolean;
  estudiantesPropuestos?: number;
  estudiantes: EstudianteAsignadoDto[];
  observaciones: ObservacionDto[];
}

export interface CreatePropuestaRequest {
  codigo: string;
  titulo: string;
  descripcion?: string;
  problema?: string;
  objetivoGeneral?: string;
  alcance?: string;
  estudiantesPropuestos?: number;
}

export interface UpdatePropuestaRequest {
  titulo: string;
  descripcion?: string;
  problema?: string;
  objetivoGeneral?: string;
  alcance?: string;
  estudiantesPropuestos?: number;
}

export interface TransicionRequest {
  comentario?: string;
}

export const ESTADOS_PROPUESTA = {
  Borrador: 'Borrador',
  EnRevision: 'EnRevision',
  Aprobada: 'Aprobada',
  Rechazada: 'Rechazada',
  Pendiente: 'Pendiente',
} as const;

export type EstadoPropuesta = keyof typeof ESTADOS_PROPUESTA;
