-- Migration 004: Datos académicos del formulario F_AA_233A en la propuesta.
-- Agrega carrera, asignaturas seleccionadas y los campos de la solicitud de
-- participación (<2 o >5 estudiantes). Permiten reflejar en el módulo de
-- Consultas y Reportes lo que el docente llenó en el formulario.
-- Run once against ticfis_propuestas database if already initialized.
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS carrera VARCHAR(150);
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS asignaturas TEXT;
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS autorizado_por VARCHAR(200);
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS fecha_autorizacion VARCHAR(20);
