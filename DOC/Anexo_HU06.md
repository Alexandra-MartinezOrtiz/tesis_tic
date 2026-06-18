# Anexo — Historia de Usuario HU06

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

**Tabla. Historia de usuario HU06**

<table>
  <tr>
    <td><b>Código:</b> HU06</td>
    <td><b>Usuario:</b> Usuario autenticado del sistema</td>
  </tr>
  <tr>
    <td><b>Nombre historia:</b> Exportación del formulario F_AA_233A y del detalle individual en PDF</td>
    <td></td>
  </tr>
  <tr>
    <td><b>Prioridad en negocio:</b> Alta</td>
    <td><b>Riesgo en desarrollo:</b> Alto</td>
  </tr>
  <tr>
    <td><b>Puntos estimados:</b> 5</td>
    <td><b>Iteración asignada:</b> 6</td>
  </tr>
  <tr>
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero exportar el formulario institucional F_AA_233A en formato PDF multipágina y el detalle individual de una propuesta TIC, para facilitar los procesos formales de registro y archivo de la Coordinación de Proyectos de Grado e Integración Curricular.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El endpoint <code>GET /pdf-formularios</code> retorna un PDF con una página por propuesta en el formato del formulario F_AA_233A.</li>
        <li>El endpoint <code>GET /{id}/export/pdf</code> retorna el PDF de detalle exclusivo de la propuesta identificada.</li>
        <li>Todos los flujos de exportación operan correctamente de extremo a extremo bajo los distintos escenarios de uso definidos.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>La estructura del formulario F_AA_233A es el documento oficial mediante el cual los docentes formalizan las propuestas TIC ante la CPGIC.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
