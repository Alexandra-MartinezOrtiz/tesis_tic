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
  carrera?: string;
  asignaturas?: string;
  autorizadoPor?: string;
  fechaAutorizacion?: string;
  presentadoPor?: string;
  estudiantesNombres?: string;
  resolucionCpgic?: string;
  presidenteCpgic?: string;
  fechaAprobacion?: string;
}

export interface CreatePropuestaRequest {
  codigo: string;
  titulo: string;
  descripcion?: string;
  problema?: string;
  objetivoGeneral?: string;
  alcance?: string;
  estudiantesPropuestos?: number;
  carrera?: string;
  asignaturas?: string;
  autorizadoPor?: string;
  fechaAutorizacion?: string;
  presentadoPor?: string;
  estudiantesNombres?: string;
  resolucionCpgic?: string;
  presidenteCpgic?: string;
  fechaAprobacion?: string;
}

export interface UpdatePropuestaRequest {
  titulo: string;
  descripcion?: string;
  problema?: string;
  objetivoGeneral?: string;
  alcance?: string;
  estudiantesPropuestos?: number;
  carrera?: string;
  asignaturas?: string;
  autorizadoPor?: string;
  fechaAutorizacion?: string;
  presentadoPor?: string;
  estudiantesNombres?: string;
  resolucionCpgic?: string;
  presidenteCpgic?: string;
  fechaAprobacion?: string;
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
