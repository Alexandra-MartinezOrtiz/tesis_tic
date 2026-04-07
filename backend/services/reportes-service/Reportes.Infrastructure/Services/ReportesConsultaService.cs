using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;
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

    public async Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(string? authorizationHeader, string? estado, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(authorizationHeader);
        var url = $"api/propuestas?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(estado))
            url += $"&estado={Uri.EscapeDataString(estado)}";
        var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<PropuestaReporteItemDto>>(json, JsonOptions);
        return list ?? new List<PropuestaReporteItemDto>();
    }

    public async Task<PropuestaReporteDetalleDto?> ObtenerPropuestaAsync(string? authorizationHeader, long id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(authorizationHeader);
        var response = await client.GetAsync($"api/propuestas/{id}", cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<PropuestaReporteDetalleDto>(json, JsonOptions);
    }

    public async Task<byte[]> ExportarPdfAsync(string? authorizationHeader, string? estado, CancellationToken cancellationToken = default)
    {
        var items = await ListarPropuestasAsync(authorizationHeader, estado, 1, 500, cancellationToken).ConfigureAwait(false);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Reporte de propuestas TIC-FIS").SemiBold().FontSize(18);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Element(CellStyle).Text("Código");
                        h.Cell().Element(CellStyle).Text("Título");
                        h.Cell().Element(CellStyle).Text("Estado");
                        h.Cell().Element(CellStyle).Text("Actualización");
                        static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(4);
                    });
                    foreach (var p in items)
                    {
                        table.Cell().PaddingVertical(4).Text(p.Codigo);
                        table.Cell().PaddingVertical(4).Text(p.Titulo);
                        table.Cell().PaddingVertical(4).Text(p.EstadoActual);
                        table.Cell().PaddingVertical(4).Text(p.FechaUltimaActualizacion.ToString("u"));
                    }
                });
            });
        });
        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportarExcelAsync(string? authorizationHeader, string? estado, CancellationToken cancellationToken = default)
    {
        var items = await ListarPropuestasAsync(authorizationHeader, estado, 1, 500, cancellationToken).ConfigureAwait(false);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Propuestas");
        ws.Cell(1, 1).Value = "Código";
        ws.Cell(1, 2).Value = "Título";
        ws.Cell(1, 3).Value = "Estado";
        ws.Cell(1, 4).Value = "Última actualización";
        var row = 2;
        foreach (var p in items)
        {
            ws.Cell(row, 1).Value = p.Codigo;
            ws.Cell(row, 2).Value = p.Titulo;
            ws.Cell(row, 3).Value = p.EstadoActual;
            ws.Cell(row, 4).Value = p.FechaUltimaActualizacion.LocalDateTime;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
