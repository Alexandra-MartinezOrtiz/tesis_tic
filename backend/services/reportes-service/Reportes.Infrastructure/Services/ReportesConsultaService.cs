using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reportes.Application.Abstractions;
using Reportes.Application.Dtos;

namespace Reportes.Infrastructure.Services;

public class ReportesConsultaService : IReportesService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    // El módulo de Consultas y Reportes trabaja únicamente con propuestas aprobadas.
    private const string EstadoAprobada = "aprobada";
    // Cupo máximo de estudiantes por propuesta aprobada.
    private const int CupoMaximo = 5;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ReportesConsultaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private HttpClient CreateClient(string? authorizationHeader)
    {
        var baseUrl = _configuration["PropuestasApi:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("PropuestasApi:BaseUrl no configurado.");
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl + "/");
        if (!string.IsNullOrEmpty(authorizationHeader))
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
        return client;
    }

    public async Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(
        string? authorizationHeader, string? estado, string? busqueda,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(authorizationHeader);
        // Este módulo siempre consulta propuestas aprobadas, sin importar el parámetro recibido.
        var url = $"api/propuestas?page={page}&pageSize={pageSize}&estado={Uri.EscapeDataString(EstadoAprobada)}";
        var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<PropuestaReporteItemDto>>(json, JsonOptions)
            ?? new List<PropuestaReporteItemDto>();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var q = busqueda.Trim().ToLowerInvariant();
            list = list.Where(p =>
                p.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Titulo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (p.DocenteEmail != null && p.DocenteEmail.Contains(q, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Calcula cupos y disponibilidad a partir de los estudiantes propuestos.
        return list
            .Select(p =>
            {
                var ocupados = Math.Clamp(p.EstudiantesPropuestos, 0, CupoMaximo);
                return p with
                {
                    EstudiantesPropuestos = ocupados,
                    CupoMaximo = CupoMaximo,
                    Disponible = ocupados < CupoMaximo,
                };
            })
            .ToList();
    }

    public async Task<PropuestaReporteDetalleDto?> ObtenerPropuestaAsync(
        string? authorizationHeader, long id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(authorizationHeader);
        var response = await client.GetAsync($"api/propuestas/{id}", cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Deserialize using an intermediate type that matches the Propuestas API shape
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var estudiantes = new List<EstudianteReporteDto>();
        if (root.TryGetProperty("estudiantes", out var estArr) && estArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in estArr.EnumerateArray())
            {
                var nombre = e.TryGetProperty("nombreCompleto", out var n) ? n.GetString() ?? "" : "";
                var email = e.TryGetProperty("email", out var em) ? em.GetString() ?? "" : "";
                var fecha = e.TryGetProperty("fechaAsignacion", out var f) && f.TryGetDateTimeOffset(out var dt)
                    ? dt : DateTimeOffset.MinValue;
                estudiantes.Add(new EstudianteReporteDto(nombre, email, fecha));
            }
        }

        var estudiantesPropuestos = root.TryGetProperty("estudiantesPropuestos", out var ep) && ep.TryGetInt32(out var epVal)
            ? Math.Clamp(epVal, 0, CupoMaximo)
            : 0;

        return new PropuestaReporteDetalleDto(
            root.TryGetProperty("id", out var idProp) ? idProp.GetInt64() : 0,
            root.TryGetProperty("codigo", out var cod) ? cod.GetString() ?? "" : "",
            root.TryGetProperty("titulo", out var tit) ? tit.GetString() ?? "" : "",
            root.TryGetProperty("descripcion", out var desc) ? desc.GetString() : null,
            root.TryGetProperty("problema", out var prob) ? prob.GetString() : null,
            root.TryGetProperty("objetivoGeneral", out var obj) ? obj.GetString() : null,
            root.TryGetProperty("alcance", out var alc) ? alc.GetString() : null,
            root.TryGetProperty("docenteId", out var did) ? did.GetInt64() : 0,
            root.TryGetProperty("docenteUsuarioIdReferencia", out var dref) ? dref.GetInt64() : 0,
            root.TryGetProperty("estadoActual", out var est) ? est.GetString() ?? "" : "",
            root.TryGetProperty("fechaEnvio", out var fe) && fe.ValueKind != JsonValueKind.Null && fe.TryGetDateTimeOffset(out var feDto) ? feDto : null,
            root.TryGetProperty("fechaUltimaActualizacion", out var fua) && fua.TryGetDateTimeOffset(out var fuaDto) ? fuaDto : DateTimeOffset.MinValue,
            root.TryGetProperty("activa", out var act) ? act.GetBoolean() : true,
            estudiantesPropuestos,
            CupoMaximo,
            estudiantesPropuestos < CupoMaximo,
            estudiantes,
            root.TryGetProperty("carrera", out var carr) ? carr.GetString() : null,
            root.TryGetProperty("asignaturas", out var asig) ? asig.GetString() : null,
            root.TryGetProperty("autorizadoPor", out var autp) ? autp.GetString() : null,
            root.TryGetProperty("fechaAutorizacion", out var fau) ? fau.GetString() : null,
            root.TryGetProperty("presentadoPor", out var pp) ? pp.GetString() : null,
            root.TryGetProperty("estudiantesNombres", out var en) ? en.GetString() : null,
            root.TryGetProperty("resolucionCpgic", out var rc) ? rc.GetString() : null,
            root.TryGetProperty("presidenteCpgic", out var pc) ? pc.GetString() : null,
            root.TryGetProperty("fechaAprobacion", out var fap) ? fap.GetString() : null
        );
    }

    public async Task<byte[]> ExportarPdfAsync(
        string? authorizationHeader, string? estado, string? busqueda, CancellationToken cancellationToken = default)
    {
        var items = await ListarPropuestasAsync(authorizationHeader, estado, busqueda, 1, 1000, cancellationToken).ConfigureAwait(false);
        var generado = DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ESCUELA POLITÉCNICA NACIONAL")
                                .Bold().FontSize(11).FontColor("#0E2240");
                            c.Item().Text("Facultad de Ingeniería en Sistemas")
                                .FontSize(9).FontColor("#616161");
                            c.Item().Text("Sistema TIC-FIS — Reporte de Propuestas Aprobadas")
                                .Bold().FontSize(10).FontColor("#0E2240");
                        });
                        row.ConstantItem(160).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Generado: {generado}").FontSize(8).FontColor("#616161");
                            c.Item().Text("Solo propuestas aprobadas").FontSize(8).FontColor("#616161");
                            c.Item().Text($"Total aprobadas: {items.Count}").Bold().FontSize(9);
                        });
                    });
                    col.Item().PaddingTop(4).LineHorizontal(2).LineColor("#F3BD46");
                    col.Item().PaddingBottom(8);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30);
                        c.ConstantColumn(80);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.ConstantColumn(45);
                        c.ConstantColumn(60);
                        c.ConstantColumn(80);
                    });

                    table.Header(h =>
                    {
                        static IContainer HeaderCell(IContainer c) => c
                            .Background("#0E2240").Padding(5)
                            .DefaultTextStyle(x => x.Bold().FontColor("#FFFFFF").FontSize(8));

                        h.Cell().Element(HeaderCell).Text("#");
                        h.Cell().Element(HeaderCell).Text("Código");
                        h.Cell().Element(HeaderCell).Text("Título");
                        h.Cell().Element(HeaderCell).Text("Proponente");
                        h.Cell().Element(HeaderCell).Text("Cupos");
                        h.Cell().Element(HeaderCell).Text("Disponible");
                        h.Cell().Element(HeaderCell).Text("Últ. actualización");
                    });

                    var alterno = false;
                    var num = 1;
                    foreach (var p in items)
                    {
                        var bg = alterno ? "#F7F9FC" : "#FFFFFF";
                        alterno = !alterno;

                        static IContainer DataCell(IContainer c, string bg) =>
                            c.Background(bg).Padding(4).BorderBottom(0.5f).BorderColor("#E0E0E0");

                        table.Cell().Element(c => DataCell(c, bg)).Text(num++.ToString()).FontColor("#9E9E9E");
                        table.Cell().Element(c => DataCell(c, bg)).Text(p.Codigo).Bold();
                        table.Cell().Element(c => DataCell(c, bg)).Text(p.Titulo);
                        table.Cell().Element(c => DataCell(c, bg)).Text(p.DocenteEmail ?? "—").FontColor("#616161");
                        table.Cell().Element(c => DataCell(c, bg)).Text($"{p.EstudiantesPropuestos}/{p.CupoMaximo}").Bold();
                        table.Cell().Element(c => DataCell(c, bg)).Text(p.Disponible ? "Sí" : "No")
                            .Bold().FontColor(p.Disponible ? "#2E7D32" : "#9E9E9E");
                        table.Cell().Element(c => DataCell(c, bg)).Text(
                            p.FechaUltimaActualizacion.LocalDateTime.ToString("dd/MM/yyyy")).FontColor("#616161");
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ").FontSize(8).FontColor("#9E9E9E");
                    t.CurrentPageNumber().FontSize(8).FontColor("#9E9E9E");
                    t.Span(" de ").FontSize(8).FontColor("#9E9E9E");
                    t.TotalPages().FontSize(8).FontColor("#9E9E9E");
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportarDetallePdfAsync(
        string? authorizationHeader, long id, CancellationToken cancellationToken = default)
    {
        var p = await ObtenerPropuestaAsync(authorizationHeader, id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Propuesta {id} no encontrada.");

        var letras = new[] { "A", "B", "C", "D", "E" };

        // Estilo neutro tipo documento Word (gris claro / negro), igual al formulario oficial F_AA_233A.
        static IContainer CeldaSeccion(IContainer c) =>
            c.Background("#D9D9D9").Border(0.5f).BorderColor("#000000").Padding(5);

        static IContainer CeldaLabel(IContainer c) =>
            c.Background("#FFFFFF").Padding(5).Border(0.5f).BorderColor("#000000");

        static IContainer CeldaValor(IContainer c) =>
            c.Background("#FFFFFF").Padding(5).Border(0.5f).BorderColor("#000000");

        static IContainer CeldaActHdr(IContainer c) =>
            c.Background("#D9D9D9").Padding(4).Border(0.5f).BorderColor("#000000");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                // ── ENCABEZADO OFICIAL ──
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ESCUELA POLITÉCNICA NACIONAL")
                                .Bold().FontSize(11).FontColor("#000000");
                            c.Item().Text("Facultad de Ingeniería de Sistemas")
                                .FontSize(8).FontColor("#333333");
                        });
                        row.ConstantItem(75).AlignRight()
                            .Text("F_AA_233A").Bold().FontSize(10).FontColor("#000000");
                    });
                    col.Item().PaddingTop(3).AlignCenter()
                        .Text("CONSEJO DE DOCENCIA")
                        .Bold().FontSize(10).FontColor("#000000");
                    col.Item().AlignCenter()
                        .Text("FORMULARIO DEL PROYECTO DE TRABAJO DE INTEGRACIÓN CURRICULAR")
                        .Bold().FontSize(9).FontColor("#000000");
                    col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor("#000000");
                    col.Item().PaddingBottom(5);
                });

                page.Content().Column(col =>
                {
                    // ── DATOS GENERALES ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(2.6f);
                        });

                        table.Cell().ColumnSpan(2).Element(CeldaSeccion)
                            .Text("DATOS GENERALES").Bold().FontColor("#000000").FontSize(9);

                        void Fila(string etiqueta, string valor)
                        {
                            table.Cell().Element(CeldaLabel).Text(etiqueta).Bold().FontSize(8.5f);
                            table.Cell().Element(CeldaValor).Text(valor).FontSize(8.5f);
                        }

                        Fila("Unidad Académica:", "Facultad de Ingeniería de Sistemas");
                        Fila("Carrera:", string.IsNullOrWhiteSpace(p.Carrera) ? "(No especificada)" : p.Carrera);
                        Fila("Proyecto:", p.Titulo);
                        Fila("Número de participantes:", p.EstudiantesPropuestos.ToString());
                        Fila("Cupos / Disponible:", $"{p.EstudiantesPropuestos}/{p.CupoMaximo}   —   {(p.Disponible ? "Sí" : "No")}");
                        Fila("Departamento:", "Departamento de Informática y Ciencias de la Computación");
                        Fila("Asignaturas:",
                            string.IsNullOrWhiteSpace(p.Asignaturas) ? "(No especificadas)" : p.Asignaturas);
                        Fila("Profesor:", $"Docente (ref. usuario #{p.DocenteUsuarioIdReferencia})");
                    });

                    col.Item().PaddingTop(5);

                    // ── DESCRIPCIÓN DEL PROYECTO ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => c.RelativeColumn());

                        table.Cell().Element(CeldaSeccion)
                            .Text("DESCRIPCIÓN DEL PROYECTO").Bold().FontColor("#000000").FontSize(9);

                        var sb = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(p.Descripcion))
                            sb.Append(p.Descripcion);
                        if (!string.IsNullOrWhiteSpace(p.Problema))
                        {
                            if (sb.Length > 0) sb.Append("\n\n");
                            sb.Append("Problema identificado:\n").Append(p.Problema);
                        }

                        table.Cell().Element(CeldaValor).MinHeight(55)
                            .Text(sb.Length > 0 ? sb.ToString() : "—")
                            .FontSize(9).LineHeight(1.5f);
                    });

                    col.Item().PaddingTop(5);

                    // ── OBJETIVO DEL PROYECTO ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => c.RelativeColumn());
                        table.Cell().Element(CeldaSeccion)
                            .Text("OBJETIVO DEL PROYECTO").Bold().FontColor("#000000").FontSize(9);
                        table.Cell().Element(CeldaValor).MinHeight(35)
                            .Text(!string.IsNullOrWhiteSpace(p.ObjetivoGeneral) ? p.ObjetivoGeneral : "—")
                            .FontSize(9).LineHeight(1.5f);
                    });

                    col.Item().PaddingTop(5);

                    // ── ALCANCE DEL PROYECTO ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => c.RelativeColumn());
                        table.Cell().Element(CeldaSeccion)
                            .Text("ALCANCE DEL PROYECTO").Bold().FontColor("#000000").FontSize(9);
                        table.Cell().Element(CeldaValor).MinHeight(40)
                            .Text(!string.IsNullOrWhiteSpace(p.Alcance) ? p.Alcance : "—")
                            .FontSize(9).LineHeight(1.5f);
                    });

                    col.Item().PaddingTop(5);

                    // ── COMPONENTES, ACTIVIDADES Y PRODUCTOS ──
                    col.Item().Element(CeldaSeccion)
                        .Text("COMPONENTES, ACTIVIDADES Y PRODUCTOS")
                        .Bold().FontColor("#000000").FontSize(9);

                    var slots = Math.Clamp(p.EstudiantesPropuestos, 0, CupoMaximo);
                    if (slots == 0)
                    {
                        col.Item().Background("#FFF8E1").Padding(8)
                            .Text($"Sin estudiantes propuestos. La propuesta está disponible para asignación (0/{p.CupoMaximo}).")
                            .FontSize(9).FontColor("#F57F17").Italic();
                    }
                    else
                    {
                        for (var i = 0; i < slots; i++)
                        {
                            var letra = i < letras.Length ? letras[i] : (i + 1).ToString();
                            var nombre = p.Estudiantes.Count > i ? p.Estudiantes[i].NombreCompleto : "";

                            col.Item().PaddingTop(5).Column(estCol =>
                            {
                                // Cabecera del estudiante (texto plano, estilo documento)
                                estCol.Item().Border(0.5f).BorderColor("#000000").Padding(5)
                                    .Text($"Estudiante {letra}:")
                                    .Bold().FontSize(9).FontColor("#000000");

                                // Módulo / Componente (plantilla en blanco)
                                estCol.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(2.6f); });
                                    t.Cell().Element(CeldaLabel)
                                        .Text("Módulo / Componente").Bold().FontSize(8.5f).FontColor("#000000");
                                    t.Cell().Element(CeldaValor).MinHeight(20).Text("");
                                });

                                // Tabla de actividades (No. / Actividades específicas / Horas + Total)
                                estCol.Item().PaddingTop(2).Text("Actividades específicas y horas asignadas:")
                                    .Bold().FontSize(8.5f).FontColor("#000000");
                                estCol.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(c =>
                                    {
                                        c.ConstantColumn(28);
                                        c.RelativeColumn();
                                        c.ConstantColumn(45);
                                    });

                                    t.Cell().Element(CeldaActHdr).AlignCenter().Text("No.").Bold().FontSize(8);
                                    t.Cell().Element(CeldaActHdr).Text("Actividades específicas").Bold().FontSize(8);
                                    t.Cell().Element(CeldaActHdr).AlignCenter().Text("Horas").Bold().FontSize(8);

                                    for (var r = 1; r <= 10; r++)
                                    {
                                        t.Cell().Border(0.5f).BorderColor("#000000").Padding(2).AlignCenter()
                                            .Text(r.ToString()).FontSize(8).FontColor("#000000");
                                        t.Cell().Height(13).Border(0.5f).BorderColor("#000000");
                                        t.Cell().Height(13).Border(0.5f).BorderColor("#000000");
                                    }

                                    t.Cell().ColumnSpan(2).Element(CeldaActHdr).AlignRight().Text("Total").Bold().FontSize(8);
                                    t.Cell().Element(CeldaActHdr).Text("");
                                });

                                // Productos y nombre del estudiante propuesto
                                estCol.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(2.6f); });
                                    t.Cell().Element(CeldaLabel)
                                        .Text("Producto(s) esperado(s)").Bold().FontSize(8.5f).FontColor("#000000");
                                    t.Cell().Element(CeldaValor).MinHeight(26).Text("");
                                    t.Cell().Element(CeldaLabel)
                                        .Text("Nombre del estudiante propuesto").Bold().FontSize(8.5f).FontColor("#000000");
                                    t.Cell().Element(CeldaValor).MinHeight(18).Text(nombre).FontSize(8.5f);
                                });
                            });
                        }
                    }

                    col.Item().PaddingTop(8);

                    // ── SOLICITUD DE PARTICIPACIÓN < 2 o > 5 ESTUDIANTES (Opcional) ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(2.6f); });
                        table.Cell().ColumnSpan(2).Element(CeldaSeccion)
                            .Text("SOLICITUD DE PARTICIPACIÓN DE MENOS DE 2 O MÁS DE 5 ESTUDIANTES (Opcional)")
                            .Bold().FontColor("#000000").FontSize(9);
                        table.Cell().Element(CeldaLabel).Text("Autorizado por:").Bold().FontSize(8.5f);
                        table.Cell().Element(CeldaValor).MinHeight(22).Text(p.AutorizadoPor ?? "").FontSize(8.5f);
                        table.Cell().Element(CeldaLabel).Text("Fecha:").Bold().FontSize(8.5f);
                        table.Cell().Element(CeldaValor).MinHeight(18).Text(p.FechaAutorizacion ?? "").FontSize(8.5f);
                    });

                    col.Item().PaddingTop(8);

                    // ── RECOMENDACIONES Y APROBACIONES ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(2.6f);
                        });

                        table.Cell().ColumnSpan(2).Element(CeldaSeccion)
                            .Text("RECOMENDACIONES Y APROBACIONES")
                            .Bold().FontColor("#000000").FontSize(9);

                        void AprobFila(string etiqueta, string valor, float minHeight = 20)
                        {
                            table.Cell().Element(CeldaLabel).Text(etiqueta).Bold().FontSize(8.5f);
                            table.Cell().Element(CeldaValor).MinHeight(minHeight)
                                .Text(valor).FontSize(8.5f).FontColor("#424242");
                        }

                        AprobFila("Presentado por:",
                            !string.IsNullOrWhiteSpace(p.PresentadoPor)
                                ? p.PresentadoPor
                                : $"Docente (ref. usuario #{p.DocenteUsuarioIdReferencia})");
                        AprobFila("Estudiantes propuestos:",
                            !string.IsNullOrWhiteSpace(p.EstudiantesNombres)
                                ? p.EstudiantesNombres
                                : (p.Estudiantes.Count > 0
                                    ? string.Join(";  ", p.Estudiantes.Select(e => e.NombreCompleto))
                                    : ""));
                        AprobFila("Resolución de la CPGIC:", p.ResolucionCpgic ?? "", 40);
                        AprobFila("Presidente de la CPGIC:", p.PresidenteCpgic ?? "", 30);
                        AprobFila("Fecha de aprobación:",
                            !string.IsNullOrWhiteSpace(p.FechaAprobacion)
                                ? p.FechaAprobacion
                                : (p.FechaEnvio.HasValue
                                    ? p.FechaEnvio.Value.LocalDateTime.ToString("dd/MM/yyyy")
                                    : ""));
                    });
                });

                // ── PIE DE PÁGINA ──
                page.Footer().Row(row =>
                {
                    row.RelativeItem()
                        .Text("Sistema TIC-FIS — Escuela Politécnica Nacional")
                        .FontSize(7.5f).FontColor("#9E9E9E");
                    row.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("Página ").FontSize(7.5f).FontColor("#9E9E9E");
                        t.CurrentPageNumber().FontSize(7.5f).FontColor("#9E9E9E");
                        t.Span(" de ").FontSize(7.5f).FontColor("#9E9E9E");
                        t.TotalPages().FontSize(7.5f).FontColor("#9E9E9E");
                    });
                    row.RelativeItem().AlignRight()
                        .Text($"F_AA_233A  |  {p.Codigo}")
                        .FontSize(7.5f).FontColor("#9E9E9E");
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportarFormulariosPdfAsync(
        string? authorizationHeader, string? estado, string? busqueda, CancellationToken cancellationToken = default)
    {
        // Obtener lista filtrada y luego el detalle completo de cada una
        var items = await ListarPropuestasAsync(authorizationHeader, estado, busqueda, 1, 500, cancellationToken)
            .ConfigureAwait(false);

        var propuestas = new List<PropuestaReporteDetalleDto>();
        foreach (var item in items)
        {
            var detalle = await ObtenerPropuestaAsync(authorizationHeader, item.Id, cancellationToken)
                .ConfigureAwait(false);
            if (detalle is not null) propuestas.Add(detalle);
        }

        var generado = DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm");
        var letras   = new[] { "A", "B", "C", "D", "E" };
        var estadoLabel = !string.IsNullOrWhiteSpace(estado) ? $" — {estado}" : " — Todos los estados";

        static IContainer CS(IContainer c) => c.Background("#0E2240").Padding(5);
        static IContainer CL(IContainer c) => c.Background("#E8EDF4").Padding(5).Border(0.5f).BorderColor("#CCCCCC");
        static IContainer CV(IContainer c) => c.Background("#FFFFFF").Padding(5).Border(0.5f).BorderColor("#CCCCCC");
        static IContainer CA(IContainer c) => c.Background("#E8EDF4").Padding(4).Border(0.5f).BorderColor("#CCCCCC");

        var document = Document.Create(container =>
        {
            // Portada / resumen
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(60).AlignCenter()
                        .Text("ESCUELA POLITÉCNICA NACIONAL")
                        .Bold().FontSize(18).FontColor("#0E2240");
                    col.Item().AlignCenter()
                        .Text("Facultad de Ingeniería en Sistemas")
                        .FontSize(11).FontColor("#616161");
                    col.Item().PaddingTop(8).AlignCenter().LineHorizontal(2).LineColor("#F3BD46");
                    col.Item().PaddingTop(20).AlignCenter()
                        .Text("FORMULARIOS DE PROYECTOS DE TRABAJO DE INTEGRACIÓN CURRICULAR")
                        .Bold().FontSize(13).FontColor("#0E2240");
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text("F_AA_233A — Consejo de Docencia")
                        .FontSize(10).FontColor("#616161");
                    col.Item().PaddingTop(40).AlignCenter()
                        .Text($"Filtro: {estadoLabel.TrimStart(' ', '—').Trim()}")
                        .Bold().FontSize(11).FontColor("#0E2240");
                    col.Item().PaddingTop(8).AlignCenter()
                        .Text($"Total de formularios: {propuestas.Count}")
                        .FontSize(10).FontColor("#424242");
                    col.Item().PaddingTop(80).AlignCenter()
                        .Text($"Generado por Sistema TIC-FIS: {generado}")
                        .FontSize(8).FontColor("#9E9E9E").Italic();
                });
            });

            // Una página F_AA_233A por cada propuesta
            foreach (var p in propuestas)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ESCUELA POLITÉCNICA NACIONAL")
                                    .Bold().FontSize(11).FontColor("#0E2240");
                                c.Item().Text("Facultad de Ingeniería de Sistemas")
                                    .FontSize(8).FontColor("#616161");
                            });
                            row.ConstantItem(75).AlignRight()
                                .Text("F_AA_233A").Bold().FontSize(10).FontColor("#0E2240");
                        });
                        col.Item().PaddingTop(3).AlignCenter()
                            .Text("CONSEJO DE DOCENCIA").Bold().FontSize(10).FontColor("#0E2240");
                        col.Item().AlignCenter()
                            .Text("FORMULARIO DEL PROYECTO DE TRABAJO DE INTEGRACIÓN CURRICULAR")
                            .Bold().FontSize(9).FontColor("#0E2240");
                        col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor("#0E2240");
                        col.Item().PaddingBottom(5);
                    });

                    page.Content().Column(col =>
                    {
                        // DATOS GENERALES
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(2.6f); });
                            table.Cell().ColumnSpan(2).Element(CS)
                                .Text("DATOS GENERALES").Bold().FontColor("#FFFFFF").FontSize(9);

                            void F(string lbl, string val) {
                                table.Cell().Element(CL).Text(lbl).Bold().FontSize(8.5f);
                                table.Cell().Element(CV).Text(val).FontSize(8.5f);
                            }
                            F("Unidad Académica:", "Facultad de Ingeniería en Sistemas (FIS)");
                            F("Carrera:", string.IsNullOrWhiteSpace(p.Carrera) ? "(No especificada)" : p.Carrera);
                            F("Proyecto:", p.Titulo);
                            F("Estudiantes propuestos:", $"{p.EstudiantesPropuestos} de {p.CupoMaximo}");
                            F("Cupos / Disponible:", $"{p.EstudiantesPropuestos}/{p.CupoMaximo}   —   {(p.Disponible ? "Sí" : "No")}");
                            F("Departamento:", "Departamento de Informática y Ciencias de la Computación");
                            F("Asignaturas:", string.IsNullOrWhiteSpace(p.Asignaturas) ? "(No especificadas)" : p.Asignaturas);
                            F("Profesor:", $"Docente (ref. usuario #{p.DocenteUsuarioIdReferencia})");

                            // Estado resaltado
                            table.Cell().Element(CL).Text("Estado del proyecto:").Bold().FontSize(8.5f);
                            var estadoColor = p.EstadoActual switch {
                                "Aprobada"   => "#2E7D32",
                                "Rechazada"  => "#E31D1A",
                                "EnRevision" => "#1565C0",
                                "Pendiente"  => "#E65100",
                                _            => "#616161",
                            };
                            table.Cell().Element(CV)
                                .Text($"{p.EstadoActual}   |   Código: {p.Codigo}")
                                .Bold().FontSize(8.5f).FontColor(estadoColor);
                        });

                        col.Item().PaddingTop(4);

                        // DESCRIPCIÓN
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => c.RelativeColumn());
                            table.Cell().Element(CS)
                                .Text("DESCRIPCIÓN DEL PROYECTO").Bold().FontColor("#FFFFFF").FontSize(9);
                            var sb = new System.Text.StringBuilder();
                            if (!string.IsNullOrWhiteSpace(p.Descripcion)) sb.Append(p.Descripcion);
                            if (!string.IsNullOrWhiteSpace(p.Problema))
                            { if (sb.Length > 0) sb.Append("\n\n"); sb.Append("Problema: ").Append(p.Problema); }
                            if (!string.IsNullOrWhiteSpace(p.ObjetivoGeneral))
                            { if (sb.Length > 0) sb.Append("\n\n"); sb.Append("Objetivo general: ").Append(p.ObjetivoGeneral); }
                            table.Cell().Element(CV).MinHeight(45)
                                .Text(sb.Length > 0 ? sb.ToString() : "—").FontSize(9).LineHeight(1.5f);
                        });

                        col.Item().PaddingTop(4);

                        // ALCANCE
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => c.RelativeColumn());
                            table.Cell().Element(CS)
                                .Text("ALCANCE DEL PROYECTO").Bold().FontColor("#FFFFFF").FontSize(9);
                            table.Cell().Element(CV).MinHeight(30)
                                .Text(!string.IsNullOrWhiteSpace(p.Alcance) ? p.Alcance : "—")
                                .FontSize(9).LineHeight(1.5f);
                        });

                        col.Item().PaddingTop(4);

                        // COMPONENTES
                        col.Item().Element(CS)
                            .Text("COMPONENTES, ACTIVIDADES ESPECÍFICAS Y PRODUCTOS")
                            .Bold().FontColor("#FFFFFF").FontSize(9);

                        if (p.Estudiantes.Count == 0)
                        {
                            col.Item().Background("#FFF8E1").Padding(7)
                                .Text("Sin estudiantes asignados.")
                                .FontSize(9).FontColor("#F57F17").Italic();
                        }
                        else
                        {
                            for (var i = 0; i < p.Estudiantes.Count; i++)
                            {
                                var est = p.Estudiantes[i];
                                var letra = i < letras.Length ? letras[i] : (i + 1).ToString();
                                col.Item().PaddingTop(3).Column(ec =>
                                {
                                    ec.Item().Background("#F3BD46").Padding(4)
                                        .Text($"Estudiante {letra}:").Bold().FontSize(9).FontColor("#0E2240");
                                    ec.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(c => c.RelativeColumn());
                                        t.Cell().Element(CL).Text("Componente").Bold().FontSize(8.5f);
                                        t.Cell().Element(CV).MinHeight(16)
                                            .Text("(Pendiente — módulo de gestión)")
                                            .FontSize(8.5f).FontColor("#9E9E9E").Italic();
                                        t.Cell().Element(CL).Text("Actividades específicas y horas asignadas").Bold().FontSize(8.5f);
                                    });
                                    ec.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(c => { c.ConstantColumn(28); c.RelativeColumn(); c.ConstantColumn(45); });
                                        t.Cell().Element(CA).AlignCenter().Text("No.").Bold().FontSize(8);
                                        t.Cell().Element(CA).Text("Actividades específicas").Bold().FontSize(8);
                                        t.Cell().Element(CA).AlignCenter().Text("Horas").Bold().FontSize(8);
                                        for (var r = 0; r < 3; r++)
                                        {
                                            t.Cell().Height(13).Border(0.5f).BorderColor("#E0E0E0");
                                            t.Cell().Height(13).Border(0.5f).BorderColor("#E0E0E0");
                                            t.Cell().Height(13).Border(0.5f).BorderColor("#E0E0E0");
                                        }
                                    });
                                    ec.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(c => c.RelativeColumn());
                                        t.Cell().Element(CL).Text("Producto(s) esperado(s)").Bold().FontSize(8.5f);
                                        t.Cell().Element(CV).MinHeight(16)
                                            .Text("(Pendiente — módulo de gestión)")
                                            .FontSize(8.5f).FontColor("#9E9E9E").Italic();
                                        t.Cell().Background("#F7F9FC").Padding(4).Border(0.5f).BorderColor("#CCCCCC")
                                            .Text($"{est.NombreCompleto}  |  {est.Email}  |  Asignado: {est.FechaAsignacion.LocalDateTime:dd/MM/yyyy}")
                                            .FontSize(8f);
                                    });
                                });
                            }
                        }

                        col.Item().PaddingTop(6);

                        // APROBACIONES
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(2.6f); });
                            table.Cell().ColumnSpan(2).Element(CS)
                                .Text("RECOMENDACIONES Y APROBACIONES").Bold().FontColor("#FFFFFF").FontSize(9);

                            void A(string lbl, string val, bool blank = false) {
                                table.Cell().Element(CL).Text(lbl).Bold().FontSize(8.5f);
                                if (blank)
                                    table.Cell().Element(CV).MinHeight(30);
                                else
                                    table.Cell().Element(CV).Text(val).FontSize(8.5f)
                                        .FontColor(string.IsNullOrEmpty(val) ? "#BDBDBD" : "#424242");
                            }
                            A("Presentado por:", !string.IsNullOrWhiteSpace(p.PresentadoPor)
                                ? p.PresentadoPor
                                : $"Docente (ref. #{p.DocenteUsuarioIdReferencia})");
                            A("Resolución CPGIC:", p.ResolucionCpgic ?? "");
                            A("Presidente CPGIC:", p.PresidenteCpgic ?? "");
                            A("Fecha de aprobación:", !string.IsNullOrWhiteSpace(p.FechaAprobacion)
                                ? p.FechaAprobacion
                                : (p.FechaEnvio.HasValue ? p.FechaEnvio.Value.LocalDateTime.ToString("dd/MM/yyyy") : ""));
                            A("Estudiantes propuestos:", !string.IsNullOrWhiteSpace(p.EstudiantesNombres)
                                ? p.EstudiantesNombres
                                : (p.Estudiantes.Count > 0
                                    ? string.Join(";  ", p.Estudiantes.Select(e => e.NombreCompleto))
                                    : "Sin estudiantes asignados"));
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Sistema TIC-FIS — Escuela Politécnica Nacional")
                            .FontSize(7.5f).FontColor("#9E9E9E");
                        row.RelativeItem().AlignCenter().Text(t =>
                        {
                            t.Span("Página ").FontSize(7.5f).FontColor("#9E9E9E");
                            t.CurrentPageNumber().FontSize(7.5f).FontColor("#9E9E9E");
                            t.Span(" de ").FontSize(7.5f).FontColor("#9E9E9E");
                            t.TotalPages().FontSize(7.5f).FontColor("#9E9E9E");
                        });
                        row.RelativeItem().AlignRight()
                            .Text($"F_AA_233A  |  {p.Codigo}")
                            .FontSize(7.5f).FontColor("#9E9E9E");
                    });
                });
            }
        });

        return document.GeneratePdf();
    }
}
