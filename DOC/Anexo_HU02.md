# Anexo — Historia de Usuario HU02

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

**Tabla. Historia de usuario HU02**

<table>
  <tr>
    <td><b>Código:</b> HU02</td>
    <td><b>Usuario:</b> Usuario autenticado del sistema</td>
  </tr>
  <tr>
    <td><b>Nombre historia:</b> Consulta del detalle de una propuesta TIC</td>
    <td></td>
  </tr>
  <tr>
    <td><b>Prioridad en negocio:</b> Alta</td>
    <td><b>Riesgo en desarrollo:</b> Medio</td>
  </tr>
  <tr>
    <td><b>Puntos estimados:</b> 5</td>
    <td><b>Iteración asignada:</b> 2</td>
  </tr>
  <tr>
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero consultar el detalle completo de una propuesta TIC, incluyendo descripción, alcance, datos del docente proponente y estudiantes asignados, para revisar su información en profundidad sin necesidad de recurrir a documentos físicos.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El sistema muestra todos los campos del formulario F_AA_233A para la propuesta seleccionada.</li>
        <li>La información del docente proponente y los estudiantes asignados se presenta correctamente.</li>
        <li>El sistema no lanza excepción cuando la propuesta tiene campos opcionales vacíos.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>La página de detalle es accesible mediante la ruta <code>/reportes/:id</code> sin recargar la aplicación.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
