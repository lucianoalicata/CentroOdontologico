// Mantiene compatibilidad con imports desde features/turnos/turnosApi.
// La fuente de verdad está en domain/turnos/types.ts e infrastructure/turnos/.

export type {
  FranjaLibreDto,
  CrearTurnoProgramadoDto,
  CrearEntreTurnoDto,
} from '../../domain/turnos/types'

import { turnosRepository } from '../../infrastructure/turnos/turnosRepository'

export const obtenerFranjasLibres =
  turnosRepository.obtenerFranjasLibres.bind(turnosRepository)
export const crearTurnoProgramado =
  turnosRepository.crearProgramado.bind(turnosRepository)
export const crearEntreTurno =
  turnosRepository.crearEntreTurno.bind(turnosRepository)
