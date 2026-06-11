6t5ty3eww-- Microservicio C — Identity Service (sección 11.2)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE roles (
    id BIGSERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(500)
);

CREATE TABLE permisos (
    id BIGSERIAL PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(500)
);

CREATE TABLE usuarios (
    id BIGSERIAL PRIMARY KEY,
    nombres VARCHAR(200) NOT NULL,
    apellidos VARCHAR(200) NOT NULL,
    email VARCHAR(320) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    actualizado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_usuarios_email ON usuarios (email);
CREATE INDEX ix_usuarios_activo ON usuarios (activo);

CREATE TABLE usuario_roles (
    id BIGSERIAL PRIMARY KEY,
    usuario_id BIGINT NOT NULL REFERENCES usuarios (id) ON DELETE CASCADE,
    rol_id BIGINT NOT NULL REFERENCES roles (id) ON DELETE CASCADE,
    UNIQUE (usuario_id, rol_id)
);

CREATE INDEX ix_usuario_roles_usuario ON usuario_roles (usuario_id);

CREATE TABLE rol_permisos (
    id BIGSERIAL PRIMARY KEY,
    rol_id BIGINT NOT NULL REFERENCES roles (id) ON DELETE CASCADE,
    permiso_id BIGINT NOT NULL REFERENCES permisos (id) ON DELETE CASCADE,
    UNIQUE (rol_id, permiso_id)
);

CREATE TABLE tokens_revocados (
    id BIGSERIAL PRIMARY KEY,
    usuario_id BIGINT NOT NULL REFERENCES usuarios (id) ON DELETE CASCADE,
    token_jti UUID NOT NULL,
    expira_en TIMESTAMPTZ NOT NULL,
    revocado_en TIMESTAMPTZ
);

CREATE INDEX ix_tokens_revocados_jti ON tokens_revocados (token_jti);
CREATE INDEX ix_tokens_revocados_usuario ON tokens_revocados (usuario_id);
