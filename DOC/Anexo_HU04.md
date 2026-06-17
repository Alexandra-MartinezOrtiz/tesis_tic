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
    <td><b>Nombre historia:</b> Visualización de estadísticas por estado de propuesta</td>
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
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero visualizar un panel de resumen con el total de propuestas TIC y su distribución por estado, para obtener una visión general del portafolio sin necesidad de revisar cada registro individualmente.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El panel muestra el conteo total de propuestas y la distribución por cada estado definido en el sistema.</li>
        <li>Los contadores se actualizan automáticamente al cargar el módulo y al aplicar filtros.</li>
        <li>Cada tarjeta estadística permite filtrar el listado por el estado que representa al hacer clic.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>Los contadores se calculan en el frontend a partir de los datos ya cargados, sin llamadas HTTP adicionales.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
