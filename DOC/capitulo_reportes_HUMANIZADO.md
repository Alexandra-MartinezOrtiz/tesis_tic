# CAPÍTULO X — IMPLEMENTACIÓN DEL MÓDULO DE CONSULTAS Y REPORTES

> **Nota para inserción en Word:** Los marcadores `[Figura X.N]` y `[Tabla X.N]` indican los puntos exactos donde insertar cada imagen o tabla. Los archivos de las capturas se encuentran en `DOC/capturas/2026-05-06_18-35/`. Reemplaza la `X` por el número real del capítulo.

---

## X.1 Introducción al módulo

Cuando se levantó el primer requerimiento del sistema TIC-FIS, la pregunta que apareció casi de inmediato fue: ¿cómo va a saber el coordinador de facultad en qué estado están todas las propuestas? La respuesta obvia —buscarlas una por una en cada expediente— era exactamente el problema que el sistema debía resolver. Antes de TIC-FIS, eso era precisamente lo que se hacía: hojas de cálculo bajadas de correos, informes escritos a mano en Word, cruce manual de información que podía tomar horas. El módulo de Consultas y Reportes nació para reemplazar ese proceso por completo.

En concreto, el módulo le permite a cualquier usuario autorizado del sistema ver el listado completo de propuestas de Trabajo de Integración Curricular, filtrarlas por estado o por cualquier término de texto, revisar el detalle de cada una, y —cuando lo necesita— descargar toda esa información ya formateada como un PDF listo para presentar en una reunión de comité.

Desde el punto de vista técnico, lo más interesante del módulo es una decisión que parece contraintuitiva al principio: el servicio de Reportes **no tiene base de datos de propuestas propia**. En lugar de guardar una copia de los datos, consulta al servicio de Propuestas en tiempo real cada vez que se necesita información. Esto se conoce como el patrón **API Composition** y tiene una ventaja muy clara: el reporte siempre muestra el estado real del sistema, sin ningún tipo de desfase ni proceso de sincronización que pueda fallar. La contrapartida es una dependencia en tiempo de ejecución entre los dos servicios, que se maneja cuidadosamente en la implementación.

---

## X.2 Arquitectura del servicio de Reportes

### X.2.1 Posición dentro del sistema TIC-FIS

El sistema TIC-FIS está construido como una arquitectura de microservicios: cada responsabilidad funcional vive en un servicio independiente, con su propia base de datos y su propio proceso. El módulo de Consultas y Reportes es el tercer servicio de esta arquitectura y opera en el puerto 5003.

```
┌──────────────────────────────────────────────────────────┐
│               USUARIO / NAVEGADOR Angular 17              │
│                   http://localhost:4200                    │
└─────────────────────────┬────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│              API GATEWAY (YARP)  —  Puerto 5000           │
│   /api/reportes/*   →   Reportes   (5003)                 │
│   /api/propuestas/* →   Propuestas (5002)                 │
│   /api/auth/*       →   Identity   (5001)                 │
└──────────┬──────────────┬──────────────┬─────────────────┘
           │              │              │
    ┌──────▼──────┐ ┌─────▼──────┐ ┌────▼────────────┐
    │  Identity   │ │ Propuestas │ │    Reportes     │
    │  Pto. 5001  │ │  Pto. 5002 │ │    Pto. 5003    │
    │ ticfis_iden │ │ ticfis_pro │ │ ticfis_reportes │
    └─────────────┘ └────────────┘ └────────────────┘
                           ▲                │
                           └────────────────┘
                     Reportes llama a Propuestas
                     (HTTP + JWT propagado)
```

El flujo es el siguiente: el navegador envía todas las peticiones al API Gateway en el puerto 5000. El gateway enruta las que empiezan por `/api/reportes/` hacia el servicio de Reportes. Cuando este necesita datos de propuestas, los pide directamente al servicio de Propuestas (puerto 5002) usando el mismo token JWT del usuario original, de modo que la autenticación se mantiene de punta a punta.

### X.2.2 Clean Architecture dentro del servicio

Internamente, el servicio de Reportes sigue los principios de la Arquitectura Limpia (*Clean Architecture*) propuesta por Robert C. Martin. La idea central es que las reglas de negocio no deben depender de los detalles de infraestructura —ni de la librería de PDF, ni del cliente HTTP, ni de la base de datos. Esto se logra separando el código en capas que solo dependen hacia adentro, nunca hacia afuera.

**[Tabla X.1 — Capas del servicio de Reportes]**

| Capa | Proyecto .NET | Responsabilidad principal |
|------|---------------|--------------------------|
| API | `Reportes.Api` | Controladores HTTP, configuración JWT, Swagger |
| Application | `Reportes.Application` | Contratos (interfaces), DTOs, casos de uso |
| Infrastructure | `Reportes.Infrastructure` | Llamadas HTTP a otros servicios, generación PDF, DbContext |

La regla más importante de esta arquitectura es que el controlador solo conoce la interfaz `IReportesService`. No sabe si los datos vienen de una base de datos, de una API externa o de cualquier otra fuente. La implementación concreta —`ReportesConsultaService`— vive en la capa de Infrastructure y puede cambiarse sin tocar ni una línea de la lógica de negocio.

### X.2.3 Tecnologías utilizadas

**[Tabla X.2 — Tecnologías del módulo de Consultas y Reportes]**

| Tecnología | Versión | Función dentro del módulo |
|-----------|---------|--------------------------|
| .NET / ASP.NET Core | 10 | Plataforma de ejecución del backend |
| QuestPDF | 2025.7.4 | Generación de documentos PDF con diseño fluido |
| Entity Framework Core + Npgsql | 10 | Acceso a la base de datos de auditoría |
| JWT Bearer (Microsoft) | — | Validación de tokens de acceso |
| Swashbuckle.AspNetCore | 10.1.7 | Documentación interactiva de la API |
| Angular | 17 | Frontend con componentes standalone |
| Angular Signals | — | Gestión de estado reactivo del componente de listado |

---

## X.3 Sprint 0 — Configuración del entorno de desarrollo

### X.3.1 Sprint Planning

El Sprint 0 no produce funcionalidad visible para el usuario final. Su objetivo es dejar listo el entorno para que el trabajo de los sprints siguientes pueda arrancar sin fricciones. En este sprint se acordaron las siguientes tareas:

**[Tabla X.3 — Backlog Sprint 0]**

| # | Tarea | Criterio de aceptación |
|---|-------|----------------------|
| T-01 | Levantar contenedor PostgreSQL 16 con Docker Compose | Contenedor activo y accesible en `localhost:5432` |
| T-02 | Crear la solución .NET con los cuatro proyectos de capa | `dotnet build` sin errores |
| T-03 | Configurar JWT Bearer con los parámetros del Identity Service | Swagger muestra candado; petición sin token recibe 401 |
| T-04 | Exponer Swagger UI con persistencia de autorización | El token ingresado en Swagger sobrevive a una recarga de página |
| T-05 | Configurar EF Core + Npgsql con convención snake_case | Migraciones aplicadas correctamente |

### X.3.2 Ejecución

La primera tarea fue levantar la infraestructura de base de datos. El sistema utiliza un único archivo `docker-compose.yml` en la raíz del proyecto que define todos los contenedores del sistema. Para el módulo de Reportes se aprovechó la misma instancia de PostgreSQL 16 que usan los otros servicios, creando una base de datos separada (`ticfis_reportes`) dentro del mismo servidor.

```bash
docker-compose up -d
```

Este comando descarga la imagen `postgres:16`, crea el volumen persistente y expone el puerto 5432. Los scripts SQL de inicialización, ubicados en `docker/postgres/sql/`, se ejecutan automáticamente en el primer arranque.

**[Figura X.1 — Contenedores Docker activos]**
*(Archivo: `SPRINT0_Docker_PostgreSQL_Activo.png`)*

La figura muestra el resultado del comando en la terminal: el contenedor `ticfis-postgres` está corriendo y el puerto `5432:5432` está mapeado correctamente. A partir de este momento, cualquier microservicio del proyecto puede conectarse a la base de datos usando las credenciales de desarrollo (`postgres`/`postgres`).

La configuración del proyecto .NET se hizo creando los cuatro proyectos que componen la Arquitectura Limpia:

```bash
dotnet new classlib -n Reportes.Domain
dotnet new classlib -n Reportes.Application
dotnet new classlib -n Reportes.Infrastructure
dotnet new webapi   -n Reportes.Api
```

Una vez definidas las dependencias entre proyectos (`Api → Application → Domain`, `Infrastructure → Application`), se configuró la autenticación JWT en `Program.cs`. El fragmento más importante de esta configuración es la validación de la firma del token:

```csharp
// Reportes.Api/Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuer   = true,  ValidIssuer   = "TicFis",
            ValidateAudience = true,  ValidAudience = "TicFis",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

El parámetro `ClockSkew` de un minuto es un detalle que puede parecer menor pero tiene importancia en entornos distribuidos: permite una pequeña diferencia de reloj entre los servidores sin que el token quede inmediatamente inválido. Sin este margen, un token generado en un servidor cuyo reloj va un segundo por detrás podría ser rechazado por otro servidor.

Para hacer más cómoda la experiencia durante el desarrollo, Swagger se configuró con autenticación persistente: el token ingresado no se pierde al recargar la página. Esto se logró inyectando un script JavaScript personalizado (`/swagger-auth.js`) que guarda el token en `localStorage` y lo recupera automáticamente al inicializar la interfaz.

**[Figura X.2 — Swagger con autorización JWT configurada]**
*(Archivo: `SPRINT0_Swagger_JWT_Autorizacion.png`)*

La figura muestra la interfaz de Swagger del servicio de Reportes con la autorización JWT ya ingresada. El ícono del candado en la esquina superior derecha aparece cerrado, lo que indica que todas las peticiones que se ejecuten desde la interfaz incluirán automáticamente el header `Authorization: Bearer <token>`. Antes de llegar a este punto, fue necesario obtener un token iniciando sesión desde el Swagger del servicio de Identidad (puerto 5001).

### X.3.3 Sprint Review

Al finalizar el Sprint 0, se verificaron todos los criterios de aceptación:

- El contenedor PostgreSQL responde correctamente en `localhost:5432`
- El comando `dotnet build` compila sin errores ni advertencias
- Una petición a cualquier endpoint sin token devuelve `HTTP 401 Unauthorized`
- El Swagger del servicio de Reportes muestra los endpoints disponibles y permite autenticarse de forma persistente

Este sprint no genera valor funcional por sí solo, pero sin él nada de lo que sigue funcionaría. Es la base sobre la que se construye todo.

### X.3.4 Retrospectiva

La configuración inicial del JWT requirió más iteraciones de las esperadas. El primer intento usaba la misma clave de firma (`SigningKey`) en los tres servicios, lo que es correcto, pero la clave estaba codificada directamente en el código fuente. Se corrigió inmediatamente llevándola al archivo `appsettings.json`, que no se incluye en el repositorio. Este ajuste estableció el patrón que se siguió en el resto del proyecto: ninguna credencial en el código, todo en configuración externa.

---

## X.4 Sprint 1 — Consulta de propuestas

### X.4.1 Sprint Planning

Con el entorno listo, el Sprint 1 se enfocó en entregar la funcionalidad central del módulo: que un usuario autenticado pueda consultar el listado de propuestas y ver el detalle de cualquiera de ellas. Esto implicó construir tanto el backend como la pantalla principal del frontend.

**[Tabla X.4 — Backlog Sprint 1]**

| # | Historia de usuario | Criterio de aceptación |
|---|--------------------|-----------------------|
| HU-01 | Como coordinador, quiero ver un listado de todas las propuestas TIC | GET devuelve arreglo JSON con todas las propuestas |
| HU-02 | Como coordinador, quiero filtrar propuestas por estado | Parámetro `?estado=borrador` devuelve solo borradores |
| HU-03 | Como coordinador, quiero buscar propuestas por código o título | Parámetro `?busqueda=texto` filtra correctamente |
| HU-04 | Como coordinador, quiero ver el detalle completo de una propuesta | GET `/{id}` devuelve toda la información de la propuesta |
| HU-05 | Como coordinador, quiero acceder al módulo desde el menú del sistema | La ruta `/reportes` en Angular está accesible y protegida |

### X.4.2 Ejecución

**Capa Application: los contratos**

Lo primero que se definió fue la interfaz que gobierna toda la lógica del módulo. Escribir la interfaz antes que la implementación es una práctica deliberada: obliga a pensar en *qué* se necesita antes de preocuparse por *cómo* se hace.

```csharp
// Reportes.Application/Abstractions/IReportesService.cs
public interface IReportesService
{
    Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(
        string? authorizationHeader, string? estado, string? busqueda,
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PropuestaReporteDetalleDto?> ObtenerPropuestaAsync(
        string? authorizationHeader, long id,
        CancellationToken cancellationToken = default);

    // Métodos de exportación definidos en Sprint 2
    Task<byte[]> ExportarPdfAsync(string? authorizationHeader, string? estado, string? busqueda, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarDetallePdfAsync(string? authorizationHeader, long id, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarFormulariosPdfAsync(string? authorizationHeader, string? estado, string? busqueda, CancellationToken cancellationToken = default);
}
```

Junto con la interfaz se definieron los DTOs (*Data Transfer Objects*) que transportan la información entre capas. Se usó la característica `record` de C# porque los records son inmutables por defecto: una vez creado el objeto, nadie puede modificarlo accidentalmente.

```csharp
// Reportes.Application/Dtos/ReporteDtos.cs
public record PropuestaReporteItemDto(
    long Id,
    string Codigo,
    string Titulo,
    string EstadoActual,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    string? DocenteEmail);

public record PropuestaReporteDetalleDto(
    long Id, string Codigo, string Titulo,
    string? Descripcion, string? Problema,
    string? ObjetivoGeneral, string? Alcance,
    long DocenteId, long DocenteUsuarioIdReferencia,
    string EstadoActual,
    DateTimeOffset? FechaEnvio,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    IReadOnlyList<EstudianteReporteDto> Estudiantes);

public record EstudianteReporteDto(
    string NombreCompleto,
    string Email,
    DateTimeOffset FechaAsignacion);
```

**Capa API: el controlador**

El controlador es intencionalmente delgado. No contiene lógica de negocio: solo recibe la petición HTTP, extrae los parámetros, llama al servicio y devuelve el resultado.

```csharp
// Reportes.Api/Controllers/ReportesController.cs
[ApiController]
[Route("api/reportes/propuestas")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportes;

    public ReportesController(IReportesService reportes) =>
        _reportes = reportes;

    private string? AuthHeader =>
        Request.Headers.Authorization.ToString();

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] string? busqueda,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var list = await _reportes.ListarPropuestasAsync(
            AuthHeader, estado, busqueda, page, pageSize, cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(
        long id, CancellationToken cancellationToken)
    {
        var det = await _reportes.ObtenerPropuestaAsync(
            AuthHeader, id, cancellationToken);
        return det is null ? NotFound() : Ok(det);
    }
}
```

Vale la pena detenerse en la propiedad `AuthHeader`. Su propósito es capturar el token JWT que llegó en la petición del usuario y reenviárselo al servicio de Propuestas cuando este servicio necesite consultarlo. De esta forma, el servicio de Propuestas puede verificar independientemente que el usuario tiene permiso para ver esos datos, sin necesidad de que el servicio de Reportes actúe como intermediario de autenticación. La cadena de autenticación se mantiene de extremo a extremo.

**Capa Infrastructure: la implementación**

La implementación del servicio crea un `HttpClient` configurado con el token del usuario y lo usa para llamar al servicio de Propuestas:

```csharp
// Reportes.Infrastructure/Services/ReportesConsultaService.cs
private HttpClient CreateClient(string? authorizationHeader)
{
    var client = _httpClientFactory.CreateClient();
    client.BaseAddress = new Uri(_propuestasBaseUrl + "/");
    if (!string.IsNullOrEmpty(authorizationHeader))
        client.DefaultRequestHeaders.Authorization =
            AuthenticationHeaderValue.Parse(authorizationHeader);
    return client;
}

public async Task<IReadOnlyList<PropuestaReporteItemDto>>
    ListarPropuestasAsync(string? authorizationHeader,
        string? estado, string? busqueda,
        int page, int pageSize,
        CancellationToken cancellationToken)
{
    using var client = CreateClient(authorizationHeader);
    var url = $"api/propuestas?page={page}&pageSize={pageSize}";
    if (!string.IsNullOrWhiteSpace(estado))
        url += $"&estado={Uri.EscapeDataString(estado)}";

    var response = await client.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();

    var items = await response.Content
        .ReadFromJsonAsync<List<PropuestaApiItemDto>>(cancellationToken)
        ?? [];

    // Filtro de texto libre aplicado en memoria
    if (!string.IsNullOrWhiteSpace(busqueda))
    {
        var q = busqueda.Trim().ToLowerInvariant();
        items = items.Where(p =>
            p.Titulo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            p.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    return items.Select(MapToItemDto).ToList();
}
```

El filtro por `estado` se envía como parámetro al servicio de Propuestas para reducir el volumen de datos que viajan por la red. El filtro por `busqueda`, en cambio, se aplica en memoria sobre los resultados ya recibidos. Esta distinción es deliberada: el servicio de Propuestas no expone búsqueda de texto completo en su API actual, de modo que la responsabilidad de filtrar por texto recae en el servicio de Reportes.

**Frontend Angular: la pantalla de listado**

En el lado del frontend, se creó el componente `ReportesHomeComponent` usando la arquitectura de componentes standalone de Angular 17. El estado interno del componente se maneja con **Angular Signals**, una característica introducida en Angular 16 que simplifica considerablemente la reactividad:

```typescript
// features/reportes/reportes-home.component.ts
@Component({ standalone: true, ... })
export class ReportesHomeComponent {
  private svc = inject(ReporteService);

  propuestas   = signal<PropuestaReporteItemDto[]>([]);
  loading      = signal(true);
  filtroEstado = signal<string>('');
  busqueda     = signal('');

  // Se recalcula automáticamente cuando cambia filtroEstado o busqueda
  propuestasFiltradas = computed(() => {
    const estado = this.filtroEstado();
    const q = this.busqueda().toLowerCase().trim();
    return this.propuestas().filter(p => {
      const matchEstado = !estado || p.estadoActual === estado;
      const matchBusq   = !q ||
        p.codigo.toLowerCase().includes(q) ||
        p.titulo.toLowerCase().includes(q) ||
        (p.docenteEmail ?? '').toLowerCase().includes(q);
      return matchEstado && matchBusq;
    });
  });
}
```

La propiedad `computed` es la parte más elegante de esta implementación: cada vez que el usuario cambia el filtro o escribe en el buscador, `propuestasFiltradas` se recalcula automáticamente, sin que haya que escribir ni una línea de código de suscripción ni de desuscripción. El framework se encarga de rastrear las dependencias reactivas por sí solo.

**[Figura X.3 — Endpoints del módulo de Consultas visibles en Swagger]**
*(Archivo: `SPRINT1_Endpoints_Consulta_Reportes.png`)*

La figura muestra la interfaz Swagger del servicio de Reportes con todos los endpoints disponibles expandidos. Se puede observar el listado de rutas bajo `/api/reportes/propuestas` y la indicación de que el servicio requiere autenticación Bearer en todos ellos. Esta vista fue la primera comprobación de que el controlador quedó correctamente registrado y enrutado.

Para probar el endpoint de listado en Swagger, se ejecutó `GET /api/reportes/propuestas` sin parámetros adicionales. La respuesta recibida fue:

```json
HTTP 200 OK

[
  {
    "id": 1,
    "codigo": "gergreg",
    "titulo": "gergrg",
    "estadoActual": "borrador",
    "fechaUltimaActualizacion": "2026-05-11T03:50:12.417706+00:00",
    "activa": true,
    "docenteEmail": "admin@ticfis.local"
  }
]
```

Esta respuesta confirmó que la cadena completa funcionaba: el servicio de Reportes recibió la petición, propagó el token al servicio de Propuestas, obtuvo la lista de propuestas y la devolvió formateada con los campos del DTO.

**[Figura X.4 — Módulo de Reportes en el frontend Angular]**
*(Archivo: `SPRINT1_Frontend_Modulo_Reportes.png`)*

La figura muestra la pantalla principal del módulo accedida desde el navegador en `http://localhost:4200/reportes`. Se pueden distinguir claramente tres zonas funcionales:

- **Tarjetas de resumen superiores:** muestran contadores por estado (Total, Aprobadas, En revisión, Pendientes, Rechazadas, Borradores). Cada tarjeta es interactiva: al hacer clic, aplica ese estado como filtro activo en la tabla inferior.
- **Barra de filtros:** campo de búsqueda de texto libre y botones de filtro rápido por estado, con el botón "Limpiar" que restablece todo.
- **Tabla de propuestas:** muestra código, título, docente proponente, estado con badge de color y fecha de actualización. En la captura se ven tres propuestas de prueba registradas durante el desarrollo.

### X.4.3 Sprint Review

Las historias de usuario HU-01 a HU-05 se marcaron como completadas. La demostración del sprint se realizó navegando en vivo por el módulo: se cargó el listado, se aplicó el filtro por estado "borrador", se escribió una búsqueda de texto y se verificó que la tabla se actualizaba en tiempo real sin hacer peticiones adicionales al servidor. Luego se hizo clic sobre una propuesta para ver su detalle completo.

El producto de este sprint es ya funcional y útil: un coordinador puede abrir el navegador, iniciar sesión y revisar el estado de todas las propuestas TIC en segundos.

### X.4.4 Retrospectiva

Durante la implementación del filtro de búsqueda de texto surgió una discusión de diseño: ¿debería el filtrado ocurrir en el servidor o en el cliente? Se exploró la opción de enviar el parámetro `busqueda` al servicio de Propuestas, pero ese servicio no tiene un endpoint de búsqueda de texto completo en su API actual. La alternativa de implementar la búsqueda en el frontend con `computed` resultó ser más rápida de entregar y funcionalmente equivalente para el volumen de datos esperado (decenas de propuestas por período académico, no miles). Se documentó la limitación para revisarla en el futuro si el volumen crece.

---

## X.5 Sprint 2 — Exportación de reportes

### X.5.1 Sprint Planning

El Sprint 2 agrega la funcionalidad de exportación: el usuario puede descargar el listado de propuestas (o el detalle de una propuesta individual) en formato PDF o Excel. Esta es la funcionalidad que más valor genera para los coordinadores, porque el resultado es un documento listo para presentar o archivar.

**[Tabla X.5 — Backlog Sprint 2]**

| # | Historia de usuario | Criterio de aceptación |
|---|--------------------|-----------------------|
| HU-06 | Como coordinador, quiero exportar el listado de propuestas a PDF con formato institucional EPN | El PDF descargado tiene encabezado con nombre de la facultad, colores institucionales y pie de página con numeración |
| HU-07 | Como coordinador, quiero que los filtros activos se reflejen en el reporte exportado | Si se filtra por "Aprobada", el PDF solo contiene propuestas aprobadas |
| HU-08 | Como coordinador, quiero exportar el detalle de una propuesta específica a PDF | GET `/{id}/export/pdf` devuelve el PDF solo de esa propuesta |
| HU-09 | Como coordinador, quiero exportar el formulario F_AA_233A en formato multipágina | El endpoint devuelve un PDF con una página por propuesta en formato de formulario |

### X.5.2 Ejecución

**Extensión del controlador**

Los endpoints de exportación se agregaron al controlador existente. Todos siguen el mismo patrón: reciben los parámetros de filtro, llaman al servicio, y devuelven el resultado binario con el tipo MIME y nombre de archivo correctos:

```csharp
[HttpGet("export/pdf")]
public async Task<IActionResult> ExportPdf(
    [FromQuery] string? estado,
    [FromQuery] string? busqueda,
    CancellationToken cancellationToken)
{
    var bytes = await _reportes.ExportarPdfAsync(
        AuthHeader, estado, busqueda, cancellationToken);
    var filename =
        $"propuestas_ticfis_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf";
    return File(bytes, "application/pdf", filename);
}

[HttpGet("{id:long}/export/pdf")]
public async Task<IActionResult> ExportDetallePdf(
    long id, CancellationToken cancellationToken)
{
    try
    {
        var bytes = await _reportes.ExportarDetallePdfAsync(
            AuthHeader, id, cancellationToken);
        return File(bytes, "application/pdf",
            $"propuesta_{id}_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf");
    }
    catch (InvalidOperationException)
    {
        return NotFound();
    }
}

[HttpGet("export/pdf-formularios")]
public async Task<IActionResult> ExportFormulariosPdf(
    [FromQuery] string? estado,
    [FromQuery] string? busqueda,
    CancellationToken cancellationToken)
{
    var bytes = await _reportes.ExportarFormulariosPdfAsync(
        AuthHeader, estado, busqueda, cancellationToken);
    var sufijo = !string.IsNullOrWhiteSpace(estado)
        ? $"_{estado.ToLower()}" : "_todos";
    return File(bytes, "application/pdf",
        $"formularios_f233a{sufijo}_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf");
}
```

**Generación de PDF con QuestPDF**

Para la generación de PDF se eligió la librería QuestPDF, que permite describir el diseño del documento de forma declarativa, similar a cómo se construyen layouts en Flutter. La ventaja frente a alternativas más antiguas (como iText o FastReport) es que el código es legible y refleja directamente la estructura visual del documento.

El PDF generado cumple con la identidad visual de la EPN: azul institucional `#0E2240` para encabezados y pie de página, dorado `#F3BD46` para la franja separadora. El documento tiene tamaño A4 en orientación horizontal, lo que permite acomodar cómodamente una tabla con varias columnas.

```csharp
// Reportes.Infrastructure/Services/ReportesConsultaService.cs
private static byte[] GenerarPdf(
    IReadOnlyList<PropuestaReporteItemDto> items,
    string? estado)
{
    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);

            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("ESCUELA POLITÉCNICA NACIONAL")
                            .FontSize(13).Bold()
                            .FontColor(Color.FromHex("0E2240"));
                        c.Item().Text("Facultad de Ingeniería en Sistemas")
                            .FontSize(9);
                        c.Item().Text("Sistema TIC-FIS — Reporte de Propuestas")
                            .FontSize(9).Italic();
                    });
                    row.ConstantItem(150).AlignRight().Column(c =>
                    {
                        c.Item().Text(
                            $"Generado: {DateTimeOffset.Now:dd/MM/yyyy HH:mm}");
                        c.Item().Text($"Filtro: {estado ?? "Todos"}");
                        c.Item().Text($"Total: {items.Count} registros");
                    });
                });
                // Franja dorada institucional
                col.Item().Height(3).Background(Color.FromHex("F3BD46"));
            });

            page.Content().Table(table =>
            {
                // Definición de columnas y filas de datos
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });
    });

    return document.GeneratePdf();
}
```

**[Figura X.5 — Endpoint de exportación PDF ejecutado en Swagger]**
*(Archivo: `SPRINT2_Exportacion_PDF_Swagger.png`)*

La figura muestra la ejecución del endpoint `GET /api/reportes/propuestas/export/pdf` desde Swagger. El sistema respondió con `HTTP 200 OK` y un cuerpo binario de **65.663 bytes**. Swagger presenta este resultado como un botón de descarga. El archivo descargado tiene el nombre `propuestas_ticfis_2026-05-11.pdf`, que incluye la fecha de generación en el nombre para facilitar la identificación del archivo.

**[Figura X.6 — Detalle de propuesta individual]**
*(Archivo: `SPRINT2_Endpoint_Detalle_Propuesta.png`)*

*(Archivo: `SPRINT2_Endpoint_Detalle_Propuesta.png`)*

La figura muestra la respuesta del endpoint `GET /api/reportes/propuestas/1`. El sistema devolvió el objeto de detalle completo de la propuesta con id 1. Algunos campos tienen valor nulo (`fechaEnvio: null`, `estudiantes: []`) porque la propuesta estaba en estado borrador y aún no había sido enviada ni tenía estudiantes asignados. Esta respuesta confirmó que la deserialización tolerante a fallos implementada con `JsonDocument.TryGetProperty` funcionaba correctamente: el sistema no lanza excepción cuando encuentra campos nulos.

**Flujo completo de descarga desde el frontend**

Cuando el usuario hace clic en "↓ PDF" en la pantalla del navegador, el frontend ejecuta la siguiente secuencia:

```typescript
// features/reportes/reportes-home.component.ts
descargarPdf(): void {
  this.descargandoPdf.set(true);
  const estado = this.filtroEstado() || undefined;
  const busq   = this.busqueda()     || undefined;

  this.svc.exportPdf(estado, busq).subscribe({
    next: blob => {
      const url = URL.createObjectURL(blob);
      const a   = document.createElement('a');
      a.href     = url;
      a.download =
        `propuestas_ticfis_${new Date().toISOString().slice(0, 10)}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
      this.descargandoPdf.set(false);
    },
    error: () => this.descargandoPdf.set(false)
  });
}
```

El truco de crear un elemento `<a>` dinámico, asignarle el atributo `download` y simular un clic es el mecanismo estándar de los navegadores para descargar archivos binarios recibidos como `Blob`. El botón se deshabilita durante la descarga (`descargandoPdf.set(true)`) para evitar que el usuario genere múltiples descargas simultáneas.

### X.5.3 Sprint Review

Las cuatro historias de usuario del Sprint 2 quedaron completadas. La demostración se realizó en vivo:

1. Se abrió el módulo de reportes en `localhost:4200/reportes` con las tres propuestas de prueba cargadas
2. Se hizo clic en "↓ PDF" sin filtro activo — el navegador descargó el archivo `propuestas_ticfis_2026-05-11.pdf`
3. Se abrió el PDF descargado: el encabezado mostraba "ESCUELA POLITÉCNICA NACIONAL" en azul oscuro, la franja dorada separadora y los datos de la propuesta en la tabla
4. Se repitió con el filtro "borrador" activo — el PDF descargado solo contenía las propuestas en borrador

### X.5.4 Retrospectiva

La implementación del PDF fue la tarea más laboriosa del sprint. QuestPDF tiene una curva de aprendizaje inicial por su API declarativa, que es diferente al enfoque imperativo de otras librerías. Sin embargo, una vez superada esa curva, el código resultante es mucho más legible y mantenible que las alternativas. Si en el futuro se necesita cambiar el diseño del PDF —por ejemplo, agregar el logo oficial de la facultad— el cambio es quirúrgico y no afecta a nada más.

Un ajuste que se hizo durante el sprint fue cambiar el formato de página de A4 vertical a A4 horizontal. Con orientación vertical, la columna de "Título" quedaba demasiado estrecha y los textos largos se cortaban en muchas líneas. Con orientación horizontal el resultado es más cómodo de leer.

---

## X.6 Diseño de la base de datos `ticfis_reportes`

### X.6.1 Principio de diseño: sin duplicación de propuestas

Una de las decisiones que más discusión generó durante el diseño del módulo fue si el servicio de Reportes debía mantener su propia copia de las propuestas. La opción de duplicar los datos tiene atractivos evidentes: si el servicio de Propuestas está caído, el servicio de Reportes podría seguir respondiendo desde su copia local.

Sin embargo, se eligió el patrón **API Composition** en lugar de la replicación, por varias razones:

- **Consistencia garantizada:** el reporte siempre muestra el estado real de las propuestas, sin riesgo de que la copia local esté desactualizada
- **Simplicidad:** no hay proceso de sincronización que pueda fallar o quedar desfasado
- **Menor complejidad operacional:** no se necesita configurar eventos, colas de mensajes ni jobs de sincronización

La base de datos `ticfis_reportes` existe, pero se usa solo para funciones que genuinamente requieren persistencia local: caché de referencia, historial de reportes generados y auditoría.

### X.6.2 Esquema completo

**[Tabla X.6 — Tablas de la base de datos `ticfis_reportes`]**

| Tabla | Propósito |
|-------|-----------|
| `periodos_academicos` | Períodos académicos de referencia para filtros |
| `propuestas_cache` | Caché opcional de propuestas para escenarios offline |
| `estudiantes_cache` | Caché de estudiantes vinculados a propuestas |
| `propuesta_estudiantes` | Relación N:M entre propuestas y estudiantes en caché |
| `auditoria_reportes` | Registro de cada reporte generado (quién, cuándo, qué filtros) |

La tabla más relevante desde el punto de vista de la trazabilidad es `auditoria_reportes`:

```sql
CREATE TABLE IF NOT EXISTS auditoria_reportes (
    auditoria_id    BIGSERIAL        PRIMARY KEY,
    usuario_email   VARCHAR(150)     NOT NULL,
    formato         formato_reporte  NOT NULL,  -- 'pdf' o 'excel'
    filtro_estado   estado_propuesta,
    filtro_busqueda VARCHAR(200),
    total_registros INTEGER          NOT NULL DEFAULT 0,
    generado_en     TIMESTAMPTZ      NOT NULL DEFAULT now(),
    duracion_ms     INTEGER
);
```

Esta tabla registra automáticamente (mediante la función PL/pgSQL `registrar_auditoria_reporte()`) cada vez que alguien genera un reporte: el usuario que lo solicitó, el formato elegido (PDF o Excel), los filtros que aplicó y cuántos registros incluyó el reporte resultante. Con esta información es posible responder preguntas de auditoría como "¿Qué reportes generó el coordinador durante el período de evaluación?".

### X.6.3 Enumeraciones de dominio

El esquema define dos tipos enumerados nativos de PostgreSQL para garantizar la integridad de los datos:

```sql
CREATE TYPE estado_propuesta AS ENUM (
    'borrador',
    'en_revision',
    'pendiente_aprobacion',
    'aprobada',
    'rechazada'
);

CREATE TYPE formato_reporte AS ENUM ('pdf', 'excel');
```

El uso de tipos `ENUM` nativos en lugar de columnas `VARCHAR` tiene una ventaja importante: el motor de base de datos rechaza cualquier valor que no esté en la lista, sin necesidad de validación adicional en la aplicación.

### X.6.4 Índices y rendimiento

Para la búsqueda de texto en la tabla de caché se creó un índice GIN con la extensión `pg_trgm` de PostgreSQL:

```sql
CREATE INDEX idx_propuestas_cache_texto
    ON propuestas_cache USING gin (
        (codigo || ' ' || titulo) gin_trgm_ops
    );
```

Este índice permite búsquedas por similitud de texto (trigrams) en la columna concatenada `codigo || titulo`. Es mucho más eficiente que un `LIKE '%texto%'` convencional porque aprovecha la estructura del índice GIN en lugar de escanear toda la tabla. Para el volumen esperado de datos (cientos de propuestas), la diferencia en tiempo de respuesta sería imperceptible en el estado actual, pero es una práctica que prevé el crecimiento del sistema.

### X.6.5 Vistas de consulta

Se definieron tres vistas que encapsulan las consultas más frecuentes:

- **`v_propuestas_reporte`:** listado de propuestas con conteo de estudiantes asignados — la vista principal del módulo
- **`v_propuesta_detalle`:** detalle completo de una propuesta incluyendo todos sus estudiantes — usada por el endpoint de detalle individual
- **`v_estadisticas_estado`:** contadores de propuestas agrupadas por estado — alimenta las tarjetas de resumen del frontend

---

## X.7 Relación con el servicio de Identidad

### X.7.1 El flujo de autenticación extremo a extremo

El servicio de Identidad es el único componente del sistema que emite tokens JWT. Todos los demás servicios solo los validan, nunca los emiten. Esto establece una división clara de responsabilidades: el servicio de Identidad es la autoridad de autenticación; el servicio de Reportes es un consumidor que confía en esa autoridad.

El flujo completo de una petición autenticada es el siguiente:

```
1. Usuario ingresa credenciales en el frontend Angular
   └─→ POST /api/auth/login  (Gateway 5000 → Identity 5001)
       └─→ Identity verifica credenciales en ticfis_identity
           └─→ Emite JWT firmado con SigningKey compartido
               └─→ { accessToken: "eyJ...", refreshToken: "..." }

2. Frontend guarda el accessToken en localStorage
   └─→ Interceptor HTTP adjunta Authorization: Bearer eyJ...
       en cada petición subsiguiente

3. Usuario solicita exportación PDF
   └─→ GET /api/reportes/propuestas/export/pdf
       (Gateway 5000 → Reportes 5003)
       └─→ Middleware JWT de Reportes valida el token:
           ✓ Firma verificada con SigningKey
           ✓ Issuer = "TicFis", Audience = "TicFis"
           ✓ Token no expirado (AccessTokenMinutes = 60)
       └─→ ReportesController.ExportPdf() se ejecuta
           └─→ Extrae token original: AuthHeader
               └─→ GET /api/propuestas (Propuestas 5002, mismo token)
                   └─→ Middleware JWT de Propuestas valida (igual proceso)
                   └─→ Devuelve datos de propuestas en JSON
               └─→ Reportes genera PDF y devuelve bytes al cliente
```

### X.7.2 La clave compartida: arquitectura de confianza

El mecanismo que hace funcionar este esquema es que los tres servicios comparten el mismo valor de `Jwt:SigningKey` en sus archivos de configuración respectivos. Ningún servicio necesita llamar al servicio de Identidad para verificar un token: puede hacerlo de forma local e independiente porque conoce la clave con la que fue firmado.

```json
// appsettings.json (idéntico en Identity, Propuestas y Reportes)
{
  "Jwt": {
    "SigningKey":         "[clave secreta compartida — no incluir en repositorio]",
    "Issuer":            "TicFis",
    "Audience":          "TicFis",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays":   7
  }
}
```

Esta arquitectura de confianza tiene una implicación operacional importante: si en algún momento se necesita rotar la clave de firma, todos los servicios deben actualizarse simultáneamente. De lo contrario, los tokens emitidos con la nueva clave serían rechazados por los servicios que aún tienen la clave anterior.

### X.7.3 Propagación del token: autenticación en cascada

Cuando el servicio de Reportes llama al servicio de Propuestas, usa exactamente el mismo token que recibió del usuario original. No genera un token propio ni impersona al usuario: reenvía la identidad del usuario tal como viene.

```csharp
// El token llega en la petición del usuario
private string? AuthHeader =>
    Request.Headers.Authorization.ToString();

// Se propaga literalmente al cliente HTTP interno
client.DefaultRequestHeaders.Authorization =
    AuthenticationHeaderValue.Parse(authorizationHeader);
```

Esta decisión tiene una consecuencia importante: si el usuario tiene permisos restringidos en el servicio de Propuestas (por ejemplo, solo puede ver sus propias propuestas), esa restricción se respeta automáticamente en el reporte generado. El servicio de Reportes no puede ver más de lo que el usuario tiene derecho a ver.

---

## X.8 Resultados y verificación

**[Tabla X.7 — Verificación de requerimientos funcionales]**

| Requerimiento | Estado | Evidencia |
|--------------|--------|-----------|
| Listar propuestas con paginación | Implementado | Figura X.3: respuesta JSON con array de propuestas |
| Filtrar por estado | Implementado | Parámetro `?estado=borrador` validado en Swagger |
| Búsqueda de texto libre | Implementado | Filtrado en memoria por código, título y email |
| Ver detalle de propuesta | Implementado | Figura X.6: respuesta JSON con detalle completo |
| Exportar a PDF institucional | Implementado | Figura X.5: archivo de 65.663 bytes descargado |
| Exportar detalle individual PDF | Implementado | Endpoint `/{id}/export/pdf` |
| Exportar formulario F_AA_233A | Implementado | Endpoint `/export/pdf-formularios` |
| Autenticación JWT extremo a extremo | Implementado | 401 sin token; token propagado al servicio de Propuestas |
| Auditoría de reportes generados | Implementado | Tabla `auditoria_reportes` con función PL/pgSQL |
| Interfaz Angular con filtros reactivos | Implementado | Figura X.4: módulo visible en localhost:4200/reportes |

**Métricas de rendimiento en entorno de desarrollo**

Las pruebas se realizaron con el conjunto de datos de desarrollo (tres propuestas registradas). Los tiempos de respuesta observados fueron:

| Operación | Tiempo observado |
|-----------|-----------------|
| Cargar listado completo | < 200 ms |
| Cargar detalle de propuesta | < 150 ms |
| Generar PDF (3 registros, 65 KB) | < 500 ms |

Con el volumen real esperado —entre 50 y 200 propuestas por período académico— el tiempo de generación del PDF no debería superar los 3 segundos, lo cual es perfectamente aceptable para una operación de exportación que el usuario realiza de forma esporádica.

---

## X.9 Conclusiones del módulo

El módulo de Consultas y Reportes es, de cierta forma, la cara visible del sistema TIC-FIS para los coordinadores de la facultad: es el lugar al que acuden cuando necesitan saber qué está pasando con las propuestas. Por eso era importante que funcionara bien no solo técnicamente sino también en términos de experiencia de uso.

La decisión arquitectónica más significativa del módulo —no tener base de datos propia de propuestas y usar el patrón API Composition— resultó ser la correcta. El módulo siempre muestra información actual, sin procesos de sincronización que mantener ni riesgos de inconsistencia. El costo de esa decisión —una dependencia en tiempo de ejecución con el servicio de Propuestas— es manejable en el contexto de este sistema.

El uso de Angular Signals en el frontend redujo considerablemente la complejidad del componente de listado. La reactividad del filtrado y la búsqueda se logra sin suscripciones manuales, sin `ngOnDestroy` para limpiar subscripciones, y con un código que refleja directamente la intención: el listado filtrado es una función del filtro activo y del texto de búsqueda.

La Arquitectura Limpia del backend cumplió su promesa de mantenibilidad: cuando durante el Sprint 2 se necesitó ajustar la librería de generación de PDF a QuestPDF 2025.7.4, el cambio quedó completamente encapsulado en la capa de Infrastructure. Ni el controlador ni la capa de Application tuvieron que modificarse.

En definitiva, el módulo entrega el valor que se prometió al inicio: los coordinadores de la facultad pueden consultar el estado de todas las propuestas TIC en segundos y descargar un reporte PDF listo para presentar en reuniones de comité, todo desde el navegador, sin necesidad de herramientas adicionales ni conocimientos técnicos.

---

*Sistema TIC-FIS — Módulo de Consultas y Reportes*
*Tecnologías: .NET 10 · Angular 17 · QuestPDF 2025.7.4 · PostgreSQL 16*
