import { apiClient } from '../http/apiClient'
import { guardarSesion } from './tokenStorage'
import type { Credenciales, LoginRespuesta, UsuarioSesion } from '../../domain/auth/types'

/**
 * Repositorio de autenticación.
 * Encapsula todas las llamadas HTTP del módulo de auth.
 */
export const authRepository = {
  /** Autentica al usuario y persiste el token. */
  async login(credenciales: Credenciales): Promise<LoginRespuesta> {
    const { data } = await apiClient.post<LoginRespuesta>('/api/auth/login', credenciales)
    guardarSesion(data.token, data.expiraEn)
    return data
  },

  /** Verifica y restaura la sesión actual usando el token guardado. */
  async obtenerSesionActual(): Promise<UsuarioSesion> {
    const { data } = await apiClient.get<UsuarioSesion>('/api/auth/me')
    return data
  },
}
