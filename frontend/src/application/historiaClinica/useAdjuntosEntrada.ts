import { useEffect, useState } from 'react'
import { adjuntosRepository } from '../../infrastructure/historiaClinica/adjuntosRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import { guardarArchivo } from '../../shared/utils/descargas'
import { validarArchivoAdjunto } from '../../domain/historiaClinica/adjuntos'
import type { ArchivoAdjuntoDto } from '../../domain/historiaClinica/adjuntos'

/**
 * Caso de uso: listar, subir y descargar los archivos adjuntos de una entrada.
 * Los adjuntos son datos clínicos: no se eliminan.
 */
export function useAdjuntosEntrada(entradaId: number) {
  const [adjuntos, setAdjuntos] = useState<ArchivoAdjuntoDto[]>([])
  const [cargando, setCargando] = useState(true)
  const [subiendo, setSubiendo] = useState(false)
  const [descargandoId, setDescargandoId] = useState<number | null>(null)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      setCargando(true)
      try {
        const lista = await adjuntosRepository.listar(entradaId)
        if (!cancelado) setAdjuntos(lista)
      } catch (error) {
        if (!cancelado) avisarError(error, 'No se pudieron cargar los adjuntos')
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()
    return () => {
      cancelado = true
    }
  }, [entradaId])

  async function subir(archivo: File | null): Promise<void> {
    if (!archivo) return

    const problema = validarArchivoAdjunto(archivo)
    if (problema) {
      avisarError(problema, 'Archivo no válido')
      return
    }

    setSubiendo(true)
    try {
      const nuevo = await adjuntosRepository.subir(entradaId, archivo)
      setAdjuntos((actuales) => [...actuales, nuevo])
      avisarExito(`Se adjuntó ${nuevo.nombreOriginal}.`)
    } catch (error) {
      avisarError(error)
    } finally {
      setSubiendo(false)
    }
  }

  async function descargar(adjunto: ArchivoAdjuntoDto): Promise<void> {
    setDescargandoId(adjunto.id)
    try {
      const contenido = await adjuntosRepository.descargar(adjunto.id)
      guardarArchivo(contenido, adjunto.nombreOriginal)
    } catch (error) {
      avisarError(error)
    } finally {
      setDescargandoId(null)
    }
  }

  return { adjuntos, cargando, subiendo, descargandoId, subir, descargar }
}
