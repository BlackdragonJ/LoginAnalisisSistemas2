namespace backend.DTO;

public record CreateUser(string usuario, string nombre_usuario, string password, int id_permiso_fk);
public record UpdateUser(string? usuario, string? nombre_usuario, string? password, int? id_permiso_fk);
