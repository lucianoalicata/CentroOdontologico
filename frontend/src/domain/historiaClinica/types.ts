// Tipos puros de dominio para la historia clínica del paciente.
// Reflejan los DTOs de la API (S1-30 / S1-33). Las fechas llegan en UTC.

export interface PrestacionRealizadaDto {
  id: number
  codigo: string
  titulo: string
  precioUnitario: number
  cantidad: number
  piezaDental: number | null
  mesesGarantia: number | null
  subtotal: number
}

export interface EntradaHistoriaClinicaDto {
  id: number
  odontologoId: number
  odontologoNombre: string
  fechaCreacion: string // ISO UTC
  comentario: string | null
  corrigeEntradaId: number | null
  prestaciones: PrestacionRealizadaDto[]
}

export interface HistoriaClinicaDto {
  id: number
  pacienteId: number
  alertasMedicas: string | null
  /** Ordenadas de la más reciente a la más antigua. */
  entradas: EntradaHistoriaClinicaDto[]
}

export interface PrestacionARegistrarDto {
  prestacionId: number
  cantidad: number
  piezaDental: number | null
  mesesGarantia: number | null
}

export interface RegistrarEntradaDto {
  comentario: string | null
  prestaciones: PrestacionARegistrarDto[]
  corrigeEntradaId: number | null
}

export interface ActualizarAlertasDto {
  alertasMedicas: string | null
}

// Deben coincidir con las reglas del dominio
export const LONGITUD_MAXIMA_COMENTARIO = 2000
export const LONGITUD_MAXIMA_ALERTAS = 1000

/** Pieza dental en notación FDI: 11-48 permanentes y 51-85 temporales. */
export function esPiezaFdiValida(pieza: number): boolean {
  if (!Number.isInteger(pieza)) return false
  const cuadrante = Math.floor(pieza / 10)
  const diente = pieza % 10
  if (cuadrante >= 1 && cuadrante <= 4) return diente >= 1 && diente <= 8
  if (cuadrante >= 5 && cuadrante <= 8) return diente >= 1 && diente <= 5
  return false
}
