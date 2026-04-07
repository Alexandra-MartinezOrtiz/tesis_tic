-- Microservicio B — Reportes Service (sección 11.4)
-- Modelo de lectura; puede alimentarse por API del servicio A en la primera versión.

CREATE TABLE propuestas_consulta (
    id BIGSERIAL PRIMARY KEY,
    propuesta_id_origen BIGINT NOT NULL,
    codigo VARCHAR(64) NOT NULL,
    titulo VARCHAR(500) NOT NULL,
    docente_nombre VARCHAR(400),
    estado_actual VARCHAR(50) NOT NULL,
    fecha_ultima_actualizacion TIMESTAMPTZ NOT NULL,
    disponible BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX ix_propuestas_consulta_codigo ON propuestas_consulta (codigo);
CREATE INDEX ix_propuestas_consulta_estado ON propuestas_consulta (estado_actual);
CREATE UNIQUE INDEX ix_propuestas_consulta_origen ON propuestas_consulta (propuesta_id_origen);

CREATE TABLE propuestas_consulta_estudiantes (
    id BIGSERIAL PRIMARY KEY,
    propuesta_consulta_id BIGINT NOT NULL REFERENCES propuestas_consulta (id) ON DELETE CASCADE,
    estudiante_nombre VARCHAR(400),
    estudiante_email VARCHAR(320)
);

CREATE INDEX ix_consulta_estudiantes_propuesta ON propuestas_consulta_estudiantes (propuesta_consulta_id);

CREATE TABLE reportes_generados (
    id BIGSERIAL PRIMARY KEY,
    tipo VARCHAR(50) NOT NULL,
    parametros_json JSONB,
    generado_por_usuario_id BIGINT NOT NULL,
    generado_en TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ruta_archivo TEXT
);

CREATE INDEX ix_reportes_generados_usuario ON reportes_generados (generado_por_usuario_id);
CREATE INDEX ix_reportes_generados_fecha ON reportes_generados (generado_en);
