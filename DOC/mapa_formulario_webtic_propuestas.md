# Mapeo formulario WebTIC ↔ modelo de datos (microservicio A — Propuestas)

Este documento cruza los campos del formulario institucional **F_AA_233A (Propuestas TIC / WebTIC)** con las tablas definidas en la sección **11.3** de [arquitectura_tic_fis_modulos_abc.md](arquitectura_tic_fis_modulos_abc.md).

**Nota:** Si el PDF oficial no está en el repositorio, completa la columna “Campo en formulario PDF” revisando el documento aprobado y marca obligatoriedad según el formulario.

## Identificación y titulación

| Campo en formulario PDF (completar) | Tabla.columna física | Observaciones |
|-------------------------------------|----------------------|---------------|
| Código / referencia interna | `propuestas.codigo` | Único por propuesta; generado o asignado por negocio |
| Título del trabajo | `propuestas.titulo` | |
| Docente proponente | `propuestas.docente_id` → `docentes` | En Identity existe `usuario_id`; aquí `usuario_id_referencia` enlaza con el usuario en servicio C |

## Contenido de la propuesta

| Campo en formulario PDF (completar) | Tabla.columna física | Observaciones |
|-------------------------------------|----------------------|---------------|
| Descripción general | `propuestas.descripcion` | |
| Problema | `propuestas.problema` | |
| Objetivo general | `propuestas.objetivo_general` | |
| Alcance | `propuestas.alcance` | |

## Estado y ciclo de vida

| Concepto | Tabla.columna | Observaciones |
|----------|---------------|---------------|
| Estado actual (borrador, en revisión, aprobada, etc.) | `propuestas.estado_actual` | Valores acordados como texto o enum en aplicación |
| Envío a revisión | `propuestas.fecha_envio` | |
| Última modificación | `propuestas.fecha_ultima_actualizacion` | |
| Activa / visible | `propuestas.activa` | |
| Historial de cambios de estado | `propuesta_historial_estados` | Trazabilidad CPGIC |
| Observaciones de comité | `propuesta_observaciones` | `creado_por_usuario_id` referencia identidad del revisor |

## Estudiantes asignados

| Campo en formulario PDF (completar) | Tabla | Observaciones |
|-------------------------------------|-------|---------------|
| Datos de estudiantes (nombres, correo, carrera) | `estudiantes` | Catálogo local del servicio A |
| Asignación a la propuesta | `propuesta_estudiantes` | Relación N:M con fecha |

## Campos del PDF no cubiertos aún en 11.3

Si el formulario incluye secciones adicionales (por ejemplo metodología, resultados esperados, recursos, tutor externos), **añadir columnas o tablas satélite** en el DDL de `ticfis_propuestas` **antes** de volver a ejecutar scaffold (enfoque database-first).

## Checklist de verificación

- [ ] Cada campo obligatorio del PDF tiene columna o tabla destino.
- [ ] `docentes.usuario_id_referencia` es coherente con el `id` de `usuarios` en el servicio C.
- [ ] Índices en `propuestas(codigo)`, `propuestas(docente_id)`, `propuestas(estado_actual)` para bandejas y filtros.
