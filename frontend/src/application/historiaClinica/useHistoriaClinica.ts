import { useCallback, useEffect, useState } from 'react'
import { historiaClinicaRepository } from '../../infrastructure/historiaClinica/historiaClinicaRepository'
import { pacientesRepository } from '../../infrastructure/pacientes/pacientesRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { HistoriaClinicaDto } from '../../domain/historiaClinica/types'
import type { PacienteDto } from '../../domain/pacientes/types'

/**
 * Caso de uso: ver la historia clínica de un paciente y editar sus alertas médicas.
 * El alta de entradas vive en useRegistrarEntrada.
 */
export function useHistoriaClinica(pacienteId: number) {
  const [paciente, setPaciente] = useState<PacienteDto | null>(null)
  const [historia, setHistoria] = useState<HistoriaClinicaDto | null>(null)
  const [cargando, setCargando] = useState(true)
  const [guardandoAlertas, setGuardandoAlertas] = useState(false)
  const [recarga, setRecarga] = useState(0)

  useEffect(() => {
    if (!Number.isInteger(pacienteId) || pacienteId <= 0) {
      setCargando(false)
      return
    }
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const [datosPaciente, datosHistoria] = await Promise.all([
          pacientesRepository.obtenerPorId(pacienteId),
          historiaClinicaRepository.obtener(pacienteId),
        ])
        if (cancelado) return
        setPaciente(datosPaciente)
        setHistoria(datosHistoria)
      } catch (error) {
        if (!cancelado) avisarError(error)
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()
    return () => {
      cancelado = true
    }
  }, [pacienteId, recarga])

  const forzarRecarga = useCallback(() => setRecarga((n) => n + 1), [])

  async function guardarAlertas(alertas: string): Promise<boolean> {
    setGuardandoAlertas(true)
    try {
      const actualizada = await historiaClinicaRepository.actualizarAlertas(pacienteId, {
        alertasMedicas: alertas.trim() || null,
      })
      setHistoria(actualizada)
      avisarExito('Alertas médicas actualizadas.')
      return true
    } catch (error) {
      avisarError(error)
      return false
    } finally {
      setGuardandoAlertas(false)
    }
  }

  return { paciente, historia, cargando, guardandoAlertas, guardarAlertas, forzarRecarga }
}
