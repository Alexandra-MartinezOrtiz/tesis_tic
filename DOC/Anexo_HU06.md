# Anexo — Historia de Usuario HU06

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

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
