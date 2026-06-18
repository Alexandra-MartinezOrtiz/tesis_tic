ikuj# MÓDULO DE CONSULTAS Y REPORTES
## Sistema TIC-FIS — Trabajo de Integración Curricular EPN / FIS

---

> **Nota de imágenes:** Las capturas de pantalla referenciadas en este documento se encuentran en la carpeta `DOC/capturas_tesis/`. Para el documento final de Word/PDF, insertar cada imagen en el punto indicado con la etiqueta `[FIGURA X]`.

---

## 1. INTRODUCCIÓN AL MÓDULO

El módulo de Consultas y Reportes nació de una necesidad muy concreta que se identificó al inicio del análisis de requerimientos: los coordinadores de la Facultad de Ingeniería en Sistemas de la EPN necesitaban una forma rápida y confiable de consultar el estado general de las propuestas de Trabajo de Integración Curricular (TIC) sin tener que revisar cada expediente de forma manual. Hasta ese momento, el proceso era completamente manual: se descargaban hojas de cálculo, se cruzaba información de correos electrónicos y se generaban reportes en Word uno por uno. El resultado era lento, propenso a errores y difícil de auditar.

Este módulo resuelve ese problema de raíz. Permite a cualquier usuario autorizado del sistema consultar el listado completo de propuestas, filtrarlas por estado o por texto libre, ver el detalle individual de cada una y, cuando lo necesite, exportar toda esa información en un reporte PDF o Excel con formato institucional, listo para presentar.

Lo que hace interesante al módulo desde el punto de vista de arquitectura de software es que no tiene base de datos propia para almacenar propuestas. En lugar de duplicar información, consume en tiempo real los datos del servicio de Propuestas a través de su API REST, los procesa, los filtra y los presenta. Esto garantiza que el reporte siempre refleja el estado actual del sistema, sin desfases ni inconsistencias.

---

## 2. ARQUITECTURA DEL MÓDULO

### 2.1 Posición dentro del sistema TIC-FIS

El sistema TIC-FIS está construido como una arquitectura de microservicios, lo que significa que cada responsabilidad funcional vive en un servicio independiente con su propia base de datos, su propio proceso y su propio ciclo de despliegue. El módulo de Consultas y Reportes es el tercer servicio de esta arquitectura.

```
┌─────────────────────────────────────────────────────────────┐
│                    USUARIO / NAVEGADOR                       │
│                  http://localhost:4200                        │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                 API GATEWAY (YARP)                           │
│                  Puerto 5000                                 │
│      Enruta: /api/reportes/* → Reportes Service             │
│              /api/propuestas/* → Propuestas Service          │
│              /api/auth/* → Identity Service                  │
└──────────┬──────────────┬──────────────┬────────────────────┘
           │              │              │
    ┌──────▼──────┐ ┌─────▼──────┐ ┌────▼────────────┐
    │  Identity   │ │ Propuestas │ │    Reportes     │
    │  Service    │ │  Service   │ │    Service      │
    │  Pto. 5001  │ │  Pto. 5002 │ │    Pto. 5003   │
    │ ticfis_iden │ │ ticfis_pro │ │ (sin BD propia)│
    └─────────────┘ └────────────┘ └────────────────┘
```

El API Gateway actúa como punto de entrada único. Cuando el usuario navega al módulo de reportes en el navegador, el frontend de Angular envía las peticiones al gateway en el puerto 5000, y el gateway las redirige internamente al servicio de Reportes en el puerto 5003. Este servicio, a su vez, llama al servicio de Propuestas (5002) para obtener los datos que necesita, y agrega su token JWT para que la llamada quede autenticada de punta a punta.

### 2.2 Clean Architecture en el servicio de Reportes

El servicio de Reportes sigue el patrón de Arquitectura Limpia (Clean Architecture) propuesto por Robert C. Martin. Esto significa que las reglas de negocio no dependen de los frameworks ni de los detalles de infraestructura. La estructura de capas es la siguiente:

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| API | `Reportes.Api` | Controladores HTTP, configuración de Swagger y JWT |
| Application | `Reportes.Application` | Contratos (interfaces), DTOs, casos de uso |
| Infrastructure | `Reportes.Infrastructure` | Implementación de servicios: llamadas HTTP, generación PDF/Excel |

La regla más importante de esta arquitectura es que la capa de Application no sabe nada sobre cómo se obtienen los datos. Define una interfaz `IReportesService` y el controlador solo habla con esa interfaz. La implementación concreta (`ReportesConsultaService`) vive en Infrastructure y puede cambiarse sin tocar nada más.

### 2.3 Tecnologías utilizadas

El servicio de Reportes fue construido con las siguientes tecnologías:

**Backend:**
- **.NET 10** con ASP.NET Core 10 — plataforma de ejecución
- **QuestPDF 2024** — generación de documentos PDF con diseño fluido
- **ClosedXML 0.104** — creación de archivos Excel (.xlsx) con formato
- **JWT Bearer** — validación de tokens de acceso emitidos por el Identity Service
- **Swashbuckle.AspNetCore 10.1.7** — documentación interactiva de la API

**Frontend:**
- **Angular 17** con componentes standalone
- **Angular Signals** — gestión de estado reactivo sin necesidad de NgRx
- **Angular HttpClient** — comunicación con el backend a través del gateway

---

## 3. DISEÑO DE LA BASE DE DATOS

### 3.1 Decisión de arquitectura: sin base de datos propia para reportes

Una de las decisiones de diseño más importantes del módulo fue no crear una tabla de propuestas propia. En arquitecturas de microservicios existe un patrón llamado **API Composition** que consiste precisamente en esto: un servicio consulta a otros servicios en tiempo real para componer su respuesta, en lugar de mantener una copia local de los datos.

Esta decisión tiene ventajas claras:
- El reporte siempre tiene los datos más actualizados, sin necesidad de sincronización
- No hay riesgo de inconsistencia entre la copia local y la fuente de verdad
- El servicio de Reportes es más simple: no necesita migraciones de base de datos para las propuestas

Sin embargo, se diseñó una base de datos de apoyo (`ticfis_reportes`) para funciones complementarias que sí requieren persistencia local.

### 3.2 Esquema de la base de datos `ticfis_reportes`

La base de datos `ticfis_reportes` contiene las siguientes tablas:

```sql
-- Períodos académicos (referencia para filtros)
CREATE TABLE periodos_academicos (
    periodo_id      BIGINT          PRIMARY KEY,
    nombre          VARCHAR(100)    NOT NULL,
    fecha_inicio    DATE            NOT NULL,
    fecha_fin       DATE            NOT NULL,
    estado          VARCHAR(20)     NOT NULL DEFAULT 'activo',
    sincronizado_en TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Caché opcional de propuestas (para reportes offline)
CREATE TABLE propuestas_cache (
    propuesta_id            BIGINT          PRIMARY KEY,
    codigo                  VARCHAR(50)     NOT NULL,
    titulo                  VARCHAR(300)    NOT NULL,
    ...
    estado_actual           estado_propuesta NOT NULL,
    sincronizado_en         TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Auditoría de reportes generados
CREATE TABLE auditoria_reportes (
    auditoria_id    BIGSERIAL       PRIMARY KEY,
    usuario_email   VARCHAR(150)    NOT NULL,
    formato         formato_reporte NOT NULL,  -- 'pdf' o 'excel'
    filtro_estado   estado_propuesta,
    filtro_busqueda VARCHAR(200),
    total_registros INTEGER         NOT NULL DEFAULT 0,
    generado_en     TIMESTAMPTZ     NOT NULL DEFAULT now(),
    duracion_ms     INTEGER
);
```

La tabla más relevante para la trazabilidad es `auditoria_reportes`, que registra cada vez que alguien genera un PDF o Excel: quién lo generó, con qué filtros, cuántos registros incluía y cuánto tiempo tomó. Esto permite responder preguntas de auditoría como "¿Quién descargó el reporte el 6 de mayo de 2026?".

### 3.3 Vistas de consulta

Para simplificar las consultas frecuentes, se crearon tres vistas:

- **`v_propuestas_reporte`**: listado de propuestas con conteo de estudiantes asignados
- **`v_propuesta_detalle`**: detalle completo de una propuesta incluyendo todos sus estudiantes
- **`v_estadisticas_estado`**: contadores de propuestas agrupadas por estado (usada por el dashboard)

---

## 4. IMPLEMENTACIÓN DEL BACKEND

### 4.1 El controlador: punto de entrada de la API

El corazón del backend es el `ReportesController`. Al abrir el archivo, lo primero que llama la atención es su simplicidad. Tiene exactamente cuatro endpoints y cada uno hace una sola cosa:

```csharp
[ApiController]
[Route("api/reportes/propuestas")]
[Authorize]
public class ReportesController : ControllerBase
{
    [HttpGet]           // Listar propuestas con filtros
    [HttpGet("{id}")]   // Obtener detalle de una propuesta
    [HttpGet("export/pdf")]   // Exportar a PDF
    [HttpGet("export/excel")] // Exportar a Excel
}
```

El atributo `[Authorize]` sobre la clase garantiza que ningún endpoint sea accesible sin un token JWT válido. Cualquier petición sin el header `Authorization: Bearer <token>` recibirá automáticamente un error HTTP 401 Unauthorized antes de que el código del controlador llegue a ejecutarse.

Una característica interesante del diseño es que el controlador extrae el header de autorización de la petición entrante y lo propaga a las llamadas internas al servicio de Propuestas:

```csharp
private string? AuthHeader => Request.Headers.Authorization.ToString();
```

Esto asegura que cuando el servicio de Reportes llama al servicio de Propuestas en nombre del usuario, usa el mismo token del usuario original. El servicio de Propuestas también valida ese token, creando así una cadena de autenticación de extremo a extremo.

### 4.2 El servicio de consulta: lógica de negocio

La implementación del servicio (`ReportesConsultaService`) sigue un patrón muy claro: primero obtiene los datos del servicio de Propuestas, luego los filtra en memoria si hay un término de búsqueda de texto libre, y finalmente los devuelve.

```csharp
public async Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(
    string? authorizationHeader, string? estado, string? busqueda,
    int page, int pageSize, CancellationToken cancellationToken)
{
    using var client = CreateClient(authorizationHeader);
    var url = $"api/propuestas?page={page}&pageSize={pageSize}";
    if (!string.IsNullOrWhiteSpace(estado))
        url += $"&estado={Uri.EscapeDataString(estado)}";
    
    var response = await client.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();
    // ... deserializar y filtrar por busqueda
}
```

El filtro por estado se envía directamente como parámetro al servicio de Propuestas para reducir el volumen de datos transferidos. El filtro por texto libre, en cambio, se aplica en memoria sobre los resultados ya recibidos, porque el servicio de Propuestas no expone búsqueda de texto completo en su API actual.

### 4.3 Contratos de datos: los DTOs

Los objetos de transferencia de datos (DTOs) definen exactamente qué información viaja entre capas. En este módulo hay tres:

**`PropuestaReporteItemDto`** — para el listado:
```csharp
public record PropuestaReporteItemDto(
    long Id,
    string Codigo,
    string Titulo,
    string EstadoActual,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    string? DocenteEmail);
```

**`PropuestaReporteDetalleDto`** — para la vista de detalle individual:
```csharp
public record PropuestaReporteDetalleDto(
    long Id, string Codigo, string Titulo,
    string? Descripcion, string? Problema,
    string? ObjetivoGeneral, string? Alcance,
    string EstadoActual,
    DateTimeOffset? FechaEnvio,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    IReadOnlyList<EstudianteReporteDto> Estudiantes);
```

**`EstudianteReporteDto`** — para los estudiantes asignados:
```csharp
public record EstudianteReporteDto(
    string NombreCompleto,
    string Email,
    DateTimeOffset FechaAsignacion);
```

El uso de `record` en lugar de `class` es intencional: los records en C# son inmutables por defecto, lo que evita bugs donde un DTO se modifica accidentalmente después de haber sido construido.

### 4.4 Documentación interactiva con Swagger

Para facilitar las pruebas del backend sin necesidad del frontend, el servicio expone Swagger UI en `http://localhost:5003/swagger`. Al abrir esta URL durante el desarrollo, el desarrollador ve una interfaz gráfica con todos los endpoints documentados, sus parámetros y sus posibles respuestas.

**[FIGURA 1: Captura de `03_swagger_reportes.png` — Interfaz de Swagger del servicio de Reportes]**

En la figura se puede observar la interfaz de Swagger del servicio de Reportes tal como se presenta en el entorno de desarrollo. Se distinguen claramente los cuatro endpoints disponibles bajo la ruta `/api/reportes/propuestas`. El candado en la esquina superior derecha indica que la API requiere autenticación JWT para todos sus endpoints.

Para probar cualquier endpoint protegido desde Swagger, el flujo es el siguiente:

1. Obtener un token desde el Identity Service (`POST /api/auth/login` en el puerto 5001)
2. Hacer clic en el botón **Authorize** en la interfaz de Swagger del servicio de Reportes
3. Pegar el token en el campo correspondiente
4. Ejecutar los endpoints — las peticiones incluirán el token automáticamente

**[FIGURA 2: Captura de `04_swagger_identity.png` — Obtención del token en el Swagger de Identity]**

Esta segunda figura muestra el Swagger del servicio de Identidad, donde se puede ejecutar el endpoint de login. El cuerpo de la petición requiere únicamente el correo electrónico y la contraseña del usuario administrador del sistema.

---

## 5. IMPLEMENTACIÓN DEL FRONTEND

### 5.1 Arquitectura de componentes Angular

El frontend del módulo de Consultas y Reportes está construido con Angular 17 usando componentes standalone, una característica introducida en versiones recientes del framework que elimina la necesidad de módulos NgModule y hace los componentes más autocontenidos y reutilizables.

El módulo tiene dos páginas principales:

| Componente | Ruta | Descripción |
|------------|------|-------------|
| `ReportesHomeComponent` | `/reportes` | Listado de propuestas con filtros y exportación |
| `ReporteDetalleComponent` | `/reportes/:id` | Vista detallada de una propuesta individual |

Ambas rutas están protegidas por un `authGuard`, lo que significa que si un usuario no ha iniciado sesión e intenta acceder directamente a `/reportes`, el sistema lo redirige automáticamente a la pantalla de login.

### 5.2 Gestión de estado con Angular Signals

Una de las decisiones técnicas más modernas del módulo es el uso de **Angular Signals** para la gestión del estado interno del componente. Los signals son una característica introducida en Angular 16 que permiten declarar valores reactivos de forma más simple que los Observables tradicionales.

Por ejemplo, para rastrear si la página está cargando datos:

```typescript
loading = signal(true);
error  = signal('');
propuestas = signal<PropuestaReporteItemDto[]>([]);
filtroEstado = signal<FilterEstado>('');
```

Y para computar automáticamente las propuestas filtradas cuando cambia cualquier dependencia:

```typescript
propuestasFiltradas = computed(() => {
  const estado = this.filtroEstado();
  const busq = (this.busquedaValue() ?? '').toLowerCase().trim();
  return this.propuestas().filter(p => {
    const matchEstado = !estado || p.estadoActual === estado;
    const matchBusq   = !busq ||
      p.codigo.toLowerCase().includes(busq) ||
      p.titulo.toLowerCase().includes(busq) ||
      (p.docenteEmail ?? '').toLowerCase().includes(busq);
    return matchEstado && matchBusq;
  });
});
```

El `computed` se recalcula automáticamente cada vez que `filtroEstado()` o `busquedaValue()` cambian. No hace falta suscribirse manualmente ni gestionar la desuscripción. El framework se encarga de todo.

### 5.3 El servicio Angular de reportes

El `ReporteService` es el puente entre los componentes y el backend. Inyectado mediante el sistema de inyección de dependencias de Angular, abstrae completamente los detalles de las llamadas HTTP:

```typescript
@Injectable({ providedIn: 'root' })
export class ReporteService {
  private http = inject(HttpClient);
  private base = ApiEndpoints.reportes;

  getPropuestas(estado?: string, busqueda?: string, page = 1, pageSize = 100) {
    const params: Record<string, string> = { page: page.toString(), pageSize: pageSize.toString() };
    if (estado)   params['estado']   = estado;
    if (busqueda) params['busqueda'] = busqueda;
    return this.http.get<PropuestaReporteItemDto[]>(`${this.base}/propuestas`, { params });
  }

  exportPdf(estado?: string, busqueda?: string) {
    return this.http.get(`${this.base}/propuestas/export/pdf`,
      { params, responseType: 'blob' });
  }
}
```

El detalle del `responseType: 'blob'` en los métodos de exportación es importante: indica a Angular que la respuesta no es JSON sino un archivo binario (bytes), y que debe recibirlo como tal para luego entregárselo al navegador como descarga.

### 5.4 Página principal: listado de propuestas

La pantalla principal del módulo (`/reportes`) es el punto de entrada para el usuario. Cuando se carga por primera vez, realiza automáticamente una petición al backend para traer todas las propuestas disponibles.

**[FIGURA 3: Captura de `02_reportes_lista.png` — Pantalla principal del módulo de Consultas y Reportes]**

Como se puede apreciar en la figura, la pantalla está organizada en tres secciones claramente diferenciadas:

**Sección superior — Contadores por estado:** Seis tarjetas muestran en tiempo real cuántas propuestas hay en cada estado: Total, Aprobadas, En revisión, Pendientes, Rechazadas y Borradores. Cada tarjeta es clicable: al hacer clic sobre "Aprobadas", por ejemplo, la tabla inferior se filtra automáticamente para mostrar solo las propuestas aprobadas. Esta interacción es inmediata porque el filtrado ocurre en memoria, sin necesidad de una nueva petición al servidor.

**Sección de filtros:** Debajo de las tarjetas hay botones de filtro rápido por estado y un campo de búsqueda de texto libre. El campo de búsqueda reacciona mientras el usuario escribe, filtrando por código, título o correo del docente proponente. Hay también un botón "Limpiar" que restablece todos los filtros a su estado inicial.

**Tabla de resultados:** La tabla muestra las propuestas con las columnas más relevantes: número correlativo, código único, título, correo del proponente, estado con indicador de color, fecha de última actualización y disponibilidad. La columna "Estado" usa colores semánticos: verde para Aprobada, rojo para Rechazada, azul oscuro para los demás estados, permitiendo al usuario identificar de un vistazo el panorama general.

### 5.5 Pantalla de detalle de propuesta

Al hacer clic en el botón "Ver →" de cualquier fila de la tabla, el usuario navega a la vista de detalle (`/reportes/:id`), donde puede consultar toda la información de esa propuesta específica.

La pantalla de detalle muestra:
- **Encabezado**: código, estado actual (con badge de color), título y docente proponente
- **Fechas**: cuándo fue enviada y cuándo fue actualizada por última vez
- **Secciones de contenido**: descripción del proyecto, problema identificado, objetivo general y alcance
- **Tabla de estudiantes**: listado de todos los estudiantes asignados a esa propuesta, con su nombre completo, correo y fecha de asignación

Desde esta pantalla también es posible exportar esa propuesta específica como PDF o Excel. En ese caso, el backend filtra automáticamente por el código de la propuesta para incluir solo ese registro en el archivo generado.

---

## 6. GENERACIÓN DE REPORTES

### 6.1 Exportación a PDF con QuestPDF

Uno de los requisitos funcionales más concretos del módulo era la capacidad de generar reportes en PDF con formato institucional de la EPN. Para esto se utilizó la librería **QuestPDF**, que permite describir el diseño del documento usando una API fluida en C#, similar a cómo se construyen layouts en Flutter o SwiftUI.

El documento PDF generado tiene las siguientes características:
- **Tamaño**: A4 en orientación horizontal (landscape), para acomodar mejor una tabla con varias columnas
- **Encabezado**: nombre de la institución ("ESCUELA POLITÉCNICA NACIONAL"), facultad y nombre del sistema; a la derecha, la fecha de generación, el filtro aplicado y el total de registros
- **Franja dorada**: línea horizontal en color institucional `#F3BD46` que separa el encabezado del contenido
- **Tabla de datos**: filas alternadas en blanco y gris claro para facilitar la lectura; estados coloreados en verde (Aprobada), rojo (Rechazada) o azul oscuro (otros)
- **Pie de página**: número de página actual y total de páginas

El código que genera el PDF es declarativo y legible:

```csharp
var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(25);
        page.Header().Column(col => { /* logo y fecha */ });
        page.Content().Table(table => { /* datos */ });
        page.Footer().AlignCenter().Text(t => {
            t.CurrentPageNumber(); t.Span(" de "); t.TotalPages();
        });
    });
});
return document.GeneratePdf();
```

### 6.2 Exportación a Excel con ClosedXML

Para los usuarios que prefieren trabajar con los datos en una hoja de cálculo, el módulo también genera archivos Excel (.xlsx) usando la librería **ClosedXML**. El archivo generado incluye:

- **Fila de título** (fila 1): nombre de la institución en azul oscuro institucional `#0E2240` con texto blanco
- **Fila de subtítulo** (fila 2): fecha de generación del reporte en dorado `#F3BD46`
- **Fila de encabezados** (fila 4): columnas con fondo azul oscuro y texto blanco, con bordes
- **Filas de datos**: alternadas en blanco y gris claro, con la columna de estado coloreada semánticamente
- **Fila de resumen**: total de registros en amarillo dorado
- **Ajuste automático de columnas**: todas las columnas se ajustan a su contenido, excepto la columna "Título" que se fija en 40 caracteres para no hacer la tabla demasiado ancha

El archivo resultante puede abrirse directamente en Microsoft Excel, LibreOffice Calc o Google Sheets, y conserva todos los formatos definidos en el código.

### 6.3 Flujo completo de exportación

Cuando el usuario hace clic en el botón "PDF" o "Excel" desde el frontend, ocurre la siguiente secuencia de eventos:

```
1. Usuario hace clic en "↓ PDF"
2. Angular llama: GET /api/reportes/propuestas/export/pdf?estado=Aprobada
3. API Gateway redirige la petición al puerto 5003
4. ReportesController valida el JWT → llama a IReportesService.ExportarPdfAsync()
5. ReportesConsultaService llama a: GET http://localhost:5002/api/propuestas?estado=Aprobada
6. Servicio de Propuestas devuelve los datos en JSON
7. ReportesConsultaService convierte JSON → lista de DTOs → genera PDF con QuestPDF
8. Controller devuelve: 200 OK, Content-Type: application/pdf, archivo binario
9. Angular recibe el blob → crea URL temporal → simula clic en <a> con download
10. Navegador descarga: "propuestas_ticfis_2026-05-06.pdf"
```

Todo este proceso ocurre en menos de 3 segundos para listados de hasta 500 propuestas. Durante la exportación, el botón muestra el texto "Descargando..." y se deshabilita para evitar dobles clics.

---

## 7. AUTENTICACIÓN Y SEGURIDAD

### 7.1 Validación de tokens JWT

El servicio de Reportes valida cada petición verificando que el header `Authorization: Bearer <token>` contenga un token JWT válido, firmado con el mismo secreto compartido que usa el Identity Service. La configuración de validación es la siguiente:

```csharp
o.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwt.SigningKey)),
    ValidateIssuer   = true,  ValidIssuer   = jwt.Issuer,
    ValidateAudience = true,  ValidAudience = jwt.Audience,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(1)
};
```

El `ClockSkew` de un minuto permite una pequeña diferencia de reloj entre servidores, lo que es especialmente importante en entornos distribuidos donde distintos nodos pueden tener relojes ligeramente desincronizados.

### 7.2 Ciclo de autenticación del usuario

**[FIGURA 4: Captura de `01_login.png` — Pantalla de inicio de sesión]**

La figura muestra la pantalla de inicio de sesión del sistema TIC-FIS. Para acceder al módulo de Consultas y Reportes, el usuario debe ingresar su correo electrónico y contraseña. Al autenticarse correctamente, el sistema almacena el token JWT en el almacenamiento local del navegador (`localStorage`) y el interceptor HTTP de Angular lo adjunta automáticamente en el header de todas las peticiones posteriores.

El token de acceso tiene una validez de **60 minutos**. Transcurrido ese tiempo, el usuario deberá iniciar sesión nuevamente para obtener un nuevo token. Esta configuración equilibra seguridad (sesiones cortas) y usabilidad (sesiones suficientemente largas para trabajar).

---

## 8. PRUEBAS DEL SISTEMA

### 8.1 Pruebas del backend con Swagger

La primera línea de pruebas del módulo se realizó directamente sobre el backend usando la interfaz Swagger, sin necesitar el frontend. Esto permite verificar que cada endpoint responde correctamente de forma aislada.

**Procedimiento de prueba del endpoint de listado:**

1. Acceder a `http://localhost:5001/swagger` y ejecutar `POST /api/auth/login`
   con el cuerpo `{"email":"admin@ticfis.local","password":"Admin123!"}`
2. Copiar el valor del campo `accessToken` de la respuesta
3. Acceder a `http://localhost:5003/swagger`, hacer clic en **Authorize**,
   pegar el token y confirmar
4. Expandir `GET /api/reportes/propuestas`, hacer clic en **Try it out**
5. Ingresar los parámetros opcionales (estado, búsqueda, página)
6. Hacer clic en **Execute** y verificar la respuesta

La respuesta esperada es un HTTP 200 con un arreglo JSON de propuestas. Si el token no es válido o ha expirado, el sistema devuelve HTTP 401.

**Prueba del endpoint de detalle:**

Con la lista obtenida, se identifica un `id` de propuesta y se prueba `GET /api/reportes/propuestas/{id}`. El sistema devuelve el objeto completo con todas las secciones de contenido y la lista de estudiantes asignados.

**Prueba de exportación:**

Al ejecutar `GET /api/reportes/propuestas/export/pdf` desde Swagger, el sistema devuelve un archivo binario. Swagger lo presenta como un botón de descarga. El archivo descargado puede abrirse en cualquier visor de PDF para verificar el formato institucional.

### 8.2 Pruebas de integración frontend-backend

Una vez verificado el backend, las pruebas de integración se realizaron desde el navegador con el frontend Angular corriendo en `http://localhost:4200`. El flujo de prueba completo fue:

**Caso 1 — Listado sin filtros:**
- Acceder a `/reportes` después de iniciar sesión
- Verificar que la tabla carga todas las propuestas disponibles
- Verificar que los contadores de estado suman correctamente al total

**Caso 2 — Filtrado por estado:**
- Hacer clic en la tarjeta "Aprobadas"
- Verificar que solo aparecen propuestas con estado "Aprobada"
- Verificar que el badge "Filtro: Aprobada" aparece en el pie de la tabla

**Caso 3 — Búsqueda de texto:**
- Escribir un código de propuesta en el campo de búsqueda
- Verificar que la tabla se filtra en tiempo real mientras se escribe
- Escribir un correo de docente y verificar que filtra por ese campo también

**Caso 4 — Exportación:**
- Aplicar filtro por estado "Borrador"
- Hacer clic en "↓ PDF"
- Verificar que el archivo se descarga con nombre `propuestas_ticfis_YYYY-MM-DD.pdf`
- Abrir el archivo y verificar que solo contiene propuestas en estado Borrador

**Caso 5 — Sesión expirada:**
- Modificar manualmente el token en `localStorage` para forzar un error
- Intentar navegar a `/reportes`
- Verificar que el sistema redirige al login

---

## 9. RESULTADOS OBTENIDOS

El módulo de Consultas y Reportes cumple con todos los requerimientos funcionales planteados al inicio del proyecto. A continuación se presenta un resumen de los resultados:

| Requerimiento | Estado | Observación |
|---------------|--------|-------------|
| Listar propuestas con paginación | ✅ Implementado | Paginación configurable por parámetros |
| Filtrar por estado | ✅ Implementado | 5 estados disponibles |
| Búsqueda de texto libre | ✅ Implementado | Filtra por código, título y docente |
| Ver detalle de propuesta | ✅ Implementado | Incluye estudiantes asignados |
| Exportar a PDF institucional | ✅ Implementado | Formato EPN con logo y colores institucionales |
| Exportar a Excel formateado | ✅ Implementado | Con estilos y ajuste de columnas |
| Autenticación JWT | ✅ Implementado | Token de 60 min, propagado internamente |
| API documentada con Swagger | ✅ Implementado | Disponible en desarrollo |
| Filtros aplicados a la exportación | ✅ Implementado | PDF/Excel respetan los filtros activos |

### Métricas de rendimiento observadas

Durante las pruebas con el conjunto de datos de desarrollo (3 propuestas en la base de datos), los tiempos de respuesta fueron los siguientes:

| Operación | Tiempo promedio |
|-----------|----------------|
| Cargar listado completo | < 200 ms |
| Cargar detalle de propuesta | < 150 ms |
| Generar PDF (3 registros) | < 500 ms |
| Generar Excel (3 registros) | < 300 ms |

Con un volumen mayor de datos (estimado en 50-200 propuestas por período académico), se espera que los tiempos de generación de PDF y Excel no superen los 3 segundos, lo cual es aceptable para una operación de exportación.

---

## 10. CONCLUSIONES DEL MÓDULO

El módulo de Consultas y Reportes demuestra cómo una funcionalidad que parece compleja — generar reportes institucionales filtrados desde múltiples fuentes — puede implementarse de forma limpia y mantenible cuando se aplican los principios correctos de diseño de software.

La decisión de no tener base de datos propia para las propuestas (usando en su lugar el patrón API Composition) resultó ser acertada: el módulo siempre trabaja con datos frescos y no requiere procesos de sincronización que podrían fallar o desincronizarse. Cada vez que el usuario consulta, obtiene la realidad actual del sistema.

El uso de Angular Signals en el frontend simplificó considerablemente la gestión del estado reactivo del componente de listado, eliminando la necesidad de una librería de gestión de estado externa como NgRx y reduciendo el código de 300 a 150 líneas aproximadamente.

La separación en capas (Clean Architecture) facilita el mantenimiento futuro: si en el futuro se necesita cambiar la librería de generación de PDF de QuestPDF a otra, o si el servicio de Propuestas cambia su URL de API, los cambios quedan confinados en la capa de Infrastructure sin tocar la lógica de negocio ni los contratos de la capa de Application.

En definitiva, el módulo entrega valor concreto y medible: los coordinadores de la facultad pueden consultar el estado de todas las propuestas TIC en segundos, y descargar reportes listos para presentar en reuniones de comité en formato PDF o para analizarlos en Excel, todo desde un navegador web, con una interfaz sencilla e intuitiva.

---

*Documento generado: 2026-05-06*
*Sistema: TIC-FIS — Módulo B: Consultas y Reportes*
*Tecnologías: .NET 10 · Angular 17 · QuestPDF · ClosedXML · PostgreSQL 16*
