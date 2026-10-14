import type { Rol } from '../auth/tipos'
import { RUTAS } from '../routes/rutas'

export interface ItemMenu {
  etiqueta: string
  ruta: string
}

// Cada módulo agrega acá sus pantallas, en la sección del rol que corresponda.
const MENU_POR_ROL: Record<Rol, ItemMenu[]> = {
  Odontologo: [{ etiqueta: 'Mi agenda', ruta: RUTAS.odontologo }],
  Secretaria: [{ etiqueta: 'Recepción', ruta: RUTAS.secretaria }],
  Administrador: [{ etiqueta: 'Administración', ruta: RUTAS.administrador }],
  Paciente: [{ etiqueta: 'Mi portal', ruta: RUTAS.paciente }],
}

// El menú de un usuario une los de todos sus roles, sin repetir pantallas.
export function menuParaRoles(roles: Rol[]): ItemMenu[] {
  const items = roles.flatMap((rol) => MENU_POR_ROL[rol])
  return items.filter((item, i) => items.findIndex((otro) => otro.ruta === item.ruta) === i)
}
