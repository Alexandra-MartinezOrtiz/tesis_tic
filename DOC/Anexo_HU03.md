# Anexo — Historia de Usuario HU03

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

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

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
