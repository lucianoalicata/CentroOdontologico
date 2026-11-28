import { apiClient } from '../http/apiClient'
import type { ResultadoPaginado } from '../../domain/shared/types'
import type {
  ActualizarPacienteDto,
  PacienteDto,
  RegistrarPacienteDto,
} from '../../domain/pacientes/types'

/**
 * Repositorio de Pacientes.
 * Encapsula todas las llamadas HTTP del módulo de pacientes.
 * Los componentes nunca llaman a apiClient directamente.
 */
export const pacientesRepository = {
  async buscar(
    filtro: string,
    pageIndex: number,
    pageSize = 10,
  ): Promise<ResultadoPaginado<PacienteDto>> {
    const { data } = await apiClient.get<ResultadoPaginado<PacienteDto>>('/api/pacientes', {
      params: {
        filtro: filtro.trim() || undefined,
        pageIndex,
        pageSize,
      },
    })
    return data
  },

  async obtenerPorId(id: number): Promise<PacienteDto> {
    const { data } = await apiClient.get<PacienteDto>(`/api/pacientes/${id}`)
    return data
  },

  async registrar(dto: RegistrarPacienteDto): Promise<PacienteDto> {
    const { data } = await apiClient.post<PacienteDto>('/api/pacientes', dto)
    return data
  },

  async actualizar(id: number, dto: ActualizarPacienteDto): Promise<PacienteDto> {
    const { data } = await apiClient.put<PacienteDto>(`/api/pacientes/${id}`, dto)
    return data
  },

  async darDeBaja(id: number): Promise<void> {
    await apiClient.delete(`/api/pacientes/${id}`)
  },

  async reactivar(id: number): Promise<PacienteDto> {
    const { data } = await apiClient.patch<PacienteDto>(`/api/pacientes/${id}/reactivar`)
    return data
  },
}
