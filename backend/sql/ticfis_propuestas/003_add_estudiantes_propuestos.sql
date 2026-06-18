-- Migration 003: Add estudiantes_propuestos column to propuestas table.
-- Soporta el cálculo de cupos (X/5) y disponibilidad en el módulo de Consultas y Reportes.
-- Run once against ticfis_propuestas database if already initialized.
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS estudiantes_propuestos INTEGER NOT NULL DEFAULT 0;
