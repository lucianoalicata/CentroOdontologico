import { apiClient } from '../http/apiClient'
import type {
  ActualizarPrestacionDto,
  CategoriaPrestacion,
  CrearPrestacionDto,
  PrestacionDto,
} from '../../domain/prestaciones/types'

interface FiltrosPrestacion {
  filtro?: string
  categoria?: CategoriaPrestacion | null
}

function aParametros({ filtro, categoria }: FiltrosPrestacion) {
  return {
    filtro: filtro?.trim() || undefined,
    categoria: categoria ?? undefined,
  }
}

/**
 * Repositorio del catálogo de prestaciones.
 * Encapsula las llamadas HTTP; los componentes nunca usan apiClient directamente.
 */
export const prestacionesRepository = {
  /** Prestaciones activas (todo el personal). */
  async listarActivas(filtros: FiltrosPrestacion = {}): Promise<PrestacionDto[]> {
    const { data } = await apiClient.get<PrestacionDto[]>('/api/prestaciones', {
      params: aParametros(filtros),
    })
    return data
  },

  /** Catálogo completo, incluidas las dadas de baja (solo administrador). */
  async listarTodas(filtros: FiltrosPrestacion = {}): Promise<PrestacionDto[]> {
    const { data } = await apiClient.get<PrestacionDto[]>('/api/prestaciones/todas', {
      params: aParametros(filtros),
    })
    return data
  },

  async crear(dto: CrearPrestacionDto): Promise<PrestacionDto> {
    const { data } = await apiClient.post<PrestacionDto>('/api/prestaciones', dto)
    return data
  },

  async actualizar(id: number, dto: ActualizarPrestacionDto): Promise<PrestacionDto> {
    const { data } = await apiClient.put<PrestacionDto>(`/api/prestaciones/${id}`, dto)
    return data
  },

  async darDeBaja(id: number): Promise<void> {
    await apiClient.delete(`/api/prestaciones/${id}`)
  },

  async reactivar(id: number): Promise<PrestacionDto> {
    const { data } = await apiClient.patch<PrestacionDto>(`/api/prestaciones/${id}/reactivar`)
    return data
  },
}
