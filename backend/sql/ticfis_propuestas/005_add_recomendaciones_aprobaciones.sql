-- Migration 005: Sección "Recomendaciones y aprobaciones" del formulario F_AA_233A.
-- Campos editables en el formulario (presentado por, estudiantes propuestos por nombre,
-- resolución / presidente / fecha de aprobación de la CPGIC) que se reflejan en Reportes.
-- Run once against ticfis_propuestas database if already initialized.
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS presentado_por VARCHAR(200);
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS estudiantes_nombres TEXT;
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS resolucion_cpgic TEXT;
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS presidente_cpgic VARCHAR(200);
ALTER TABLE propuestas ADD COLUMN IF NOT EXISTS fecha_aprobacion VARCHAR(20);
