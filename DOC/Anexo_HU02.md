# Anexo — Historia de Usuario HU02

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

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

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
