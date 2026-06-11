-- Microservicio A — Propuestas Service (sección 11.3)
-- Referencias usuario_id_* son lógicas al servicio Identity (sin FK cross-DB).

CREATE TABLE docentes (
    id BIGSERIAL PRIMARY KEY,
    usuario_id_referencia BIGINT NOT NULL,
    email VARCHAR(320),
    titulo VARCHAR(200),
    departamento VARCHAR(200)
);

CREATE UNIQUE INDEX ix_docentes_usuario_ref ON docentes (usuario_id_referencia);

CREATE TABLE estudiantes (
    id BIGSERIAL PRIMARY KEY,
    nombres VARCHAR(200) NOT NULL,
    apellidos VARCHAR(200) NOT NULL,
    email VARCHAR(320) NOT NULL,
    carrera VARCHAR(200)
);

CREATE INDEX ix_estudiantes_email ON estudiantes (email);

CREATE TABLE propuestas (
    id BIGSERIAL PRIMARY KEY,
    codigo VARCHAR(64) NOT NULL UNIQUE,
    titulo VARCHAR(500) NOT NULL,
    descripcion TEXT,
    problema TEXT,
    objetivo_general TEXT,
    alcance TEXT,
    docente_id BIGINT NOT NULL REFERENCES docentes (id) ON DELETE RESTRICT,
    estado_actual VARCHAR(50) NOT NULL,
    fecha_envio TIMESTAMPTZ,
    fecha_ultima_actualizacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    activa BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX ix_propuestas_docente ON propuestas (docente_id);
CREATE INDEX ix_propuestas_estado ON propuestas (estado_actual);
CREATE INDEX ix_propuestas_fecha_actualizacion ON propuestas (fecha_ultima_actualizacion);

CREATE TABLE propuesta_estudiantes (
    id BIGSERIAL PRIMARY KEY,
    propuesta_id BIGINT NOT NULL REFERENCES propuestas (id) ON DELETE CASCADE,
    estudiante_id BIGINT NOT NULL REFERENCES estudiantes (id) ON DELETE RESTRICT,
    fecha_asignacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (propuesta_id, estudiante_id)
);

CREATE TABLE propuesta_observaciones (
    id BIGSERIAL PRIMARY KEY,
    propuesta_id BIGINT NOT NULL REFERENCES propuestas (id) ON DELETE CASCADE,
    observacion TEXT NOT NULL,
    creado_por_usuario_id BIGINT NOT NULL,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_propuesta_observaciones_propuesta ON propuesta_observaciones (propuesta_id);

CREATE TABLE propuesta_historial_estados (
    id BIGSERIAL PRIMARY KEY,
    propuesta_id BIGINT NOT NULL REFERENCES propuestas (id) ON DELETE CASCADE,
    estado_anterior VARCHAR(50),
    estado_nuevo VARCHAR(50) NOT NULL,
    comentario TEXT,
    cambiado_por_usuario_id BIGINT NOT NULL,
    cambiado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_propuesta_historial_propuesta ON propuesta_historial_estados (propuesta_id);
