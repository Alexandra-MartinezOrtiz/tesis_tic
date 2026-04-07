# Arquitectura del Sistema TIC-FIS

## 1. Objetivo del documento

Definir una arquitectura técnica detallada para el sistema **TIC-FIS**, orientado a la gestión de propuestas de Trabajo de Integración Curricular, separando responsabilidades por módulos asociados a los estudiantes **A, B y C**, y estableciendo lineamientos de frontend, backend, lógica de negocio, diseño de base de datos, patrones de diseño, principios SOLID, buenas prácticas y criterios de implementación.

---

## 2. Visión general del sistema

El sistema TIC-FIS permitirá digitalizar el proceso de gestión de propuestas TIC dentro de la Facultad de Ingeniería de Sistemas. El sistema debe cubrir los siguientes procesos principales:

- Registro y actualización de propuestas por parte de docentes.
- Revisión y resolución de propuestas por parte de la CPGIC.
- Consulta de propuestas aprobadas y generación de reportes.
- Gestión de usuarios, roles, autenticación y autorización.

La solución propuesta utilizará las siguientes tecnologías:

- **Frontend:** Angular 17
- **Backend:** C# 12 con ASP.NET Core 8
- **Base de datos:** PostgreSQL 16
- **Control de versiones:** Git
- **Repositorio:** GitHub

---

## 3. Enfoque arquitectónico recomendado

## 3.1 Decisión arquitectónica

La arquitectura del **backend debe implementarse con microservicios**. Esta decisión permite separar claramente los dominios funcionales del sistema, asignar responsabilidades técnicas por módulo, favorecer el desacoplamiento, mejorar la mantenibilidad y permitir despliegues independientes en el futuro.

La solución propuesta se estructura en:

### Frontend

- **Una aplicación Angular 17** como cliente principal.
- Puede evolucionar a microfrontends en el futuro, pero para este proyecto se recomienda un frontend unificado.

### Backend basado en microservicios

- **Microservicio A: Propuestas TIC**
- **Microservicio B: Consultas y Reportes**
- **Microservicio C: Usuarios y Autenticación**

### Componentes de soporte recomendados

- **API Gateway** para entrada unificada desde el frontend.
- **Base de datos por microservicio** como principio arquitectónico recomendado.
- **Comunicación síncrona inicial mediante HTTP/REST**.
- Evolución futura a comunicación asíncrona para eventos de negocio relevantes.

## 3.2 Principios arquitectónicos del backend

- Cada microservicio debe poseer su propia lógica de negocio.
- Cada microservicio debe tener autonomía sobre su persistencia.
- No se deben compartir tablas entre microservicios.
- La comunicación entre microservicios debe realizarse por contratos explícitos.
- Cada servicio debe poder evolucionar con bajo acoplamiento.
- Las reglas de negocio deben permanecer encapsuladas dentro del servicio propietario del dominio.

## 3.3 Microservicios propuestos

### Microservicio A: Propuestas TIC
Responsable del ciclo de vida de las propuestas, observaciones, revisión, cambios de estado y asignación de estudiantes.

### Microservicio B: Consultas y Reportes
Responsable de consultas optimizadas, filtros, vistas de lectura, consolidación de datos visibles y exportación de reportes.

### Microservicio C: Usuarios y Autenticación
Responsable de identidad, autenticación, autorización, gestión de usuarios, roles y permisos.

## 3.4 API Gateway

Se recomienda un **API Gateway** como punto único de entrada para el frontend. Sus responsabilidades deben incluir:

- Enrutamiento a microservicios.
- Validación inicial del token.
- Aplicación de políticas transversales.
- Manejo consistente de CORS.
- Agregación mínima cuando sea necesario.

## 3.5 Despliegue independiente en contenedores

Para que la solución cumpla realmente con una arquitectura de microservicios, los tres microservicios deben implementarse como **proyectos independientes**, con capacidad de construcción, ejecución y despliegue autónomo.

Esto implica que cada microservicio debe contar con:

- su propio proyecto o solución,
- su propia API,
- su propia lógica de negocio,
- su propia persistencia,
- su propia configuración,
- su propio archivo `Dockerfile`,
- y capacidad de desplegarse en un **contenedor distinto**.

La arquitectura mínima esperada debe contemplar:

- un contenedor para **Propuestas Service**,
- un contenedor para **Reportes Service**,
- un contenedor para **Identity Service**,
- y opcionalmente un contenedor adicional para el **API Gateway**.

Esta separación permite:

- despliegue independiente,
- versionamiento independiente,
- aislamiento de fallos,
- escalado selectivo,
- y mejor alineación con los principios de microservicios.

Si los tres módulos estuvieran dentro de un único proyecto ejecutable, aunque internamente estuvieran separados por carpetas o capas, la solución no debería considerarse formalmente como una arquitectura de microservicios, sino como un monolito modular.

## 3.6 Comunicación entre servicios

Para la primera versión del proyecto se recomienda:

- **REST síncrono** para consultas entre servicios.
- Uso de DTOs y contratos explícitos.
- Timeouts y manejo de errores controlados.

En una evolución futura, se puede incorporar comunicación basada en eventos para escenarios como:

- propuesta aprobada,
- propuesta actualizada,
- estudiante asignado,
- usuario creado o desactivado.

---

## 4. Arquitectura frontend

## 4.1 Responsabilidad del frontend

El frontend en Angular 17 será responsable de:

- Renderizar la interfaz de usuario.
- Gestionar navegación y rutas.
- Validar datos del lado cliente.
- Consumir APIs del backend.
- Manejar estados de carga, error y éxito.
- Implementar guardas de rutas según rol.
- Mantener una experiencia de usuario consistente, accesible y escalable.

## 4.2 Estructura recomendada del frontend

```text
src/
  app/
    core/
      guards/
      interceptors/
      services/
      models/
      constants/
    shared/
      components/
      pipes/
      directives/
      utils/
    features/
      propuestas/
        pages/
        components/
        services/
        models/
      reportes/
        pages/
        components/
        services/
        models/
      auth/
        pages/
        components/
        services/
        models/
      usuarios/
        pages/
        components/
        services/
        models/
    layout/
    app.routes.ts
```

## 4.3 Principios de diseño en frontend

- Separar componentes de presentación de componentes contenedores.
- Centralizar acceso a API mediante servicios.
- Evitar lógica de negocio compleja dentro de componentes.
- Reutilizar componentes visuales en `shared`.
- Mantener modelos tipados con TypeScript.
- Usar formularios reactivos para validaciones robustas.
- Implementar interceptores para autenticación, manejo de errores y logging básico.

## 4.4 Patrones de diseño aplicables en frontend

### Container/Presenter
Separar componentes que manejan estado, navegación y llamadas a API de componentes puramente visuales.

### Facade
Crear servicios fachada por módulo para simplificar la interacción de páginas con múltiples servicios internos.

### Strategy
Aplicable para exportación de reportes, filtros dinámicos o representación diferenciada según rol.

### Guard Pattern
Uso de guards para control de acceso por autenticación y autorización.

## 4.5 Buenas prácticas frontend

- Un componente debe tener una responsabilidad clara.
- No duplicar interfaces ni modelos.
- Evitar subscribes sin manejo de liberación de recursos.
- Mantener nombres consistentes en rutas, componentes y servicios.
- Mostrar errores funcionales de forma comprensible para el usuario.
- Aplicar accesibilidad básica: labels, contraste, navegación con teclado.
- Evitar lógica de autorización solo en frontend; el backend debe validar todo.

---

## 5. Arquitectura backend

## 5.1 Responsabilidad del backend

El backend en ASP.NET Core 8 debe implementarse como un **ecosistema de microservicios**, donde cada servicio sea responsable de un dominio funcional específico y exponga APIs REST independientes.

Sus responsabilidades incluyen:

- Exponer APIs REST por servicio.
- Aplicar reglas de negocio propias del dominio.
- Persistir información en su propia base de datos.
- Gestionar autenticación y autorización.
- Validar integridad de datos.
- Generar reportes exportables en el microservicio correspondiente.
- Mantener trazabilidad y observabilidad por servicio.

## 5.2 Estructura recomendada del backend por microservicio

```text
backend/
  gateway/
    TicFis.ApiGateway/
      Dockerfile

  services/
    propuestas-service/
      Propuestas.Api/
      Propuestas.Application/
      Propuestas.Domain/
      Propuestas.Infrastructure/
      Propuestas.Tests/
      Dockerfile

    reportes-service/
      Reportes.Api/
      Reportes.Application/
      Reportes.Domain/
      Reportes.Infrastructure/
      Reportes.Tests/
      Dockerfile

    identity-service/
      Identity.Api/
      Identity.Application/
      Identity.Domain/
      Identity.Infrastructure/
      Identity.Tests/
      Dockerfile
```

Cada microservicio debe ser una unidad desplegable independiente. Esto significa que debe poder:

- compilarse por separado,
- ejecutarse por separado,
- probarse por separado,
- desplegarse por separado,
- y contenedorizase de forma independiente.

Desde el punto de vista técnico, para cumplir con una arquitectura de microservicios, no basta con separar módulos a nivel lógico; es necesario que cada servicio exista como proyecto autónomo y que pueda ejecutarse en un contenedor diferente.

## 5.3 Capas internas por microservicio

Cada microservicio debe organizarse internamente por capas:

### Capa API
- Controllers o minimal APIs.
- Validación de requests.
- Autorización.
- Serialización de respuestas.

### Capa Application
- Casos de uso.
- Servicios de aplicación.
- DTOs.
- Interfaces.
- Orquestación de operaciones.

### Capa Domain
- Entidades.
- Value Objects.
- Servicios de dominio.
- Reglas de negocio.
- Eventos de dominio si se incorporan.

### Capa Infrastructure
- Implementación de repositorios.
- Configuración de EF Core.
- Acceso a PostgreSQL.
- Integraciones con PDF/Excel.
- Seguridad y JWT en el servicio de identidad.

## 5.4 Base de datos por microservicio

Como principio de microservicios, cada servicio debe tener **su propia base de datos o esquema privado**, evitando acoplamiento por persistencia compartida.

### Propuestas Service
Base de datos propietaria para propuestas, historial, observaciones y asignaciones.

### Reportes Service
Base de datos o esquema orientado a lectura, filtros y reportes. Puede poblarse por replicación lógica, sincronización controlada o consumo de APIs del servicio de propuestas, según el nivel de complejidad deseado.

### Identity Service
Base de datos propietaria para usuarios, roles, permisos, credenciales y sesiones.

## 5.5 Recomendación de comunicación

### Comunicación síncrona
- HTTP/REST entre microservicios.
- Ideal para una primera versión del proyecto.

### Comunicación asíncrona futura
- Eventos de dominio o mensajería para propagar cambios relevantes.
- Especialmente útil entre Propuestas Service y Reportes Service.

## 5.6 Transacciones y consistencia

- Cada microservicio mantiene consistencia fuerte dentro de su propio límite.
- Entre servicios se asume **consistencia eventual** cuando exista sincronización de datos derivados.
- No se deben usar transacciones distribuidas como primera opción.

## 5.7 Despliegue

Cada microservicio debe poder desplegarse de forma independiente y versionarse por separado.

La estrategia recomendada es usar contenedores Docker independientes para cada componente backend:

- `propuestas-service`
- `reportes-service`
- `identity-service`
- `api-gateway` (recomendado)

Cada uno debe tener:

- su propio `Dockerfile`,
- variables de entorno propias,
- configuración propia,
- y pipeline de construcción independiente en la medida de lo posible.

En un entorno local o académico, el despliegue puede coordinarse con `docker-compose`, permitiendo levantar toda la solución de forma integrada, pero manteniendo la independencia real de cada servicio.

Ejemplo conceptual:

```yaml
services:
  api-gateway:
    build: ./gateway/TicFis.ApiGateway
  propuestas-service:
    build: ./services/propuestas-service
  reportes-service:
    build: ./services/reportes-service
  identity-service:
    build: ./services/identity-service
```

Este enfoque permite demostrar claramente que la arquitectura sí cumple con los criterios de microservicios.

---

## 6. Principios SOLID aplicados

## 6.1 Single Responsibility Principle

Cada clase debe tener una única razón para cambiar.

Ejemplos:
- Un controlador no debe contener lógica de negocio.
- Un repositorio no debe generar archivos PDF.
- Un servicio de autenticación no debe consultar reportes.

## 6.2 Open/Closed Principle

Los módulos deben estar abiertos a extensión, pero cerrados a modificación.

Ejemplo:
- La exportación de reportes debe permitir agregar CSV en el futuro sin modificar la lógica central existente.

## 6.3 Liskov Substitution Principle

Las implementaciones concretas deben poder sustituir a sus abstracciones sin romper el comportamiento.

Ejemplo:
- `IReporteExporter` puede tener implementaciones `PdfReporteExporter` y `ExcelReporteExporter`.

## 6.4 Interface Segregation Principle

No obligar a una clase a depender de métodos que no usa.

Ejemplo:
- Separar interfaces de consulta de interfaces de escritura.

## 6.5 Dependency Inversion Principle

Las capas superiores no deben depender de implementaciones concretas, sino de abstracciones.

Ejemplo:
- Application depende de `IPropuestaRepository`, no de `PropuestaRepository`.

---

## 7. Patrones de diseño recomendados

## 7.1 Repository

Encapsula el acceso a datos y evita acoplar la lógica de negocio con la persistencia.

## 7.2 Unit of Work

Útil para coordinar cambios de múltiples repositorios dentro de una misma transacción.

## 7.3 CQRS ligero

Separar operaciones de lectura y escritura cuando el módulo lo requiera, especialmente en Consultas y Reportes.

- Commands: crear, actualizar, revisar, aprobar, rechazar.
- Queries: listar propuestas, obtener detalle, generar datos para reporte.

## 7.4 Factory

Útil para crear exportadores, validadores o respuestas según tipo de reporte o rol.

## 7.5 Strategy

Ideal para:
- exportación PDF/Excel,
- reglas de filtrado,
- comportamientos por rol.

## 7.6 Specification

Muy útil para filtros dinámicos sobre propuestas, combinando criterios como estado, docente, fecha y disponibilidad.

## 7.7 Mediator

Puede aplicarse mediante MediatR para desacoplar controladores y casos de uso.

## 7.8 Mapper

Para transformar entidades a DTOs y viceversa, preferiblemente usando mapeo explícito o AutoMapper con moderación.

---

## 8. Código limpio y buenas prácticas

## 8.1 Reglas generales

- Nombres claros, precisos y orientados al dominio.
- Métodos cortos y enfocados.
- Clases pequeñas con responsabilidad única.
- Evitar duplicación.
- Preferir composición sobre herencia innecesaria.
- Validar temprano y fallar de forma controlada.
- Centralizar manejo de excepciones.
- Mantener reglas de negocio fuera de controladores.

## 8.2 Convenciones recomendadas

- Entidades en singular.
- Tablas en singular o plural, pero consistente.
- DTOs con sufijos claros: `CreatePropuestaRequest`, `PropuestaResponse`.
- Interfaces con prefijo `I`.
- Casos de uso con verbos: `CreatePropuestaHandler`, `GetPropuestasQueryHandler`.

## 8.3 Pruebas

- Pruebas unitarias para servicios de dominio y aplicación.
- Pruebas de integración para repositorios y endpoints.
- Pruebas funcionales para flujos principales.
- Pruebas de usabilidad para frontend.

---

## 9. Separación por módulos o microservicios según estudiantes

# 9.1 Estudiante A — Módulo de Propuestas TIC

## Responsabilidad funcional

Gestiona el ciclo de vida de las propuestas TIC desde su creación hasta su revisión y actualización.

## Funcionalidades principales

- Crear propuesta TIC.
- Editar propuesta antes o después de observaciones.
- Enviar propuesta a revisión.
- Registrar observaciones.
- Aprobar, rechazar o marcar como pendiente.
- Asociar estudiantes a propuestas cuando corresponda.
- Consultar historial de estados.

## Backend del módulo A

### Entidades principales
- Propuesta
- PropuestaEstadoHistorial
- PropuestaIntegrante
- ObservacionPropuesta
- Docente

### Casos de uso
- Crear propuesta
- Actualizar propuesta
- Enviar propuesta a revisión
- Revisar propuesta
- Aprobar propuesta
- Rechazar propuesta
- Marcar propuesta como pendiente
- Asignar estudiantes a propuesta
- Obtener detalle completo de propuesta

### Reglas de negocio
- Una propuesta debe tener un docente proponente.
- Una propuesta no puede aprobarse si faltan campos obligatorios.
- Solo usuarios con rol CPGIC pueden aprobar, rechazar o marcar pendiente.
- Solo una propuesta aprobada puede publicarse para consulta general.
- Toda transición de estado debe registrarse en historial.
- Si una propuesta aprobada recibe asignación de estudiantes, debe persistirse la trazabilidad del cambio.

## Frontend del módulo A

### Pantallas sugeridas
- Formulario de creación y edición.
- Bandeja de propuestas del docente.
- Vista de detalle.
- Bandeja de revisión CPGIC.
- Pantalla de observaciones.
- Pantalla de historial.

### Implementación recomendada
- Formularios reactivos.
- Validaciones por secciones.
- Componentes reutilizables para datos de docente, estudiantes, estado e historial.
- Confirmaciones al aprobar, rechazar o reenviar.

---

# 9.2 Estudiante B — Módulo de Consultas y Reportes

## Responsabilidad funcional

Permite consultar propuestas aprobadas, filtrarlas, ver su detalle y generar reportes exportables.

## Funcionalidades principales

- Listar propuestas aprobadas.
- Filtrar por estado, docente, fecha, disponibilidad.
- Consultar detalle de propuesta.
- Visualizar estudiantes asignados.
- Generar reporte general.
- Exportar a PDF y Excel.

## Backend del módulo B

### Entidades o modelos relevantes
- PropuestaConsultaView
- ReporteGenerado
- FiltroReporte

En este módulo puede ser válido utilizar una aproximación CQRS o vistas materializadas si el volumen crece.

### Casos de uso
- Obtener propuestas aprobadas
- Obtener propuestas filtradas
- Obtener detalle de propuesta para consulta
- Generar dataset de reporte
- Exportar reporte PDF
- Exportar reporte Excel

### Reglas de negocio
- Solo deben mostrarse propuestas aprobadas o estados permitidos según rol.
- Los filtros deben validar rangos de fechas y consistencia de criterios.
- La exportación debe respetar exactamente el conjunto filtrado por el usuario.
- Los reportes deben incluir al menos estado, proponente, fecha de última actualización y disponibilidad.

## Frontend del módulo B

### Pantallas sugeridas
- Listado principal de propuestas.
- Barra de filtros avanzados.
- Vista de detalle.
- Vista previa de reporte.
- Acciones de exportación.

### Implementación recomendada
- Tabla paginada.
- Filtros desacoplados como componente independiente.
- Estado visual claro para aprobado, pendiente, rechazado, disponible, asignado.
- Exportación disparada desde acciones con feedback visual.

### Patrones recomendados
- Specification para filtros.
- Strategy para exportación.
- Facade para orquestar consultas y reportes desde frontend.

---

# 9.3 Estudiante C — Módulo de Usuarios y Autenticación

## Responsabilidad funcional

Gestiona acceso seguro, usuarios, roles y permisos.

## Funcionalidades principales

- Inicio de sesión.
- Cierre de sesión.
- Restablecimiento de contraseña.
- Gestión de usuarios.
- Gestión de roles.
- Control de acceso por módulo y acción.

## Backend del módulo C

### Entidades principales
- Usuario
- Rol
- Permiso
- UsuarioRol
- TokenRevocado o Sesion

### Casos de uso
- Autenticar usuario
- Emitir token JWT
- Refrescar token
- Registrar usuario
- Actualizar usuario
- Cambiar contraseña
- Restablecer contraseña
- Asignar roles
- Validar permisos

### Reglas de negocio
- El correo institucional debe ser único.
- No se debe almacenar contraseñas en texto plano.
- Toda contraseña debe almacenarse con hash seguro.
- Solo administradores pueden gestionar roles.
- El acceso a acciones sensibles debe validarse en backend.

## Frontend del módulo C

### Pantallas sugeridas
- Login.
- Recuperación de contraseña.
- Administración de usuarios.
- Administración de roles.
- Perfil de usuario.

### Implementación recomendada
- Guards de autenticación y roles.
- Interceptor para token JWT.
- Manejo de expiración de sesión.
- Formularios reactivos con validaciones fuertes.

---

## 10. Lógica de negocio transversal

## 10.1 Estados de propuesta sugeridos

- Borrador
- En revisión
- Pendiente de corrección
- Aprobada
- Rechazada

## 10.2 Flujo principal del negocio

1. El docente crea una propuesta.
2. La propuesta queda en borrador o se envía a revisión.
3. La CPGIC revisa la propuesta.
4. La propuesta puede aprobarse, rechazarse o quedar pendiente.
5. Las propuestas aprobadas quedan disponibles para consulta.
6. El módulo de reportes consume esta información y la presenta filtrada.
7. Los usuarios autorizados pueden exportar reportes.

## 10.3 Reglas transversales

- Todas las acciones críticas deben registrar auditoría mínima.
- Toda transición de estado debe quedar trazable.
- La autorización debe depender de rol y acción.
- La información de consulta debe derivarse de datos consistentes del módulo A.

---

## 11. Diseño de base de datos

## 11.1 Enfoque general

Debido a que el backend debe construirse con microservicios, se adopta el principio **database per service**. Esto significa que cada microservicio controla su propia persistencia y no comparte tablas directamente con otros servicios.

Se propone PostgreSQL 16 como tecnología común, pero con separación por servicio mediante:

- bases de datos independientes, o
- esquemas independientes estrictamente aislados.

La opción preferida es **una base por microservicio**.

## 11.2 Base de datos del microservicio C — Identity Service

### Tablas principales

#### usuarios
- id
- nombres
- apellidos
- email
- password_hash
- activo
- creado_en
- actualizado_en

#### roles
- id
- nombre
- descripcion

#### permisos
- id
- codigo
- descripcion

#### usuario_roles
- id
- usuario_id
- rol_id

#### rol_permisos
- id
- rol_id
- permiso_id

#### sesiones o tokens_revocados
- id
- usuario_id
- token_jti
- expira_en
- revocado_en

## 11.3 Base de datos del microservicio A — Propuestas Service

### Tablas principales

#### docentes
- id
- usuario_id_referencia
- titulo
- departamento

#### estudiantes
- id
- nombres
- apellidos
- email
- carrera

#### propuestas
- id
- codigo
- titulo
- descripcion
- problema
- objetivo_general
- alcance
- docente_id
- estado_actual
- fecha_envio
- fecha_ultima_actualizacion
- activa

#### propuesta_estudiantes
- id
- propuesta_id
- estudiante_id
- fecha_asignacion

#### propuesta_observaciones
- id
- propuesta_id
- observacion
- creado_por_usuario_id
- creado_en

#### propuesta_historial_estados
- id
- propuesta_id
- estado_anterior
- estado_nuevo
- comentario
- cambiado_por_usuario_id
- cambiado_en

## 11.4 Base de datos del microservicio B — Reportes Service

Este microservicio debe enfocarse en lectura y generación de reportes.

### Opción recomendada
Mantener un modelo de lectura propio para consultas rápidas y exportaciones.

### Tablas principales sugeridas

#### propuestas_consulta
- id
- propuesta_id_origen
- codigo
- titulo
- docente_nombre
- estado_actual
- fecha_ultima_actualizacion
- disponible

#### propuestas_consulta_estudiantes
- id
- propuesta_consulta_id
- estudiante_nombre
- estudiante_email

#### reportes_generados
- id
- tipo
- parametros_json
- generado_por_usuario_id
- generado_en
- ruta_archivo

## 11.5 Estrategias para alimentar Reportes Service

Se pueden usar tres estrategias, en orden de madurez:

### Estrategia 1: Consulta síncrona al servicio A
Más simple, útil para la primera versión. Reportes Service consulta Propuestas Service en tiempo real.

### Estrategia 2: Sincronización programada
Reportes Service actualiza su modelo de lectura periódicamente.

### Estrategia 3: Eventos de negocio
Cuando Propuestas Service cambia estados o asignaciones, publica eventos para actualizar el modelo de lectura de Reportes Service.

## 11.6 Buenas prácticas de base de datos

- No compartir tablas entre servicios.
- Exponer datos entre servicios solo mediante API o eventos.
- Crear índices sobre columnas de filtro y búsqueda.
- Usar migraciones independientes por microservicio.
- Mantener integridad interna en cada servicio.
- Evitar duplicación innecesaria, salvo cuando sea justificada para lectura y reportes.

---

## 12. Implementación recomendada por módulo

## 12.1 Módulo A

### Paso a paso
1. Definir entidad Propuesta y estados.
2. Diseñar formulario del frontend.
3. Implementar endpoints CRUD y transición de estados.
4. Registrar historial y observaciones.
5. Integrar permisos por rol.
6. Probar flujos de docente y CPGIC.

## 12.2 Módulo B

### Paso a paso
1. Diseñar modelo de consulta optimizado.
2. Implementar endpoints de listado, detalle y filtros.
3. Crear componente de filtros en Angular.
4. Implementar exportadores PDF y Excel.
5. Incorporar paginación y ordenamiento.
6. Probar consistencia entre filtros y reportes exportados.

## 12.3 Módulo C

### Paso a paso
1. Implementar entidad Usuario, Rol y Permiso.
2. Configurar autenticación JWT.
3. Implementar login y recuperación de contraseña.
4. Aplicar autorización por políticas.
5. Proteger endpoints y rutas del frontend.
6. Probar escenarios de acceso permitido y denegado.

---

## 13. APIs sugeridas

## Módulo A
- `POST /api/propuestas`
- `PUT /api/propuestas/{id}`
- `GET /api/propuestas/{id}`
- `POST /api/propuestas/{id}/enviar-revision`
- `POST /api/propuestas/{id}/aprobar`
- `POST /api/propuestas/{id}/rechazar`
- `POST /api/propuestas/{id}/pendiente`
- `POST /api/propuestas/{id}/asignar-estudiantes`

## Módulo B
- `GET /api/reportes/propuestas`
- `GET /api/reportes/propuestas/{id}`
- `GET /api/reportes/propuestas/export/pdf`
- `GET /api/reportes/propuestas/export/excel`

## Módulo C
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `GET /api/usuarios`
- `POST /api/usuarios`
- `PUT /api/usuarios/{id}`
- `POST /api/usuarios/{id}/roles`

---

## 14. Seguridad

- Autenticación con JWT.
- Contraseñas con hashing fuerte.
- Autorización por roles y permisos.
- Validación de entradas en backend.
- Protección contra sobreexposición de datos.
- Logging de operaciones sensibles.
- Configuración segura de CORS.
- No exponer stack traces al cliente.

---

## 15. Observabilidad y mantenimiento

- Logging estructurado.
- Manejo global de excepciones.
- Health checks.
- Versionado de API.
- Migraciones controladas.
- Documentación OpenAPI/Swagger.

---

## 16. Recomendación final

Dado que el backend debe construirse con **microservicios**, la arquitectura final recomendada queda definida así:

- **Frontend unificado en Angular 17**.
- **API Gateway** como punto de entrada.
- **Microservicio A: Propuestas TIC**.
- **Microservicio B: Consultas y Reportes**.
- **Microservicio C: Usuarios y Autenticación**.
- **Persistencia separada por microservicio**.
- **Comunicación REST síncrona** en la primera etapa.
- Evolución futura hacia **eventos de negocio** para mejorar desacoplamiento y consistencia eventual.
- **Despliegue independiente en contenedores distintos para cada microservicio**.

La separación funcional por estudiantes queda así:

- **Estudiante A:** desarrollo del microservicio de Propuestas TIC.
- **Estudiante B:** desarrollo del microservicio de Consultas y Reportes.
- **Estudiante C:** desarrollo del microservicio de Usuarios y Autenticación.

Para que esta arquitectura cumpla formalmente con los principios de microservicios, cada uno de estos tres servicios debe existir como un **proyecto backend independiente**, con:

- su propia solución o proyecto,
- su propia API,
- su propia lógica de negocio,
- su propia persistencia,
- su propia configuración,
- su propio `Dockerfile`,
- y capacidad de desplegarse en un **contenedor diferente**.

Si la solución se implementa como un único ejecutable backend con módulos internos, no debería denominarse microservicios, sino arquitectura modular. Por tanto, la independencia de construcción, despliegue y ejecución de cada servicio es un criterio obligatorio para sostener técnicamente esta decisión arquitectónica.

Cada microservicio debe diseñarse respetando:

- principios SOLID,
- patrones de diseño apropiados,
- encapsulamiento de reglas de negocio,
- contratos claros,
- bajo acoplamiento,
- alta cohesión,
- código limpio,
- trazabilidad y pruebas.

Este enfoque permite una arquitectura moderna, escalable y alineada con el reparto académico del proyecto, manteniendo claridad técnica y posibilidad de crecimiento futuro.

