-- Migration 002: Add email column to docentes table
-- Run once against ticfis_propuestas database if already initialized.
ALTER TABLE docentes ADD COLUMN IF NOT EXISTS email VARCHAR(320);
CREATE INDEX IF NOT EXISTS ix_docentes_email ON docentes (email);
