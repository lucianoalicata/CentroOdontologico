import axios from 'axios'

// Formato de error que devuelve la API (ProblemDetails de ASP.NET Core)
export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

// Convierte cualquier error en un mensaje legible para mostrar al usuario.
export function mensajeDeError(error: unknown): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    if (!error.response) {
      return 'No se pudo conectar con el servidor. Verifique su conexión.'
    }

    const datos = error.response.data

    if (datos?.errors) {
      const mensajes = Object.values(datos.errors).flat()
      if (mensajes.length > 0) return mensajes.join(' ')
    }
    if (datos?.detail) return datos.detail

    if (error.response.status === 403) return 'No tiene permisos para realizar esta acción.'
    if (error.response.status === 404) return 'No se encontró el recurso solicitado.'
  }

  return 'Ocurrió un error inesperado. Intente nuevamente.'
}