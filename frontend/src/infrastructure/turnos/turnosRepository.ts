import { apiClient } from '../http/apiClient'
import type {
  CrearEntreTurnoDto,
  CrearTurnoProgramadoDto,
  FranjaLibreDto,
} from '../../domain/turnos/types'

/**
 * Repositorio de Turnos.
 * Encapsula todas las llamadas HTTP del módulo de turnos.
 */
export const turnosRepository = {
  async obtenerFranjasLibres(
    odontologoId: number,
    fecha: string,
  ): Promise<FranjaLibreDto[]> {
    const { data } = await apiClient.get<FranjaLibreDto[]>('/api/turnos/franjas-libres', {
      params: { odontologoId, fecha },
    })
    return data
  },

  async crearProgramado(dto: CrearTurnoProgramadoDto): Promise<void> {
    await apiClient.post('/api/turnos/programados', dto)
  },

  async crearEntreTurno(dto: CrearEntreTurnoDto): Promise<void> {
    await apiClient.post('/api/turnos/entre-turnos', dto)
  },
}
