// Tipos puros de dominio para autenticación.
// No importan nada de frameworks externos.

export type Rol = 'Administrador' | 'Secretaria' | 'Odontologo' | 'Paciente'

export interface Credenciales {
  email: string
  password: string
}

export interface UsuarioSesion {
  id: number
  nombre: string
  apellido: string
  email: string
  roles: Rol[]
  pacienteId: number | null
  odontologoId: number | null
  secretariaId: number | null
}

export interface LoginRespuesta {
  token: string
  expiraEn: string // ISO UTC, e.g. "2026-10-12T21:30:00Z"
  usuario: UsuarioSesion
}
