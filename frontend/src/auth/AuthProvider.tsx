import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { apiClient, registrarAlNoAutorizado } from '../api/apiClient'
import { AuthContext } from './AuthContext'
import type { AuthContextValor } from './AuthContext'
import { borrarSesion, guardarSesion, leerSesion } from './tokenStorage'
import type { Credenciales, LoginRespuesta, Rol, UsuarioSesion } from './tipos'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioSesion | null>(null)
  // Solo hay que "cargar" si existe un token guardado para restaurar
  const [cargando, setCargando] = useState<boolean>(() => leerSesion() !== null)

  // Al abrir la aplicación: si hay un token guardado, se valida pidiendo la sesión a la API
  useEffect(() => {
    if (!leerSesion()) return
    let cancelado = false

    apiClient
      .get<UsuarioSesion>('/api/auth/me')
      .then((respuesta) => {
        if (!cancelado) setUsuario(respuesta.data)
      })
      .catch(() => {
        if (!cancelado) {
          borrarSesion()
          setUsuario(null)
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  // Si la API responde 401 en cualquier request, se cierra la sesión
  useEffect(() => {
    registrarAlNoAutorizado(() => setUsuario(null))
    return () => registrarAlNoAutorizado(null)
  }, [])

  const login = useCallback(async (credenciales: Credenciales) => {
    const { data } = await apiClient.post<LoginRespuesta>('/api/auth/login', credenciales)
    guardarSesion(data.token, data.expiraEn)
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
