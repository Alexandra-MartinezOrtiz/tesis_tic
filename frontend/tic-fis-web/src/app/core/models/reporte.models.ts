export interface PropuestaReporteItemDto {
  id: number;
  codigo: string;
  titulo: string;
  estadoActual: string;
  fechaUltimaActualizacion: string;
  activa: boolean;
  docenteEmail?: string;
  estudiantesPropuestos: number;
  cupoMaximo: number;
  disponible: boolean;
}

export interface EstudianteReporteDto {
  nombreCompleto: string;
  email: string;
  fechaAsignacion: string;
}

export interface PropuestaReporteDetalleDto {
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
  estudiantesPropuestos: number;
  cupoMaximo: number;
  disponible: boolean;
  estudiantes: EstudianteReporteDto[];
}

export interface FiltrosReporte {
  estado?: string;
  busqueda?: string;
}
