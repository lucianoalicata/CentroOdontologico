import { useEffect, useState } from 'react'
import type {
  FranjaHorariaDto,
  GuardarFranjaHorariaDto,
} from '../../domain/horarios/types'
import { horariosRepository } from '../../infrastructure/horarios/horariosRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'

function aMinutos(hora: string): number {
  const [hh, mm] = hora.split(':').map(Number)
  return hh * 60 + mm
}

export function validarFranjaSinSolapamiento(
  nueva: GuardarFranjaHorariaDto,
  existentes: FranjaHorariaDto[],
  idIgnorar?: number,
): string | null {
  const inicioNuevo = aMinutos(nueva.horaInicio)
  const finNuevo = aMinutos(nueva.horaFin)

  if (Number.isNaN(inicioNuevo) || Number.isNaN(finNuevo)) {
    return 'Ingresá horarios válidos.'
  }
  if (inicioNuevo >= finNuevo) {
    return 'La hora de inicio debe ser anterior a la hora de fin.'
  }
  if (nueva.duracionTurnoMinutos < 10 || nueva.duracionTurnoMinutos > 120) {
    return 'La duración del turno debe estar entre 10 y 120 minutos.'
  }
  if (finNuevo - inicioNuevo < nueva.duracionTurnoMinutos) {
    return 'La franja horaria es más corta que la duración de un turno.'
  }

  const seSolapa = existentes.some((f) => {
    if (f.id === idIgnorar) return false
    if (f.diaSemana !== nueva.diaSemana) return false
    const inicioExistente = aMinutos(f.horaInicio)
    const finExistente = aMinutos(f.horaFin)
    return inicioNuevo < finExistente && finNuevo > inicioExistente
  })

  if (seSolapa) {
    return 'La franja se solapa con otro horario ya cargado en el mismo día.'
  }

  return null
}

export function useDisponibilidadOdontologo(odontologoId: number | null) {
  const [franjas, setFranjas] = useState<FranjaHorariaDto[]>([])
  const [cargando, setCargando] = useState(Boolean(odontologoId))
  const [guardando, setGuardando] = useState(false)
  const [recarga, setRecarga] = useState(0)

  useEffect(() => {
    if (!odontologoId) return
    let cancelado = false

    async function cargar() {
      try {
        const lista = await horariosRepository.listarPorOdontologo(odontologoId!)
        if (!cancelado) {
          const ordenadas = [...lista].sort((a, b) =>
            a.diaSemana !== b.diaSemana
              ? a.diaSemana - b.diaSemana
              : a.horaInicio.localeCompare(b.horaInicio),
          )
          setFranjas(ordenadas)
        }
      } catch {
        // Si el backend de horarios aún no está mergeado, no rompe la vista
        if (!cancelado) setFranjas([])
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()
    return () => {
      cancelado = true
    }
  }, [odontologoId, recarga])

  function refrescar() {
    if (!odontologoId) return
    setCargando(true)
    setRecarga((n) => n + 1)
  }

  async function guardarFranja(
    dto: GuardarFranjaHorariaDto,
    franjaEditarId?: number,
  ): Promise<boolean> {
    if (!odontologoId) return false

    const errorValidacion = validarFranjaSinSolapamiento(dto, franjas, franjaEditarId)
    if (errorValidacion) {
      avisarError(errorValidacion)
      return false
    }

    setGuardando(true)
    try {
      if (franjaEditarId) {
        await horariosRepository.actualizar(odontologoId, franjaEditarId, dto)
        avisarExito('Horario actualizado correctamente.')
      } else {
        await horariosRepository.agregar(odontologoId, dto)
        avisarExito('Horario agregado correctamente.')
      }
      refrescar()
      return true
    } catch (error) {
      avisarError(error)
      return false
    } finally {
      setGuardando(false)
    }
  }

  async function eliminarFranja(franjaId: number): Promise<void> {
    if (!odontologoId) return
    try {
      await horariosRepository.eliminar(odontologoId, franjaId)
      avisarExito('Horario eliminado correctamente.')
      refrescar()
    } catch (error) {
      avisarError(error)
    }
  }

  return {
    franjas,
    cargando,
    guardando,
    guardarFranja,
    eliminarFranja,
  }
}