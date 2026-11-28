// Tipos puros de dominio para el catálogo de prestaciones.

export type CategoriaPrestacion =
  | 'Consulta'
  | 'Prevencion'
  | 'Operatoria'
  | 'Endodoncia'
  | 'Periodoncia'
  | 'Cirugia'
  | 'Protesis'
  | 'Ortodoncia'
  | 'Odontopediatria'
  | 'Radiologia'
  | 'Otra'

// Etiquetas legibles para mostrar en la interfaz (el backend envía el nombre del enum).
export const ETIQUETAS_CATEGORIA: Record<CategoriaPrestacion, string> = {
  Consulta: 'Consulta',
  Prevencion: 'Prevención',
  Operatoria: 'Operatoria',
  Endodoncia: 'Endodoncia',
  Periodoncia: 'Periodoncia',
  Cirugia: 'Cirugía',
  Protesis: 'Prótesis',
  Ortodoncia: 'Ortodoncia',
  Odontopediatria: 'Odontopediatría',
  Radiologia: 'Radiología',
  Otra: 'Otra',
}

export const CATEGORIAS_PRESTACION = Object.keys(ETIQUETAS_CATEGORIA) as CategoriaPrestacion[]

export interface PrestacionDto {
  id: number
  codigo: string
  titulo: string
  categoria: CategoriaPrestacion
  duracionEstimadaMinutos: number
  precioParticular: number
  requiereAutorizacionPrevia: boolean
  activo: boolean
}

export interface CrearPrestacionDto {
  codigo: string
  titulo: string
  categoria: CategoriaPrestacion
  duracionEstimadaMinutos: number
  precioParticular: number
  requiereAutorizacionPrevia: boolean
}

export type ActualizarPrestacionDto = Omit<CrearPrestacionDto, 'codigo'>
