# Evidencia — Fragmentos de código fuente

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

## Introducción

Este documento conserva los fragmentos de código fuente más representativos del módulo,
referidos en el Capítulo 2 de la tesis. En el cuerpo principal se mantiene una
descripción breve de cada fragmento y el patrón de diseño aplicado; el código completo
se traslada aquí para reducir la extensión del documento **sin perder la evidencia
técnica**. Este material **no forma parte del documento principal de la tesis**.

---

## 1. Configuración de autenticación JWT Bearer (`Program.cs`) — Sprint 0

Centraliza la validación de emisor, audiencia, firma y tiempo de vida del token. El
`ClockSkew` de un minuto absorbe diferencias de reloj entre servidores.

```csharp
builder.Services.AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters =
            new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuer   = true, ValidIssuer   = "TicFis",
            ValidateAudience = true, ValidAudience = "TicFis",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

## 2. Interfaz del servicio de Reportes y DTO del listado — Sprint 1

Contrato del servicio (`IReportesService`) y DTO inmutable (`record`) del listado.
Aplica *Interface Segregation* exponiendo solo lo necesario al consumidor.

```csharp
// Reportes.Application/Abstractions/IReportesService.cs
public interface IReportesService {
    Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(
        string? authorizationHeader,
        string? estado, string? busqueda,
        int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<PropuestaReporteDetalleDto?> ObtenerPropuestaAsync(
        string? authorizationHeader, long id,
        CancellationToken cancellationToken = default);
}

// DTO del listado de propuestas
public record PropuestaReporteItemDto(
    long Id, string Codigo, string Titulo,
    string EstadoActual,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa, string? DocenteEmail);
```

## 3. DTO de detalle completo de propuesta y DTO de estudiante — Sprint 2

`PropuestaReporteDetalleDto` refleja las secciones del formulario F_AA_233A e incluye
la colección de `EstudianteReporteDto`.

```csharp
public record EstudianteReporteDto(
    string NombreCompleto, string Email,
    DateTimeOffset FechaAsignacion);

public record PropuestaReporteDetalleDto(
    long            Id,
    string          Codigo,         string          Titulo,
    string?         Descripcion,    string?         Problema,
    string?         ObjetivoGeneral, string?        Alcance,
    long            DocenteId,
    string          EstadoActual,
    DateTimeOffset? FechaEnvio,
    DateTimeOffset  FechaUltimaActualizacion,
    bool            Activa,
    IReadOnlyList<EstudianteReporteDto> Estudiantes);
```

## 4. Componente de detalle con Angular Signals — Sprint 2

Lee el `id` de la URL, realiza una única llamada al servicio y gestiona los tres estados
de la vista (`loading`, `error`, `propuesta`) con signals.

```typescript
export class ReporteDetalleComponent implements OnInit {
  private svc   = inject(ReporteService);
  private route = inject(ActivatedRoute);

  loading   = signal(true);
  error     = signal('');
  propuesta = signal<PropuestaReporteDetalleDto | null>(null);

  readonly letras = ['A', 'B', 'C', 'D', 'E'];  // etiquetas por estudiante

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.svc.getPropuestaById(id).subscribe({
      next:  (data) => { this.propuesta.set(data); this.loading.set(false); },
      error: ()     => {
        this.error.set('No se pudo cargar el detalle de la propuesta.');
        this.loading.set(false);
      },
    });
  }

  imprimir() { window.print(); }

  nombresEstudiantes(p: PropuestaReporteDetalleDto): string {
    return p.estudiantes.length > 0
      ? p.estudiantes.map(e => e.nombreCompleto).join(';  ')
      : 'Sin estudiantes asignados';
  }
}
```

## 5. Filtrado reactivo con `computed()` y signals — Sprint 3

`propuestasFiltradas` recalcula en memoria el subconjunto visible (por disponibilidad
y búsqueda) sin peticiones adicionales al servidor; los indicadores operan sobre la
colección completa.

```typescript
type FilterDisponibilidad = '' | 'Disponibles' | 'No disponibles';

filtroDisponibilidad = signal<FilterDisponibilidad>('');
busquedaValue  = toSignal(
    this.filtrosForm.controls.busqueda.valueChanges,
    { initialValue: '' });

propuestasFiltradas = computed(() => {
    const disp = this.filtroDisponibilidad();
    const busq = (this.busquedaValue() ?? '').toLowerCase().trim();
    return this.propuestas().filter(p => {
      const matchDisp = !disp
        || (disp === 'Disponibles'    &&  p.disponible)
        || (disp === 'No disponibles' && !p.disponible);
      const matchBusq   = !busq
        || p.codigo.toLowerCase().includes(busq)
        || p.titulo.toLowerCase().includes(busq)
        || (p.docenteEmail ?? '').toLowerCase().includes(busq);
      return matchDisp && matchBusq;
    });
  });

  // -- Ciclo de vida --
  ngOnInit() { this.cargar(); }

  cargar() {
    this.loading.set(true);
    this.error.set('');
    this.svc.getPropuestas().subscribe({
      next:  (data) => { this.propuestas.set(data); this.loading.set(false); },
      error: ()     => {
        this.error.set('Error al cargar los reportes. Verifique la conexion.');
        this.loading.set(false);
      },
    });
  }

  // -- Acciones de filtro --
  setEstado(e: FilterEstado) { this.filtroEstado.set(e); }

  limpiar() {
    this.filtrosForm.reset();
    this.filtroEstado.set('');
  }

  contar(estado: string) {
    return this.propuestas().filter(p => p.estadoActual === estado).length;
  }
}
```

## 6. Encabezado institucional del reporte PDF con QuestPDF — Sprint 5

API declarativa de QuestPDF con la identidad visual EPN (azul `#0E2240`, franja dorada
`#F3BD46`).

```csharp
page.Header().Column(col => {
    col.Item().Row(row => {
        row.RelativeItem().Column(c => {
            c.Item().Text("ESCUELA POLITECNICA NACIONAL")
                .FontSize(13).Bold()
                .FontColor(Color.FromHex("0E2240"));
            c.Item().Text("Facultad de Ingenieria en Sistemas")
                .FontSize(9);
            c.Item().Text("Sistema TIC-FIS - Reporte de Propuestas")
                .FontSize(9).Italic();
        });
        row.ConstantItem(150).AlignRight().Column(c => {
            c.Item().Text(
                $"Generado: {DateTimeOffset.Now:dd/MM/yyyy HH:mm}");
            c.Item().Text($"Filtro: {estado ?? "Todos"}");
            c.Item().Text($"Total: {items.Count} registros");
        });
    });
    // Franja dorada institucional separadora
    col.Item().Height(3).Background(Color.FromHex("F3BD46"));
});
```

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**.
