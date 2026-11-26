// Tipos puros de dominio para el módulo de Pacientes.

export interface PacienteDto {
  id: number
  nombre: string
  apellido: string
  dni: string
  telefono: string
  fechaNacimiento: string // YYYY-MM-DD (DateOnly)
  domicilio: string | null
  tieneCuentaWeb: boolean
  activo: boolean
}

export interface RegistrarPacienteDto {
  nombre: string
  apellido: string
  dni: string
  telefono: string
  fechaNacimiento: string
  domicilio: string | null
}

export interface ActualizarPacienteDto {
  nombre: string
  apellido: string
  telefono: string
  fechaNacimiento: string
  domicilio: string | null
}
