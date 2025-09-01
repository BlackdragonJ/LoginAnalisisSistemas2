using backend.Data;
using backend.DTO;
using backend.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(Database db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List()
    {
        var data = await db.Users.Include(u => u.Permission)
            .Select(u => new {
                id_usuario = u.Id,
                usuario = u.Usuario,
                nombre_usuario = u.NombreUsuario,
                id_permiso_fk = u.PermissionId,
                nombre_rol = u.Permission != null ? u.Permission.NombreRol : null
            }).ToListAsync();

        return Ok(data);
    }

    [HttpPost]
    [Authorize(Policy = "ITOnly")]
    public async Task<IActionResult> Create([FromBody] CreateUser dto)
    {
        if (string.IsNullOrWhiteSpace(dto.usuario) || string.IsNullOrWhiteSpace(dto.password))
            return BadRequest(new { error = "Usuario y contraseña requeridos" });

        var exists = await db.Users.AnyAsync(u => u.Usuario == dto.usuario);
        if (exists) return BadRequest(new { error = "Usuario ya existe" });

        var u = new Users
        {
            Usuario = dto.usuario.Trim(),
            NombreUsuario = dto.nombre_usuario.Trim(),
            PasswordHash = dto.password,
            PermissionId = dto.id_permiso_fk
        };

        db.Users.Add(u);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = u.Id }, new { id_usuario = u.Id });
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await db.Users.Include(x => x.Permission).FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        return Ok(new {
            id_usuario = u.Id,
            usuario = u.Usuario,
            nombre_usuario = u.NombreUsuario,
            id_permiso_fk = u.PermissionId,
            nombre_rol = u.Permission?.NombreRol
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ITOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUser dto)
    {
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();

        if (dto.usuario is not null) u.Usuario = dto.usuario.Trim();
        if (dto.nombre_usuario is not null) u.NombreUsuario = dto.nombre_usuario.Trim();
        if (dto.password is not null)
        {
            if (dto.password.Length < 6) return BadRequest(new { error = "Password >= 6" });
            u.PasswordHash = dto.password;
        }
        if (dto.id_permiso_fk.HasValue) u.PermissionId = dto.id_permiso_fk.Value;

        await db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ITOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        db.Users.Remove(u);
        await db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
