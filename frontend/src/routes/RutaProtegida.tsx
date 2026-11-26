import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import type { Rol } from '../domain/auth/types'
import { Spinner } from '../ui/atoms/Spinner/Spinner'
import { RUTAS } from './rutas'

interface Props {
  /** Si se omite, alcanza con tener una sesión iniciada */
  roles?: Rol[]
}

/**
 * Template de ruta protegida.
 * Redirige al login si no hay sesión, o a /prohibido si falta el rol.
 */
export function RutaProtegida({ roles }: Props) {
  const { usuario, cargando, tieneRol } = useAuth()

  if (cargando) return <Spinner />

  if (!usuario) {
    return <Navigate to={RUTAS.login} replace />
  }

  if (roles && !tieneRol(...roles)) {
    return <Navigate to={RUTAS.prohibido} replace />
  }

  return <Outlet />
}
