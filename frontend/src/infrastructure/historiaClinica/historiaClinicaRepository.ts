import { apiClient } from '../http/apiClient'
import type {
  ActualizarAlertasDto,
  EntradaHistoriaClinicaDto,
  HistoriaClinicaDto,
  RegistrarEntradaDto,
} from '../../domain/historiaClinica/types'

const base = (pacienteId: number) => `/api/pacientes/${pacienteId}/historia-clinica`

/**
 * Repositorio de la historia clínica de un paciente.
 * La API crea la historia en el primer acceso (obtener o crear).
 */
export const historiaClinicaRepository = {
  async obtener(pacienteId: number): Promise<HistoriaClinicaDto> {
    const { data } = await apiClient.get<HistoriaClinicaDto>(base(pacienteId))
    return data
  },

  async actualizarAlertas(pacienteId: number, dto: ActualizarAlertasDto): Promise<HistoriaClinicaDto> {
    const { data } = await apiClient.put<HistoriaClinicaDto>(`${base(pacienteId)}/alertas`, dto)
    return data
  },

  async registrarEntrada(
    pacienteId: number,
    dto: RegistrarEntradaDto,
  ): Promise<EntradaHistoriaClinicaDto> {
    const { data } = await apiClient.post<EntradaHistoriaClinicaDto>(
      `${base(pacienteId)}/entradas`,
      dto,
    )
    return data
  },
}
