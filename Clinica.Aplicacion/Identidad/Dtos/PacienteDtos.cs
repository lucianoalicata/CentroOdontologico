namespace Clinica.Aplicacion.Identidad.Dtos;

public record RegistrarPacienteDto(
    string Nombre,
    string Apellido,
    string Dni,
    string Telefono,
    DateOnly FechaNacimiento,
    string? Domicilio);

public record PacienteDto(
    int Id,
    string Nombre,
    string Apellido,
    string Dni,
    string Telefono,
    DateOnly FechaNacimiento,
    string? Domicilio,
    bool TieneCuentaWeb,
    bool Activo);