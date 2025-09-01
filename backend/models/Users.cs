using System.ComponentModel.DataAnnotations.Schema;

namespace backend.models;

[Table("usuarios")]
public class Users
{
    [Column("id_usuario")]
    public int Id { get; set; }

    [Column("usuario")]
    public string Usuario { get; set; } = "";

    [Column("nombre_usuario")]
    public string NombreUsuario { get; set; } = "";

    [Column("password_hash")]
    public string PasswordHash { get; set; } = "";

    [Column("id_permiso_fk")]
    public int PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
