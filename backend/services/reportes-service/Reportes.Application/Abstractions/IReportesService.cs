using Reportes.Application.Dtos;

namespace Reportes.Application.Abstractions;

public interface IReportesService
{
    Task<IReadOnlyList<PropuestaReporteItemDto>> ListarPropuestasAsync(string? authorizationHeader, string? estado, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PropuestaReporteDetalleDto?> ObtenerPropuestaAsync(string? authorizationHeader, long id, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarPdfAsync(string? authorizationHeader, string? estado, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarExcelAsync(string? authorizationHeader, string? estado, CancellationToken cancellationToken = default);
}
