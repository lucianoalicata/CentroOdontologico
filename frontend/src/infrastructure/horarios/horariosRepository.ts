import type {
  FranjaHorariaDto,
  GuardarFranjaHorariaDto,
} from '../../domain/horarios/types'
import { apiClient } from '../http/apiClient'

export const horariosRepository = {
  async listarPorOdontologo(odontologoId: number): Promise<FranjaHorariaDto[]> {
    const { data } = await apiClient.get<FranjaHorariaDto[]>(
      `/api/odontologos/${odontologoId}/horarios`,
    )
    return data
  },

  async agregar(
    odontologoId: number,
    dto: GuardarFranjaHorariaDto,
  ): Promise<FranjaHorariaDto> {
    const { data } = await apiClient.post<FranjaHorariaDto>(
      `/api/odontologos/${odontologoId}/horarios`,
      dto,
    )
    return data
  },

  async actualizar(
    odontologoId: number,
    franjaId: number,
    dto: GuardarFranjaHorariaDto,
  ): Promise<FranjaHorariaDto> {
    const { data } = await apiClient.put<FranjaHorariaDto>(
      `/api/odontologos/${odontologoId}/horarios/${franjaId}`,
      dto,
    )
    return data
  },

  async eliminar(odontologoId: number, franjaId: number): Promise<void> {
    await apiClient.delete(`/api/odontologos/${odontologoId}/horarios/${franjaId}`)
  },
}