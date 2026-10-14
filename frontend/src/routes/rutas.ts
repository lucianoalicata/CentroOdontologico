import type { Rol } from '../auth/tipos'

export const RUTAS = {
  login: '/login',
  administrador: '/admin',
  secretaria: '/secretaria',
  odontologo: '/odontologo',
  paciente: '/paciente',
  prohibido: '/403',
} as const

// Si un usuario tiene varios roles, se usa el primero de esta lista
const PRIORIDAD: { rol: Rol; ruta: string }[] = [
  { rol: 'Odontologo', ruta: RUTAS.odontologo },
  { rol: 'Secretaria', ruta: RUTAS.secretaria },
  { rol: 'Administrador', ruta: RUTAS.administrador },
  { rol: 'Paciente', ruta: RUTAS.paciente },
]

// Pantalla inicial según los roles del usuario
export function rutaInicial(roles: Rol[]): string {
  return PRIORIDAD.find((p) => roles.includes(p.rol))?.ruta ?? RUTAS.login
}
