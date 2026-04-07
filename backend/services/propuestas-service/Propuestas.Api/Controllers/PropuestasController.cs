using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propuestas.Application.Abstractions;
using Propuestas.Application.Dtos;

namespace Propuestas.Api.Controllers;

[ApiController]
[Route("api/propuestas")]
[Authorize]
public class PropuestasController : ControllerBase
{
    private readonly IPropuestaService _propuestas;

    public PropuestasController(IPropuestaService propuestas)
    {
        _propuestas = propuestas;
    }

    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private IReadOnlyList<string> Roles => User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PropuestaListItemDto>>> Listar([FromQuery] string? estado, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _propuestas.ListarAsync(UserId, Roles, estado, page, pageSize, cancellationToken).ConfigureAwait(false);
            return Ok(list);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PropuestaDetailDto>> Obtener(long id, CancellationToken cancellationToken)
    {
        try
        {
            return await _propuestas.ObtenerAsync(UserId, Roles, id, cancellationToken).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PropuestaDetailDto>> Crear([FromBody] CreatePropuestaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var det = await _propuestas.CrearAsync(UserId, Roles, request, cancellationToken).ConfigureAwait(false);
            return Created($"/api/propuestas/{det.Id}", det);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<PropuestaDetailDto>> Actualizar(long id, [FromBody] UpdatePropuestaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _propuestas.ActualizarAsync(UserId, Roles, id, request, cancellationToken).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/enviar-revision")]
    public async Task<IActionResult> EnviarRevision(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _propuestas.EnviarRevisionAsync(UserId, Roles, id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/aprobar")]
    public async Task<IActionResult> Aprobar(long id, [FromBody] TransicionRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            await _propuestas.AprobarAsync(UserId, Roles, id, request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/rechazar")]
    public async Task<IActionResult> Rechazar(long id, [FromBody] TransicionRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            await _propuestas.RechazarAsync(UserId, Roles, id, request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/pendiente")]
    public async Task<IActionResult> Pendiente(long id, [FromBody] TransicionRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            await _propuestas.MarcarPendienteAsync(UserId, Roles, id, request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/asignar-estudiantes")]
    public async Task<IActionResult> AsignarEstudiantes(long id, [FromBody] AsignarEstudiantesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _propuestas.AsignarEstudiantesAsync(UserId, Roles, id, request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
