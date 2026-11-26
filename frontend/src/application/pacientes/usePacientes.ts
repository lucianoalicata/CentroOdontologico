import { useEffect, useState } from 'react'
import { useDebouncedValue } from '@mantine/hooks'
import { pacientesRepository } from '../../infrastructure/pacientes/pacientesRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { PacienteDto } from '../../domain/pacientes/types'

const TAMANIO_PAGINA = 10

/**
 * Caso de uso: listar, filtrar, paginar y dar de baja pacientes.
 * SRP: solo gestiona la lista de pacientes (no el formulario).
 * DIP: depende del repositorio (abstracción), no de apiClient directamente.
 */
export function usePacientes() {
  const [pacientes, setPacientes] = useState<PacienteDto[]>([])
  const [cargando, setCargando] = useState(true)
  const [filtro, setFiltro] = useState('')
  const [filtroDebounced] = useDebouncedValue(filtro, 350)

  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [totalItems, setTotalItems] = useState(0)
  const [recarga, setRecarga] = useState(0)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const res = await pacientesRepository.buscar(filtroDebounced, pagina, TAMANIO_PAGINA)
        if (cancelado) return
        setPacientes(res.items)
        setTotalPaginas(Math.max(1, res.totalPages))
        setTotalItems(res.totalItems)
      } catch (error) {
        if (!cancelado) avisarError(error)
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()
    return () => { cancelado = true }
  }, [filtroDebounced, pagina, recarga])

  function cambiarFiltro(valor: string) {
    setFiltro(valor)
    setPagina(1)
  }

  function cambiarPagina(nuevaPagina: number) {
    setPagina(nuevaPagina)
  }

  function forzarRecarga() {
    setRecarga((n) => n + 1)
  }

  async function darDeBaja(paciente: PacienteDto): Promise<boolean> {
    try {
      await pacientesRepository.darDeBaja(paciente.id)
      avisarExito('Paciente dado de baja correctamente.')
      forzarRecarga()
      return true
    } catch (error) {
      avisarError(error)
      return false
    }
  }

  return {
    pacientes,
    cargando,
    filtro,
    pagina,
    totalPaginas,
    totalItems,
    cambiarFiltro,
    cambiarPagina,
    forzarRecarga,
    darDeBaja,
  }
}
