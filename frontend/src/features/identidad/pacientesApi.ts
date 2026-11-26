// Mantiene compatibilidad con imports desde features/identidad/pacientesApi.
// La fuente de verdad está en domain/pacientes/types.ts e infrastructure/pacientes/.

export type {
  PacienteDto,
  RegistrarPacienteDto,
  ActualizarPacienteDto,
} from '../../domain/pacientes/types'
export type { ResultadoPaginado } from '../../domain/shared/types'

import { pacientesRepository } from '../../infrastructure/pacientes/pacientesRepository'

export const buscarPacientes = pacientesRepository.buscar.bind(pacientesRepository)
export const registrarPaciente = pacientesRepository.registrar.bind(pacientesRepository)
export const actualizarPaciente = pacientesRepository.actualizar.bind(pacientesRepository)
export const darDeBajaPaciente = pacientesRepository.darDeBaja.bind(pacientesRepository)
export const reactivarPaciente = pacientesRepository.reactivar.bind(pacientesRepository)
