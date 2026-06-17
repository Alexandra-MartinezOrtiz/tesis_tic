# Anexo — Historia de Usuario HU04

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

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

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
