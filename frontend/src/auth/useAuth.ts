import { useContext } from 'react'
import { AuthContext } from './AuthContext'

// Hook para acceder a la sesión desde cualquier componente
export function useAuth() {
  const contexto = useContext(AuthContext)
  if (!contexto) {
    throw new Error('useAuth debe usarse dentro de <AuthProvider>')
  }
  return contexto
}
