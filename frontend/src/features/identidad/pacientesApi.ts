import { apiClient } from '../../api/apiClient'

export interface PacienteDto {
  id: number
  nombre: string
  apellido: string
  dni: string
  telefono: string
  fechaNacimiento: string // formato YYYY-MM-DD (DateOnly)
  domicilio: string | null
  tieneCuentaWeb: boolean
  activo: boolean
}

export interface RegistrarPacienteDto {
  nombre: string
  apellido: string
  dni: string
  telefono: string
  fechaNacimiento: string
  domicilio: string | null
}

export interface ActualizarPacienteDto {
  nombre: string
  apellido: string
  telefono: string
  fechaNacimiento: string
  domicilio: string | null
}

export interface ResultadoPaginado<T> {
  items: T[]
  totalItems: number
  pageIndex: number
  pageSize: number
  totalPages: number
}

export async function buscarPacientes(
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
}

export async function registrarPaciente(dto: RegistrarPacienteDto): Promise<PacienteDto> {
  const { data } = await apiClient.post<PacienteDto>('/api/pacientes', dto)
  return data
}

export async function actualizarPaciente(
  id: number,
  dto: ActualizarPacienteDto,
): Promise<PacienteDto> {
  const { data } = await apiClient.put<PacienteDto>(`/api/pacientes/${id}`, dto)
  return data
}

export async function darDeBajaPaciente(id: number): Promise<void> {
  await apiClient.delete(`/api/pacientes/${id}`)
}

export async function reactivarPaciente(id: number): Promise<PacienteDto> {
  const { data } = await apiClient.patch<PacienteDto>(`/api/pacientes/${id}/reactivar`)
  return data
}