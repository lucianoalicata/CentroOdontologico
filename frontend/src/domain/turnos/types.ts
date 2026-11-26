// Tipos puros de dominio para el módulo de Turnos.

export interface FranjaLibreDto {
  horaInicio: string // "HH:mm"
  horaFin: string    // "HH:mm"
}

export interface CrearTurnoProgramadoDto {
  odontologoId: number
  pacienteId: number
  fecha: string      // "YYYY-MM-DD"
  horaInicio: string // "HH:mm"
  motivo?: string | null
}

export interface CrearEntreTurnoDto {
  odontologoId: number
  fecha: string      // "YYYY-MM-DD"
  horaInicio: string // "HH:mm"
  nombre: string
  telefono: string
  dni?: string | null
  motivo?: string | null
}
