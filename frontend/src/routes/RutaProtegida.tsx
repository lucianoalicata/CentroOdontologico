import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import type { Rol } from '../auth/tipos'
import { PantallaCarga } from '../shared/components/PantallaCarga'
import { RUTAS } from './rutas'

interface Props {
  // Si se omite, alcanza con tener una sesión iniciada
  roles?: Rol[]
}

export function RutaProtegida({ roles }: Props) {
  const { usuario, cargando, tieneRol } = useAuth()

  if (cargando) return <PantallaCarga />

  // Sin sesión: al login
  if (!usuario) {
    return <Navigate to={RUTAS.login} replace />
  }

  // Con sesión pero sin el rol necesario
  if (roles && !tieneRol(...roles)) {
    return <Navigate to={RUTAS.prohibido} replace />
  }

  return <Outlet />
}
