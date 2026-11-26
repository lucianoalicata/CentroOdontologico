import { useEffect, useState } from 'react'
import { useDebouncedValue } from '@mantine/hooks'
import { pacientesRepository } from '../../infrastructure/pacientes/pacientesRepository'
import { turnosRepository } from '../../infrastructure/turnos/turnosRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { PacienteDto } from '../../domain/pacientes/types'
import type { FranjaLibreDto } from '../../domain/turnos/types'

export type TipoTurno = 'programado' | 'entreturno'

interface UseTurnoFormOptions {
  abierto: boolean
  odontologoInicialId?: number
  fechaInicial?: string
  onGuardado: () => void
  onCerrar: () => void
}

/**
 * Caso de uso: crear un turno programado o un entre-turno.
 * SRP: maneja el estado y lógica del formulario de turno.
 * ISP: separado de useOdontologos y usePacientes.
 */
export function useTurnoForm({
  abierto,
  odontologoInicialId,
  fechaInicial,
  onGuardado,
  onCerrar,
}: UseTurnoFormOptions) {
  const hoyIso = new Date().toISOString().slice(0, 10)

  const [tipo, setTipo] = useState<TipoTurno>('programado')
  const [guardando, setGuardando] = useState(false)

  // Campos comunes
  const [odontologoId, setOdontologoId] = useState<string | null>(
    odontologoInicialId ? String(odontologoInicialId) : null,
  )
  const [fecha, setFecha] = useState<string>(fechaInicial ?? hoyIso)
  const [motivo, setMotivo] = useState('')

  // Campos de turno programado
  const [busquedaPaciente, setBusquedaPaciente] = useState('')
  const [busquedaDebounced] = useDebouncedValue(busquedaPaciente, 300)
  const [opcionesPacientes, setOpcionesPacientes] = useState<PacienteDto[]>([])
  const [pacienteId, setPacienteId] = useState<string | null>(null)
  const [franjas, setFranjas] = useState<FranjaLibreDto[]>([])
  const [cargandoFranjas, setCargandoFranjas] = useState(false)
  const [franjaSeleccionada, setFranjaSeleccionada] = useState<string | null>(null)

  // Campos de entre-turno
  const [nombreEntreTurno, setNombreEntreTurno] = useState('')
  const [telefonoEntreTurno, setTelefonoEntreTurno] = useState('')
  const [dniEntreTurno, setDniEntreTurno] = useState('')
  const [horaEntreTurno, setHoraEntreTurno] = useState('')

  // Buscar pacientes con debounce
  useEffect(() => {
    if (!abierto || tipo !== 'programado') return
    let cancelado = false

    async function cargarPacientes() {
      try {
        const res = await pacientesRepository.buscar(busquedaDebounced, 1, 15)
        if (!cancelado) setOpcionesPacientes(res.items)
      } catch {
        // Silencioso en autocompletado
      }
    }

    cargarPacientes()
    return () => { cancelado = true }
  }, [abierto, tipo, busquedaDebounced])

  // Cargar franjas cuando cambia odontólogo o fecha
  useEffect(() => {
    if (!abierto || tipo !== 'programado' || !odontologoId || !fecha) return
    let cancelado = false

    async function cargarFranjas() {
      setCargandoFranjas(true)
      try {
        const disponibles = await turnosRepository.obtenerFranjasLibres(
          Number(odontologoId),
          fecha,
        )
        if (!cancelado) setFranjas(disponibles)
      } catch {
        if (!cancelado) setFranjas([])
      } finally {
        if (!cancelado) setCargandoFranjas(false)
      }
    }

    cargarFranjas()
    return () => { cancelado = true }
  }, [abierto, tipo, odontologoId, fecha])

  function agregarPacienteNuevo(nuevo: PacienteDto) {
    setOpcionesPacientes((prev) => [nuevo, ...prev.filter((p) => p.id !== nuevo.id)])
    setPacienteId(String(nuevo.id))
  }

  function limpiarYcerrar() {
    setPacienteId(null)
    setFranjaSeleccionada(null)
    setNombreEntreTurno('')
    setTelefonoEntreTurno('')
    setDniEntreTurno('')
    setHoraEntreTurno('')
    setMotivo('')
    onCerrar()
  }

  function cambiarOdontologo(val: string | null) {
    setOdontologoId(val)
    setFranjaSeleccionada(null)
  }

  function cambiarFecha(val: string) {
    setFecha(val)
    setFranjaSeleccionada(null)
  }

  function cambiarTipo(val: TipoTurno) {
    setTipo(val)
    setFranjaSeleccionada(null)
  }

  async function guardar(): Promise<void> {
    if (!odontologoId || !fecha) {
      avisarError('Seleccioná un odontólogo y una fecha.')
      return
    }

    setGuardando(true)
    try {
      if (tipo === 'programado') {
        if (!pacienteId || !franjaSeleccionada) {
          avisarError('Seleccioná un paciente y una franja horaria libre.')
          return
        }
        await turnosRepository.crearProgramado({
          odontologoId: Number(odontologoId),
          pacienteId: Number(pacienteId),
          fecha,
          horaInicio: franjaSeleccionada,
          motivo: motivo.trim() || null,
        })
        avisarExito('Turno programado registrado correctamente.')
      } else {
        if (!nombreEntreTurno.trim() || !telefonoEntreTurno.trim() || !horaEntreTurno) {
          avisarError('Completá nombre, teléfono y hora para el entre-turno.')
          return
        }
        await turnosRepository.crearEntreTurno({
          odontologoId: Number(odontologoId),
          fecha,
          horaInicio: horaEntreTurno,
          nombre: nombreEntreTurno.trim(),
          telefono: telefonoEntreTurno.trim(),
          dni: dniEntreTurno.trim() || null,
          motivo: motivo.trim() || null,
        })
        avisarExito('Entre-turno registrado correctamente.')
      }

      onGuardado()
      limpiarYcerrar()
    } catch (error) {
      avisarError(error)
    } finally {
      setGuardando(false)
    }
  }

  return {
    // Estado
    tipo,
    guardando,
    odontologoId,
    fecha,
    motivo,
    busquedaPaciente,
    opcionesPacientes,
    pacienteId,
    franjas,
    cargandoFranjas,
    franjaSeleccionada,
    nombreEntreTurno,
    telefonoEntreTurno,
    dniEntreTurno,
    horaEntreTurno,
    // Acciones
    cambiarTipo,
    cambiarOdontologo,
    cambiarFecha,
    setMotivo,
    setBusquedaPaciente,
    setPacienteId,
    setFranjaSeleccionada,
    setNombreEntreTurno,
    setTelefonoEntreTurno,
    setDniEntreTurno,
    setHoraEntreTurno,
    agregarPacienteNuevo,
    limpiarYcerrar,
    guardar,
  }
}
