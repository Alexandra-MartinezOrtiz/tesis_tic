# Anexo — Historia de Usuario HU04

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

**Tabla. Historia de usuario HU04**

<table>
  <tr>
    <td><b>Código:</b> HU04</td>
    <td><b>Usuario:</b> Usuario autenticado del sistema</td>
  </tr>
  <tr>
    <td><b>Nombre historia:</b> Visualización de indicadores de disponibilidad de propuestas</td>
    <td></td>
  </tr>
  <tr>
    <td><b>Prioridad en negocio:</b> Alta</td>
    <td><b>Riesgo en desarrollo:</b> Bajo</td>
  </tr>
  <tr>
    <td><b>Puntos estimados:</b> 3</td>
    <td><b>Iteración asignada:</b> 4</td>
  </tr>
  <tr>
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero visualizar un panel de resumen con el total de propuestas TIC aprobadas y su disponibilidad, para obtener una visión general del portafolio sin necesidad de revisar cada registro individualmente.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El panel muestra el total de propuestas aprobadas, las disponibles, las no disponibles y los cupos ocupados.</li>
        <li>Los indicadores se actualizan automáticamente al cargar el módulo y al aplicar filtros.</li>
        <li>La disponibilidad se calcula a partir de los cupos, con un máximo de cinco estudiantes por propuesta.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>Los indicadores se calculan en el frontend a partir de los datos ya cargados, sin llamadas HTTP adicionales.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
