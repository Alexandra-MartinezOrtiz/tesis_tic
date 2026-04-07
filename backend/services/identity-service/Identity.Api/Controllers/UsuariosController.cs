using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _usuarios.ListAsync(cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDetalleDto>> Create([FromBody] CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var u = await _usuarios.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            return Created($"/api/usuarios/{u.Id}", u);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UsuarioDetalleDto>> Update(long id, [FromBody] UpdateUsuarioRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var u = await _usuarios.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
            return Ok(u);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/roles")]
    public async Task<IActionResult> AsignarRoles(long id, [FromBody] AsignarRolesRequest request, CancellationToken cancellationToken)
    {
        await _usuarios.AsignarRolesAsync(id, request, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
