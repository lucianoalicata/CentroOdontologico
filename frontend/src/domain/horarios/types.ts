export type DiaSemana = 1 | 2 | 3 | 4 | 5 | 6 | 7 // 1 = Lunes ... 7 = Domingo

export interface FranjaHorariaDto {
  id: number
  odontologoId: number
  diaSemana: DiaSemana
  horaInicio: string // "HH:mm"
  horaFin: string    // "HH:mm"
  duracionTurnoMinutos: number
}

export interface GuardarFranjaHorariaDto {
  diaSemana: DiaSemana
  horaInicio: string // "HH:mm"
  horaFin: string    // "HH:mm"
  duracionTurnoMinutos: number
}

export const DIAS_SEMANA: { valor: DiaSemana; etiqueta: string }[] = [
  { valor: 1, etiqueta: 'Lunes' },
  { valor: 2, etiqueta: 'Martes' },
  { valor: 3, etiqueta: 'Miércoles' },
  { valor: 4, etiqueta: 'Jueves' },
  { valor: 5, etiqueta: 'Viernes' },
  { valor: 6, etiqueta: 'Sábado' },
]