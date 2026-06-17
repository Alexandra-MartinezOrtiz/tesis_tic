# Anexo — Historias de Usuario completas

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

## Introducción

Este documento conserva el detalle completo de las seis historias de usuario (HU01–HU06)
que orientaron el desarrollo del Módulo de Consultas y Reportes. En el cuerpo principal
de la tesis se incluye únicamente una **tabla resumen** con la trazabilidad
historia → sprint → funcionalidad; aquí se preserva la versión íntegra de cada historia
—descripción, criterios de aceptación y observaciones— como respaldo técnico que **no
forma parte del documento principal de la tesis**.

Las historias se definieron siguiendo el formato propuesto por Scrum, describiendo cada
requerimiento desde la perspectiva del usuario final.

---

## HU01 — Consulta del listado de propuestas TIC aprobadas

| Campo | Valor |
|---|---|
| **Código** | HU01 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Medio |
| **Puntos estimados** | 5 |
| **Iteración asignada** | Sprint 1 |

**Descripción:** Como usuario autenticado, quiero visualizar el listado de propuestas de
Trabajo de Integración Curricular (TIC) gestionadas por la CPGIC, para consultar su
estado, proponente y acceder al detalle de cada una desde la interfaz del sistema.

**Criterios de aceptación:**
- El listado muestra código, título, estado, proponente y fecha de última actualización.
- El usuario puede navegar al detalle de cualquier propuesta seleccionada.
- La ruta del módulo está protegida y redirige al inicio de sesión si no existe token activo.

**Observaciones:**
- La información se obtiene en tiempo real desde el servicio de Propuestas mediante API Composition.

---

## HU02 — Consulta del detalle de una propuesta TIC

| Campo | Valor |
|---|---|
| **Código** | HU02 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Medio |
| **Puntos estimados** | 5 |
| **Iteración asignada** | Sprint 2 |

**Descripción:** Como usuario autenticado, quiero consultar el detalle completo de una
propuesta TIC, incluyendo descripción, alcance, datos del docente proponente y
estudiantes asignados, para revisar su información en profundidad sin necesidad de
recurrir a documentos físicos.

**Criterios de aceptación:**
- El sistema muestra todos los campos del formulario F_AA_233A para la propuesta seleccionada.
- La información del docente proponente y los estudiantes asignados se presenta correctamente.
- El sistema no lanza excepción cuando la propuesta tiene campos opcionales vacíos.

**Observaciones:**
- La página de detalle es accesible mediante la ruta `/reportes/:id` sin recargar la aplicación.

---

## HU03 — Filtrado y búsqueda de propuestas TIC

| Campo | Valor |
|---|---|
| **Código** | HU03 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Medio |
| **Puntos estimados** | 5 |
| **Iteración asignada** | Sprint 3 |

**Descripción:** Como usuario autenticado, quiero filtrar y buscar propuestas TIC por
estado, código, título o nombre del docente, para localizar registros específicos de
manera rápida y eficiente sin revisar el catálogo completo.

**Criterios de aceptación:**
- El sistema permite filtrar propuestas por estado (Borrador, EnRevision, Aprobada, Rechazada, Pendiente).
- El usuario puede buscar propuestas por código, título o nombre del docente en tiempo real.
- Los filtros pueden combinarse sin afectar la consistencia ni duplicar lógica de consulta.

**Observaciones:**
- La búsqueda de texto libre se aplica en memoria sobre los datos ya cargados para evitar peticiones adicionales al servidor.

---

## HU04 — Visualización de estadísticas por estado de propuesta

| Campo | Valor |
|---|---|
| **Código** | HU04 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Bajo |
| **Puntos estimados** | 3 |
| **Iteración asignada** | Sprint 4 |

**Descripción:** Como usuario autenticado, quiero visualizar un panel de resumen con el
total de propuestas TIC y su distribución por estado, para obtener una visión general
del portafolio sin necesidad de revisar cada registro individualmente.

**Criterios de aceptación:**
- El panel muestra el conteo total de propuestas y la distribución por cada estado definido en el sistema.
- Los contadores se actualizan automáticamente al cargar el módulo y al aplicar filtros.
- Cada tarjeta estadística permite filtrar el listado por el estado que representa al hacer clic.

**Observaciones:**
- Los contadores se calculan en el frontend a partir de los datos ya cargados, sin llamadas HTTP adicionales.

---

## HU05 — Exportación del listado de propuestas en formato PDF

| Campo | Valor |
|---|---|
| **Código** | HU05 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Medio |
| **Puntos estimados** | 5 |
| **Iteración asignada** | Sprint 5 |

**Descripción:** Como usuario autenticado, quiero exportar el listado de propuestas TIC
en formato PDF con diseño institucional EPN, para su archivo y análisis, respetando los
filtros activos aplicados en la interfaz al momento de la exportación.

**Criterios de aceptación:**
- El PDF generado incluye encabezado institucional EPN con colores corporativos y numeración de páginas.
- Los filtros activos en la interfaz se reflejan en el contenido del reporte exportado.
- El archivo se descarga directamente desde el navegador con un solo clic, sin intervención de herramientas externas.

**Observaciones:**
- El reporte se genera en orientación A4 horizontal para acomodar la tabla de propuestas con sus columnas completas.

---

## HU06 — Exportación del formulario F_AA_233A y del detalle individual en PDF

| Campo | Valor |
|---|---|
| **Código** | HU06 |
| **Usuario** | Usuario autenticado del sistema |
| **Prioridad en negocio** | Alta |
| **Riesgo en desarrollo** | Alto |
| **Puntos estimados** | 5 |
| **Iteración asignada** | Sprint 6 |

**Descripción:** Como usuario autenticado, quiero exportar el formulario institucional
F_AA_233A en formato PDF multipágina y el detalle individual de una propuesta TIC, para
facilitar los procesos formales de registro y archivo de la Coordinación de Proyectos de
Grado e Integración Curricular.

**Criterios de aceptación:**
- El endpoint `GET /pdf-formularios` retorna un PDF con una página por propuesta en el formato del formulario F_AA_233A.
- El endpoint `GET /{id}/export/pdf` retorna el PDF de detalle exclusivo de la propuesta identificada.
- Todos los flujos de exportación operan correctamente de extremo a extremo bajo los distintos escenarios de uso definidos.

**Observaciones:**
- La estructura del formulario F_AA_233A es el documento oficial mediante el cual los docentes formalizan las propuestas TIC ante la CPGIC.

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
