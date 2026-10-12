import axios from 'axios'
import type { AxiosError } from 'axios'
import { borrarSesion, leerSesion } from '../auth/tokenStorage'

const API_URL = import.meta.env.VITE_API_URL as string | undefined

if (!API_URL) {
  throw new Error('Falta configurar VITE_API_URL en el archivo .env.development')
}

// Cliente HTTP único. Ningún componente debe usar fetch o axios directamente.
export const apiClient = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
})

// El contexto de autenticación registra acá qué hacer cuando la API responde 401.
// Así este archivo no depende de React.
let alNoAutorizado: (() => void) | null = null

export function registrarAlNoAutorizado(callback: (() => void) | null): void {
  alNoAutorizado = callback
}

// Antes de cada request: agrega el token si hay una sesión vigente
apiClient.interceptors.request.use((config) => {
  const sesion = leerSesion()
  if (sesion) {
    config.headers.Authorization = `Bearer ${sesion.token}`
  }
  return config
})

// Después de cada respuesta: ante un 401 (salvo en el propio login) se cierra la sesión
apiClient.interceptors.response.use(
  (respuesta) => respuesta,
  (error: AxiosError) => {
    const esLogin = error.config?.url?.includes('/api/auth/login') ?? false

    if (error.response?.status === 401 && !esLogin) {
      borrarSesion()
      alNoAutorizado?.()
    }

    return Promise.reject(error)
  },
)