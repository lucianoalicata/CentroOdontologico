import { useEffect, useState } from 'react'
import { prestacionesRepository } from '../../infrastructure/prestaciones/prestacionesRepository'
import { avisarError } from '../../shared/utils/avisos'
import type { PrestacionDto } from '../../domain/prestaciones/types'

/**
 * Caso de uso: obtener el catálogo de prestaciones activas para elegir al registrar una atención.
 * Se carga cuando `habilitado` es verdadero (por ejemplo, al abrir el formulario).
 */
export function usePrestacionesActivas(habilitado = true) {
  const [prestaciones, setPrestaciones] = useState<PrestacionDto[]>([])
  const [cargando, setCargando] = useState(false)

  useEffect(() => {
    if (!habilitado) return
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const lista = await prestacionesRepository.listarActivas()
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
  }, [habilitado])

  return { prestaciones, cargando }
}
