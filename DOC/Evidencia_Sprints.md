# Evidencia — Tareas por Sprint (Sprint Backlog)

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

## Introducción

Este documento conserva el detalle de las tareas planificadas en cada sprint (T01–T12)
con sus criterios de aceptación. En el cuerpo principal de la tesis, el Capítulo 2
describe los sprints de forma narrativa y resume los incrementos en la tabla "Resumen
de incrementos por sprint"; el desglose tarea por tarea se preserva aquí como respaldo
técnico que **no forma parte del documento principal de la tesis**.

---

## Sprint 1 — Consulta del listado de propuestas aprobadas

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T01 | Implementar el endpoint REST del listado de propuestas TIC | `GET /api/reportes/propuestas` retorna arreglo JSON paginado con la información de cada propuesta |
| T02 | Desarrollar la interfaz Angular del listado con guardia de autenticación JWT | La ruta `/reportes` es accesible únicamente para usuarios con token activo, con redirección al inicio de sesión en caso contrario |

## Sprint 2 — Detalle de propuesta TIC

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T03 | Implementar el endpoint `GET /propuestas/{id}` con DTO de detalle e integración del listado de estudiantes asignados | La ruta devuelve `HTTP 200` con el objeto `PropuestaReporteDetalleDto` completo, incluyendo la colección de estudiantes |
| T04 | Desarrollar la página de detalle en Angular con tolerancia a campos nulos y estructura F_AA_233A | La ruta `/reportes/:id` muestra todas las secciones del formulario sin errores ni excepciones por campos opcionales vacíos |

## Sprint 3 — Filtrado y búsqueda avanzada

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T05 | Extender el endpoint de listado con parámetros de filtrado opcionales aplicando el patrón *Specification* | El filtro de disponibilidad retorna únicamente las propuestas que correspondan; los criterios se combinan sin duplicar lógica de consulta |
| T06 | Desarrollar el componente de filtros en Angular con signals reactivos y búsqueda en memoria | El panel expone el selector de disponibilidad (Todas, Disponibles, No disponibles) y la búsqueda de texto libre por código, título o proponente; el filtrado ocurre en tiempo real sin peticiones adicionales al servidor |

## Sprint 4 — Panel de indicadores de disponibilidad

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T07 | Implementar los indicadores de disponibilidad derivados del signal de propuestas en Angular | Los indicadores calculan en memoria el total de aprobadas, disponibles, no disponibles y cupos ocupados, sin llamadas HTTP adicionales al servidor |
| T08 | Desarrollar el panel de tarjetas de disponibilidad e integrarlo con el listado y los filtros activos | Las tarjetas muestran totales actualizados al cargar el módulo y al aplicar filtros |

## Sprint 5 — Generación del reporte PDF resumen

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T09 | Implementar el generador ResumenPdf con QuestPDF y el endpoint `GET /export/pdf` con diseño institucional EPN | El endpoint retorna un PDF A4 horizontal con encabezado institucional; los filtros activos se reflejan en el contenido exportado |
| T10 | Desarrollar el botón de descarga en Angular con retroalimentación visual durante la generación del PDF | El usuario puede descargar el reporte con un solo clic desde la interfaz; el botón muestra estado de carga durante la generación |

## Sprint 6 — Exportación del formulario F_AA_233A y pruebas de integración

| Tarea | Descripción | Criterio de aceptación |
|---|---|---|
| T11 | Implementar los generadores FormulariosPdf y DetallePdf con QuestPDF y sus endpoints REST de exportación | `GET /pdf-formularios` y `GET /{id}/pdf` retornan archivos `application/pdf` con la estructura del formulario F_AA_233A |
| T12 | Realizar las pruebas de integración y validación funcional de todos los flujos del módulo | Todos los flujos principales operan correctamente de extremo a extremo bajo los distintos escenarios de uso definidos |

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, los sprints se describen de forma narrativa y se sintetizan en la tabla de
> incrementos por sprint del Capítulo 2.
