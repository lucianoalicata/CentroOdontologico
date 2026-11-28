import { useEffect, useState } from 'react'
import { useDebouncedValue } from '@mantine/hooks'
import { prestacionesRepository } from '../../infrastructure/prestaciones/prestacionesRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { CategoriaPrestacion, PrestacionDto } from '../../domain/prestaciones/types'

/**
 * Caso de uso: gestionar el catálogo completo de prestaciones (administrador).
 * Lista, filtra, da de baja y reactiva. El formulario vive en usePrestacionForm.
 */
export function usePrestacionesAdmin() {
  const [prestaciones, setPrestaciones] = useState<PrestacionDto[]>([])
  const [cargando, setCargando] = useState(true)
  const [filtro, setFiltro] = useState('')
  const [filtroDebounced] = useDebouncedValue(filtro, 350)
  const [categoria, setCategoria] = useState<CategoriaPrestacion | null>(null)
  const [mostrarInactivas, setMostrarInactivas] = useState(false)
  const [recarga, setRecarga] = useState(0)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const lista = await prestacionesRepository.listarTodas({ filtro: filtroDebounced, categoria })
        if (!cancelado) setPrestaciones(lista)
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
  }, [filtroDebounced, categoria, recarga])

  const visibles = mostrarInactivas ? prestaciones : prestaciones.filter((p) => p.activo)

  function forzarRecarga() {
    setRecarga((n) => n + 1)
  }

  async function darDeBaja(prestacion: PrestacionDto): Promise<boolean> {
    try {
      await prestacionesRepository.darDeBaja(prestacion.id)
      avisarExito('Prestación dada de baja correctamente.')
      forzarRecarga()
      return true
    } catch (error) {
      avisarError(error)
      return false
    }
  }

  async function reactivar(prestacion: PrestacionDto): Promise<boolean> {
    try {
      await prestacionesRepository.reactivar(prestacion.id)
      avisarExito('Prestación reactivada correctamente.')
      forzarRecarga()
      return true
    } catch (error) {
      avisarError(error)
      return false
    }
  }

  return {
    prestaciones: visibles,
    totalActivas: prestaciones.filter((p) => p.activo).length,
    cargando,
    filtro,
    categoria,
    mostrarInactivas,
    cambiarFiltro: setFiltro,
    cambiarCategoria: setCategoria,
    cambiarMostrarInactivas: setMostrarInactivas,
    forzarRecarga,
    darDeBaja,
    reactivar,
  }
}
