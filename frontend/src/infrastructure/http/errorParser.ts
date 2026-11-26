import axios from 'axios'

// Formato de error que devuelve la API (ProblemDetails de ASP.NET Core).
interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

// Interfaz genérica para errores de red/axios.
interface ErrorConRespuesta {
  response?: {
    data?: {
      mensaje?: string
      message?: string
      title?: string
      detail?: string
      errors?: Record<string, string[]>
    }
    status?: number
  }
  message?: string
}

/**
 * Convierte cualquier error (Axios, JS, string) en un mensaje
 * legible para mostrar al usuario. Único punto de conversión de errores.
 */
export function parsearError(error: unknown): string {
  // Error de axios con ProblemDetails
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
    if (datos?.title) return datos.title

    if (error.response.status === 403) return 'No tiene permisos para realizar esta acción.'
    if (error.response.status === 404) return 'No se encontró el recurso solicitado.'
  }

  // Error de red genérico con propiedad response
  if (error && typeof error === 'object') {
    const err = error as ErrorConRespuesta
    const data = err.response?.data

    if (data?.mensaje) return data.mensaje
    if (data?.message) return data.message
    if (data?.detail) return data.detail
    if (data?.title) return data.title
    if (err.message) return err.message
  }

  if (typeof error === 'string' && error.trim().length > 0) return error

  return 'Ocurrió un error inesperado. Intente nuevamente.'
}
