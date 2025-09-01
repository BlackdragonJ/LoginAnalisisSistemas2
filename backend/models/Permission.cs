using System.ComponentModel.DataAnnotations.Schema;

namespace backend.models;

[Table("permisos")]
public class Permission
{
    [Column("id_permiso")]
    public int Id { get; set; }

    [Column("nombre_rol")]
    public string NombreRol { get; set; } = "";

    public ICollection<Users> Users { get; set; } = new List<Users>();
}
