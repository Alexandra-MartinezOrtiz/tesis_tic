# Anexo — Historia de Usuario HU01

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

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

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
