import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { registrarAlNoAutorizado } from '../infrastructure/http/apiClient'
import { authRepository } from '../infrastructure/auth/authRepository'
import { borrarSesion, leerSesion } from '../infrastructure/auth/tokenStorage'
import { AuthContext } from './AuthContext'
import type { AuthContextValor } from './AuthContext'
import type { Credenciales, Rol, UsuarioSesion } from '../domain/auth/types'

/**
 * Proveedor del contexto de autenticación.
 * DIP: depende de authRepository y tokenStorage (abstracciones), no de apiClient directamente.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioSesion | null>(null)
  const [cargando, setCargando] = useState<boolean>(() => leerSesion() !== null)

  // Al abrir la app: si hay token guardado, se valida contra la API
  useEffect(() => {
    if (!leerSesion()) return
    let cancelado = false

    authRepository
      .obtenerSesionActual()
      .then((u) => { if (!cancelado) setUsuario(u) })
      .catch(() => {
        if (!cancelado) {
          borrarSesion()
          setUsuario(null)
        }
      })
      .finally(() => { if (!cancelado) setCargando(false) })

    return () => { cancelado = true }
  }, [])

  // Cierra sesión automáticamente cuando la API responde 401
  useEffect(() => {
    registrarAlNoAutorizado(() => setUsuario(null))
    return () => registrarAlNoAutorizado(null)
  }, [])

  const login = useCallback(async (credenciales: Credenciales) => {
    const data = await authRepository.login(credenciales)
    setUsuario(data.usuario)
    return data.usuario
  }, [])

  const logout = useCallback(() => {
    borrarSesion()
    setUsuario(null)
  }, [])

  // Cierra la sesión sola cuando el token vence
  useEffect(() => {
    if (!usuario) return
    const sesion = leerSesion()
    if (!sesion) return

    const restante = new Date(sesion.expiraEn).getTime() - Date.now()
    const temporizador = setTimeout(logout, restante)
    return () => clearTimeout(temporizador)
  }, [usuario, logout])

  const tieneRol = useCallback(
    (...roles: Rol[]) => usuario !== null && roles.some((rol) => usuario.roles.includes(rol)),
    [usuario],
  )

  const valor = useMemo<AuthContextValor>(
    () => ({ usuario, cargando, login, logout, tieneRol }),
    [usuario, cargando, login, logout, tieneRol],
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}
