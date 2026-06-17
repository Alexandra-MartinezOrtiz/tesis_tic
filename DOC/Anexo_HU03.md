# Anexo — Historia de Usuario HU03

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

**Tabla. Historia de usuario HU03**

<table>
  <tr>
    <td><b>Código:</b> HU03</td>
    <td><b>Usuario:</b> Usuario autenticado del sistema</td>
  </tr>
  <tr>
    <td><b>Nombre historia:</b> Filtrado y búsqueda de propuestas TIC</td>
    <td></td>
  </tr>
  <tr>
    <td><b>Prioridad en negocio:</b> Alta</td>
    <td><b>Riesgo en desarrollo:</b> Medio</td>
  </tr>
  <tr>
    <td><b>Puntos estimados:</b> 5</td>
    <td><b>Iteración asignada:</b> 3</td>
  </tr>
  <tr>
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero filtrar y buscar las propuestas TIC aprobadas por disponibilidad, código, título o nombre del proponente, para localizar registros específicos de manera rápida y eficiente sin revisar el catálogo completo.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El sistema permite filtrar las propuestas aprobadas por disponibilidad (Todas, Disponibles, No disponibles).</li>
        <li>El usuario puede buscar propuestas por código, título o nombre del proponente en tiempo real.</li>
        <li>Los filtros pueden combinarse sin afectar la consistencia ni duplicar lógica de consulta.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>La búsqueda de texto libre se aplica en memoria sobre los datos ya cargados para evitar peticiones adicionales al servidor.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
