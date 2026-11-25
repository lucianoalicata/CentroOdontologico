namespace Clinica.Aplicacion.Identidad.Dtos;

public record OdontologoActivoDto(
    int Id,
    string Nombre,
    string Apellido,
    string Matricula,
    string Especialidad);