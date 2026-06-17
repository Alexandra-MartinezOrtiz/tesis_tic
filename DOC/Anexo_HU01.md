# Anexo — Historia de Usuario HU01

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

**Tabla. Historia de usuario HU01**

<table>
  <tr>
    <td><b>Código:</b> HU01</td>
    <td><b>Usuario:</b> Usuario autenticado del sistema</td>
  </tr>
  <tr>
    <td><b>Nombre historia:</b> Consulta del listado de propuestas TIC aprobadas</td>
    <td></td>
  </tr>
  <tr>
    <td><b>Prioridad en negocio:</b> Alta</td>
    <td><b>Riesgo en desarrollo:</b> Medio</td>
  </tr>
  <tr>
    <td><b>Puntos estimados:</b> 5</td>
    <td><b>Iteración asignada:</b> 1</td>
  </tr>
  <tr>
    <td colspan="2"><b>Descripción:</b> Como usuario autenticado, quiero visualizar el listado de propuestas de Trabajo de Integración Curricular (TIC) gestionadas por la CPGIC, para consultar su estado, proponente y acceder al detalle de cada una desde la interfaz del sistema.</td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Criterios de aceptación:</b>
      <ul>
        <li>El listado muestra código, título, estado, proponente y fecha de última actualización.</li>
        <li>El usuario puede navegar al detalle de cualquier propuesta seleccionada.</li>
        <li>La ruta del módulo está protegida y redirige al inicio de sesión si no existe token activo.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <b>Observaciones:</b>
      <ul>
        <li>La información se obtiene en tiempo real desde el servicio de Propuestas mediante API Composition.</li>
      </ul>
    </td>
  </tr>
</table>

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**. En la
> tesis, las historias de usuario se presentan de forma sintetizada en la tabla resumen
> del Capítulo 2.
