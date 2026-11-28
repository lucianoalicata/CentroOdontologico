import { apiClient } from '../http/apiClient'
import type { ArchivoAdjuntoDto } from '../../domain/historiaClinica/adjuntos'

const deEntrada = (entradaId: number) => `/api/historia-clinica/entradas/${entradaId}/adjuntos`

/** Repositorio de archivos adjuntos de las entradas de la historia clínica. */
export const adjuntosRepository = {
  async listar(entradaId: number): Promise<ArchivoAdjuntoDto[]> {
    const { data } = await apiClient.get<ArchivoAdjuntoDto[]>(deEntrada(entradaId))
    return data
  },

  async subir(entradaId: number, archivo: File): Promise<ArchivoAdjuntoDto> {
    const formulario = new FormData()
    formulario.append('archivo', archivo)

    // El cliente usa JSON por defecto; con multipart el navegador agrega el boundary.
    const { data } = await apiClient.post<ArchivoAdjuntoDto>(deEntrada(entradaId), formulario, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data
  },

  /** Descarga el contenido con el token de la sesión (no sirve un enlace directo). */
  async descargar(adjuntoId: number): Promise<Blob> {
    const { data } = await apiClient.get<Blob>(`/api/historia-clinica/adjuntos/${adjuntoId}/contenido`, {
      responseType: 'blob',
    })
    return data
  },
}
