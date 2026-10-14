import { createContext } from 'react'
import type { Credenciales, Rol, UsuarioSesion } from './tipos'

export interface AuthContextValor {
  usuario: UsuarioSesion | null
  // true mientras se restaura la sesión al abrir la aplicación
  cargando: boolean
  login: (credenciales: Credenciales) => Promise<UsuarioSesion>
  logout: () => void
  // true si el usuario tiene al menos uno de los roles indicados
  tieneRol: (...roles: Rol[]) => boolean
}

export const AuthContext = createContext<AuthContextValor | null>(null)