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
    public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var list = await _reportes.ListarPropuestasAsync(AuthHeader, estado, page, pageSize, cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(long id, CancellationToken cancellationToken)
    {
        var det = await _reportes.ObtenerPropuestaAsync(AuthHeader, id, cancellationToken).ConfigureAwait(false);
        return det is null ? NotFound() : Ok(det);
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string? estado, CancellationToken cancellationToken)
    {
        var bytes = await _reportes.ExportarPdfAsync(AuthHeader, estado, cancellationToken).ConfigureAwait(false);
        return File(bytes, "application/pdf", "propuestas.pdf");
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string? estado, CancellationToken cancellationToken)
    {
        var bytes = await _reportes.ExportarExcelAsync(AuthHeader, estado, cancellationToken).ConfigureAwait(false);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "propuestas.xlsx");
    }
}
