-- =============================================================
-- MÓDULO: CONSULTAS Y REPORTES
-- Base de datos: ticfis_reportes
-- Motor: PostgreSQL 16
-- Proyecto: TIC-FIS — Trabajo de Integración Curricular EPN/FIS
-- Autor: Módulo Consultas y Reportes
-- =============================================================

-- -------------------------------------------------------------
-- 1. CREAR BASE DE DATOS
-- -------------------------------------------------------------
-- Ejecutar como superusuario antes de conectarse a la BD:
-- CREATE DATABASE ticfis_reportes
--     WITH OWNER = postgres
--     ENCODING = 'UTF8'
--     LC_COLLATE = 'en_US.UTF-8'
--     LC_CTYPE = 'en_US.UTF-8'
--     TEMPLATE = template0;

-- Conectarse a ticfis_reportes y ejecutar el resto:

-- -------------------------------------------------------------
-- 2. EXTENSIONES
-- -------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";   -- búsqueda por similitud de texto

-- -------------------------------------------------------------
-- 3. ENUMERACIONES
-- -------------------------------------------------------------
DO $$ BEGIN
    CREATE TYPE estado_propuesta AS ENUM (
        'borrador',
        'en_revision',
        'pendiente_aprobacion',
        'aprobada',
        'rechazada'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE formato_reporte AS ENUM ('pdf', 'excel');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- -------------------------------------------------------------
-- 4. TABLA: periodos_academicos
-- Períodos académicos de referencia para filtrar reportes.
-- El módulo de Propuestas gestiona los datos; Reportes los
-- replica aquí para construir filtros sin depender del servicio.
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS periodos_academicos (
    periodo_id      BIGINT          PRIMARY KEY,
    nombre          VARCHAR(100)    NOT NULL,
    fecha_inicio    DATE            NOT NULL,
    fecha_fin       DATE            NOT NULL,
    estado          VARCHAR(20)     NOT NULL DEFAULT 'activo',
    sincronizado_en TIMESTAMPTZ     NOT NULL DEFAULT now()
);

COMMENT ON TABLE periodos_academicos IS
    'Copia local de períodos académicos sincronizada desde el servicio de Propuestas.';

-- -------------------------------------------------------------
-- 5. TABLA: propuestas_cache
-- Caché de propuestas leídas desde el servicio de Propuestas.
-- Permite generar reportes sin depender de disponibilidad del
-- servicio externo en tiempo real.
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS propuestas_cache (
    propuesta_id            BIGINT          PRIMARY KEY,
    codigo                  VARCHAR(50)     NOT NULL,
    titulo                  VARCHAR(300)    NOT NULL,
    resumen                 TEXT,
    problema                TEXT,
    justificacion           TEXT,
    objetivos               TEXT,
    alcance                 TEXT,
    metodologia             TEXT,
    resultados_esperados    TEXT,
    estado_actual           estado_propuesta NOT NULL,
    periodo_id              BIGINT          REFERENCES periodos_academicos(periodo_id),
    docente_email           VARCHAR(150),
    activa                  BOOLEAN         NOT NULL DEFAULT TRUE,
    fecha_creacion          TIMESTAMPTZ,
    fecha_ultima_actualizacion TIMESTAMPTZ,
    sincronizado_en         TIMESTAMPTZ     NOT NULL DEFAULT now()
);

COMMENT ON TABLE propuestas_cache IS
    'Caché local de propuestas. Se sincroniza desde el servicio de Propuestas '
    'al generar reportes o via tarea programada.';

CREATE INDEX IF NOT EXISTS idx_propuestas_cache_estado
    ON propuestas_cache (estado_actual);

CREATE INDEX IF NOT EXISTS idx_propuestas_cache_periodo
    ON propuestas_cache (periodo_id);

CREATE INDEX IF NOT EXISTS idx_propuestas_cache_activa
    ON propuestas_cache (activa);

-- Índice para búsqueda de texto en código y título
CREATE INDEX IF NOT EXISTS idx_propuestas_cache_texto
    ON propuestas_cache USING gin (
        (codigo || ' ' || titulo) gin_trgm_ops
    );

-- -------------------------------------------------------------
-- 6. TABLA: estudiantes_cache
-- Caché de estudiantes asignados a propuestas.
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS estudiantes_cache (
    estudiante_id   BIGINT          PRIMARY KEY,
    identificacion  VARCHAR(20)     NOT NULL,
    nombres         VARCHAR(100)    NOT NULL,
    apellidos       VARCHAR(100)    NOT NULL,
    correo          VARCHAR(150),
    sincronizado_en TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- -------------------------------------------------------------
-- 7. TABLA: propuesta_estudiantes
-- Relación propuesta ↔ estudiante (many-to-many).
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS propuesta_estudiantes (
    propuesta_id    BIGINT  NOT NULL REFERENCES propuestas_cache(propuesta_id) ON DELETE CASCADE,
    estudiante_id   BIGINT  NOT NULL REFERENCES estudiantes_cache(estudiante_id) ON DELETE CASCADE,
    rol_asignado    VARCHAR(50),
    fecha_asignacion TIMESTAMPTZ,
    PRIMARY KEY (propuesta_id, estudiante_id)
);

CREATE INDEX IF NOT EXISTS idx_propuesta_estudiantes_propuesta
    ON propuesta_estudiantes (propuesta_id);

-- -------------------------------------------------------------
-- 8. TABLA: auditoria_reportes
-- Registro de cada reporte generado: quién, cuándo, con qué
-- filtros y en qué formato.
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS auditoria_reportes (
    auditoria_id    BIGSERIAL       PRIMARY KEY,
    usuario_email   VARCHAR(150)    NOT NULL,
    formato         formato_reporte NOT NULL,
    filtro_estado   estado_propuesta,
    filtro_busqueda VARCHAR(200),
    filtro_periodo  BIGINT,
    total_registros INTEGER         NOT NULL DEFAULT 0,
    generado_en     TIMESTAMPTZ     NOT NULL DEFAULT now(),
    duracion_ms     INTEGER                          -- tiempo de generación en ms
);

COMMENT ON TABLE auditoria_reportes IS
    'Log de todos los reportes PDF/Excel generados con sus filtros aplicados.';

CREATE INDEX IF NOT EXISTS idx_auditoria_reportes_usuario
    ON auditoria_reportes (usuario_email);

CREATE INDEX IF NOT EXISTS idx_auditoria_reportes_fecha
    ON auditoria_reportes (generado_en DESC);

-- -------------------------------------------------------------
-- 9. VISTAS
-- -------------------------------------------------------------

-- Vista principal para el listado de propuestas con filtros
CREATE OR REPLACE VIEW v_propuestas_reporte AS
SELECT
    p.propuesta_id,
    p.codigo,
    p.titulo,
    p.estado_actual,
    p.docente_email,
    p.activa,
    p.fecha_creacion,
    p.fecha_ultima_actualizacion,
    pa.nombre                           AS periodo_nombre,
    COUNT(pe.estudiante_id)             AS total_estudiantes
FROM propuestas_cache p
LEFT JOIN periodos_academicos pa ON pa.periodo_id = p.periodo_id
LEFT JOIN propuesta_estudiantes pe ON pe.propuesta_id = p.propuesta_id
GROUP BY
    p.propuesta_id, p.codigo, p.titulo, p.estado_actual,
    p.docente_email, p.activa, p.fecha_creacion,
    p.fecha_ultima_actualizacion, pa.nombre;

COMMENT ON VIEW v_propuestas_reporte IS
    'Vista usada por el módulo de reportes para listar propuestas con conteo de estudiantes.';

-- Vista de detalle de propuesta con estudiantes
CREATE OR REPLACE VIEW v_propuesta_detalle AS
SELECT
    p.propuesta_id,
    p.codigo,
    p.titulo,
    p.resumen,
    p.problema,
    p.justificacion,
    p.objetivos,
    p.alcance,
    p.metodologia,
    p.resultados_esperados,
    p.estado_actual,
    p.docente_email,
    p.fecha_creacion,
    p.fecha_ultima_actualizacion,
    pa.nombre                           AS periodo_nombre,
    e.estudiante_id,
    e.identificacion,
    e.nombres,
    e.apellidos,
    e.correo                            AS correo_estudiante,
    pe.rol_asignado,
    pe.fecha_asignacion
FROM propuestas_cache p
LEFT JOIN periodos_academicos pa ON pa.periodo_id = p.periodo_id
LEFT JOIN propuesta_estudiantes pe ON pe.propuesta_id = p.propuesta_id
LEFT JOIN estudiantes_cache e ON e.estudiante_id = pe.estudiante_id;

COMMENT ON VIEW v_propuesta_detalle IS
    'Vista de detalle completo: propuesta + todos sus estudiantes asignados.';

-- Vista de estadísticas por estado (para los contadores del dashboard)
CREATE OR REPLACE VIEW v_estadisticas_estado AS
SELECT
    estado_actual,
    COUNT(*) AS total
FROM propuestas_cache
WHERE activa = TRUE
GROUP BY estado_actual;

COMMENT ON VIEW v_estadisticas_estado IS
    'Contadores de propuestas activas agrupadas por estado para el dashboard.';

-- -------------------------------------------------------------
-- 10. FUNCIÓN: registrar_auditoria
-- Registra automáticamente cada reporte generado.
-- -------------------------------------------------------------
CREATE OR REPLACE FUNCTION registrar_auditoria_reporte(
    p_usuario_email     VARCHAR,
    p_formato           formato_reporte,
    p_filtro_estado     estado_propuesta DEFAULT NULL,
    p_filtro_busqueda   VARCHAR DEFAULT NULL,
    p_filtro_periodo    BIGINT DEFAULT NULL,
    p_total_registros   INTEGER DEFAULT 0,
    p_duracion_ms       INTEGER DEFAULT NULL
)
RETURNS BIGINT
LANGUAGE plpgsql
AS $$
DECLARE
    v_id BIGINT;
BEGIN
    INSERT INTO auditoria_reportes (
        usuario_email, formato, filtro_estado, filtro_busqueda,
        filtro_periodo, total_registros, duracion_ms
    ) VALUES (
        p_usuario_email, p_formato, p_filtro_estado, p_filtro_busqueda,
        p_filtro_periodo, p_total_registros, p_duracion_ms
    ) RETURNING auditoria_id INTO v_id;

    RETURN v_id;
END;
$$;

COMMENT ON FUNCTION registrar_auditoria_reporte IS
    'Registra en auditoria_reportes cada vez que se genera un PDF o Excel.';

-- -------------------------------------------------------------
-- 11. DATOS INICIALES DE PRUEBA
-- -------------------------------------------------------------
INSERT INTO periodos_academicos (periodo_id, nombre, fecha_inicio, fecha_fin, estado)
VALUES
    (1, '2026-A', '2026-01-01', '2026-06-30', 'activo'),
    (2, '2025-B', '2025-07-01', '2025-12-31', 'cerrado')
ON CONFLICT (periodo_id) DO NOTHING;

INSERT INTO propuestas_cache (
    propuesta_id, codigo, titulo, estado_actual, activa,
    fecha_creacion, fecha_ultima_actualizacion
) VALUES
    (1, 'gergreg', 'grhrhrh', 'borrador', TRUE, now(), now()),
    (2, 'GVDFGDF', 'DGDFG',  'borrador', TRUE, now(), now())
ON CONFLICT (propuesta_id) DO NOTHING;

-- -------------------------------------------------------------
-- FIN DEL SCRIPT
-- =============================================================
-- NOTAS PARA EL EQUIPO:
--
-- Este script crea la BD del módulo CONSULTAS Y REPORTES.
-- Este módulo NO modifica datos de propuestas ni de usuarios.
-- Solo LEE desde el servicio de Propuestas vía HTTP y genera
-- reportes PDF/Excel.
--
-- Integración con otros módulos:
--   - Propuestas Service → comunica vía HTTP (puerto 5002)
--   - Identity Service  → valida JWT en cada petición (puerto 5001)
--   - API Gateway       → enruta /api/reportes/* a este servicio (puerto 5000)
--
-- Credenciales locales de desarrollo:
--   Host: localhost | Port: 5432
--   Database: ticfis_reportes
--   User: postgres | Password: postgres
-- =============================================================
