# Evidencia — Base de datos de apoyo (`ticfis_reportes`)

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

## Introducción

Este documento detalla el esquema de la base de datos de apoyo `ticfis_reportes`. En el
cuerpo principal de la tesis se conservan la descripción de las tablas y el diagrama
entidad-relación; el detalle de los tipos enumerados y las vistas de consulta se traslada
aquí para reducir la extensión del documento **sin perder la evidencia técnica**. Este
material **no forma parte del documento principal de la tesis**.

---

## 1. Tablas del esquema

| Tabla | Propósito |
|---|---|
| `periodos_academicos` | Períodos académicos de referencia para filtros temporales |
| `propuestas_cache` | Caché opcional de propuestas para escenarios sin conectividad |
| `estudiantes_cache` | Caché de estudiantes vinculados a propuestas |
| `propuesta_estudiantes` | Relación N:M entre propuestas y estudiantes en caché |
| `auditoria_reportes` | Registro de cada operación de exportación realizada |

La entidad `auditoria_reportes` registra automáticamente el correo del usuario
solicitante, el formato del reporte, los filtros aplicados, el total de registros
incluidos y la duración de la operación en milisegundos.

## 2. Enumeraciones de dominio

Dos tipos enumerados nativos de PostgreSQL, que el motor valida automáticamente
rechazando cualquier valor fuera del conjunto:

- `estado_propuesta`: `borrador`, `en_revision`, `pendiente_aprobacion`, `aprobada`,
  `rechazada`.
- `formato_reporte`: `pdf`.

## 3. Vistas de consulta

Tres vistas que simplifican las consultas frecuentes del módulo:

- `v_propuestas_reporte` — listado de propuestas con el conteo de estudiantes asignados.
- `v_propuesta_detalle` — detalle completo de una propuesta incluyendo todos sus integrantes.
- `v_estadisticas_estado` — propuestas agrupadas por estado; vista de apoyo disponible en
  la base de datos para agregaciones del módulo.

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**.
