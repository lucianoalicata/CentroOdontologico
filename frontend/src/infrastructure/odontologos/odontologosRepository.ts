import { apiClient } from '../http/apiClient'
import type { OdontologoActivoDto } from '../../domain/odontologos/types'

/**
 * Repositorio de Odontólogos.
 * Encapsula todas las llamadas HTTP del módulo de odontólogos.
 */
export const odontologosRepository = {
  async listarActivos(): Promise<OdontologoActivoDto[]> {
    const { data } = await apiClient.get<OdontologoActivoDto[]>('/api/odontologos')
    return data
  },
}
