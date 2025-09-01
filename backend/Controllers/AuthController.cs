using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Database db, IConfiguration cfg) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login dto)
    {
        if (string.IsNullOrWhiteSpace(dto.usuario) || string.IsNullOrWhiteSpace(dto.password))
            return BadRequest(new { error = "Datos invalidos" });

        var user = await db.Users.Include(u => u.Permission)
            .FirstOrDefaultAsync(u => u.Usuario == dto.usuario);

        if (user is null) 
            return Unauthorized(new { error = "Usuario invalido" });

        var sent = dto.password.Trim();
        var stored = (user.PasswordHash ?? "").Trim();

        Console.WriteLine($"[LOGIN] dto.pass='{sent}' db.pass='{stored}'");

        if(sent != stored)
            return Unauthorized(new { error = "Contraseña invalida" });

        var role = user.Permission?.NombreRol ?? (user.PermissionId == 1 ? "IT" : "Desarrollador");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var exp = DateTime.UtcNow.AddHours(int.Parse(cfg["Jwt:ExpireHours"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Usuario),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"],
            audience: cfg["Jwt:Audience"],
            claims: claims,
            expires: exp,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = jwt, rol = role });
    }
}
