using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reportes.Application.Abstractions;

namespace Reportes.Api.Controllers;

[ApiController]
[Route("api/reportes/propuestas")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportes;

    public ReportesController(IReportesService reportes)
    {
        _reportes = reportes;
    }

    private string? AuthHeader => Request.Headers.Authorization.ToString();

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] string? busqueda,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var list = await _reportes.ListarPropuestasAsync(AuthHeader, estado, busqueda, page, pageSize, cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(long id, CancellationToken cancellationToken)
    {
        var det = await _reportes.ObtenerPropuestaAsync(AuthHeader, id, cancellationToken).ConfigureAwait(false);
        return det is null ? NotFound() : Ok(det);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] string? estado,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
    {
        var bytes = await _reportes.ExportarPdfAsync(AuthHeader, estado, busqueda, cancellationToken).ConfigureAwait(false);
        var filename = $"propuestas_ticfis_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    [HttpGet("export/pdf-formularios")]
    public async Task<IActionResult> ExportFormulariosPdf(
        [FromQuery] string? estado,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
    {
        var bytes = await _reportes.ExportarFormulariosPdfAsync(AuthHeader, estado, busqueda, cancellationToken).ConfigureAwait(false);
        var sufijo = !string.IsNullOrWhiteSpace(estado) ? $"_{estado.ToLower()}" : "_todos";
        var filename = $"formularios_f233a{sufijo}_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    [HttpGet("{id:long}/export/pdf")]
    public async Task<IActionResult> ExportDetallePdf(long id, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _reportes.ExportarDetallePdfAsync(AuthHeader, id, cancellationToken).ConfigureAwait(false);
            var filename = $"propuesta_{id}_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf";
            return File(bytes, "application/pdf", filename);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

}
