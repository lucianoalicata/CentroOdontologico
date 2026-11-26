import { useEffect, useState } from 'react'
import { odontologosRepository } from '../../infrastructure/odontologos/odontologosRepository'
import { avisarError } from '../../shared/utils/avisos'
import type { OdontologoActivoDto } from '../../domain/odontologos/types'

/**
 * Caso de uso: obtener la lista de odontólogos activos.
 * Se activa al montarse (o cuando cambia `habilitado`).
 */
export function useOdontologos(habilitado = true) {
  const [odontologos, setOdontologos] = useState<OdontologoActivoDto[]>([])
  const [cargando, setCargando] = useState(false)

  useEffect(() => {
    if (!habilitado) return
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const lista = await odontologosRepository.listarActivos()
        if (!cancelado) setOdontologos(lista)
      } catch (error) {
        if (!cancelado) avisarError(error)
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()
    return () => { cancelado = true }
  }, [habilitado])

  return { odontologos, cargando }
}
