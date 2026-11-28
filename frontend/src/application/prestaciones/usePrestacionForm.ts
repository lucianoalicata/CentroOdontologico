import { useEffect, useState } from 'react'
import { useForm } from '@mantine/form'
import { prestacionesRepository } from '../../infrastructure/prestaciones/prestacionesRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { CategoriaPrestacion, PrestacionDto } from '../../domain/prestaciones/types'

// Deben coincidir con las reglas del dominio (Prestacion.cs)
export const LONGITUD_MAXIMA_CODIGO = 30
export const LONGITUD_MAXIMA_TITULO = 200
export const DURACION_MAXIMA_MINUTOS = 480

interface ValoresFormulario {
  codigo: string
  titulo: string
  categoria: CategoriaPrestacion | null
  duracionEstimadaMinutos: number | string
  precioParticular: number | string
  requiereAutorizacionPrevia: boolean
}

const VALORES_VACIOS: ValoresFormulario = {
  codigo: '',
  titulo: '',
  categoria: null,
  duracionEstimadaMinutos: 30,
  precioParticular: 0,
  requiereAutorizacionPrevia: false,
}

interface Opciones {
  abierto: boolean
  prestacionEditar?: PrestacionDto | null
  onGuardado: (prestacion: PrestacionDto) => void
  onCerrar: () => void
}

/**
 * Caso de uso: crear o editar una prestación del catálogo.
 * El código solo se informa en el alta: identifica a la prestación y no se edita.
 */
export function usePrestacionForm({ abierto, prestacionEditar, onGuardado, onCerrar }: Opciones) {
  const [guardando, setGuardando] = useState(false)
  const esEdicion = Boolean(prestacionEditar)

  const form = useForm<ValoresFormulario>({
    initialValues: VALORES_VACIOS,
    validate: {
      codigo: (v) => {
        if (esEdicion) return null
        const limpio = v.trim()
        if (limpio.length === 0) return 'El código es obligatorio.'
        if (limpio.length > LONGITUD_MAXIMA_CODIGO)
          return `El código no puede superar los ${LONGITUD_MAXIMA_CODIGO} caracteres.`
        return null
      },
      titulo: (v) => {
        const limpio = v.trim()
        if (limpio.length === 0) return 'El título es obligatorio.'
        if (limpio.length > LONGITUD_MAXIMA_TITULO)
          return `El título no puede superar los ${LONGITUD_MAXIMA_TITULO} caracteres.`
        return null
      },
      categoria: (v) => (v ? null : 'La categoría es obligatoria.'),
      duracionEstimadaMinutos: (v) => {
        const n = Number(v)
        if (!Number.isInteger(n) || n <= 0) return 'La duración debe ser mayor a cero.'
        if (n > DURACION_MAXIMA_MINUTOS)
          return `La duración no puede superar los ${DURACION_MAXIMA_MINUTOS} minutos.`
        return null
      },
      precioParticular: (v) =>
        v === '' || Number(v) < 0 ? 'El precio no puede ser negativo.' : null,
    },
  })

  useEffect(() => {
    if (!abierto) return

    form.setValues(
      prestacionEditar
        ? {
            codigo: prestacionEditar.codigo,
            titulo: prestacionEditar.titulo,
            categoria: prestacionEditar.categoria,
            duracionEstimadaMinutos: prestacionEditar.duracionEstimadaMinutos,
            precioParticular: prestacionEditar.precioParticular,
            requiereAutorizacionPrevia: prestacionEditar.requiereAutorizacionPrevia,
          }
        : VALORES_VACIOS,
    )
    form.clearErrors()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [abierto, prestacionEditar])

  async function guardar(valores: ValoresFormulario) {
    setGuardando(true)
    try {
      const datos = {
        titulo: valores.titulo.trim(),
        categoria: valores.categoria as CategoriaPrestacion,
        duracionEstimadaMinutos: Number(valores.duracionEstimadaMinutos),
        precioParticular: Number(valores.precioParticular),
        requiereAutorizacionPrevia: valores.requiereAutorizacionPrevia,
      }

      let resultado: PrestacionDto
      if (prestacionEditar) {
        resultado = await prestacionesRepository.actualizar(prestacionEditar.id, datos)
        avisarExito('Prestación actualizada correctamente.')
      } else {
        resultado = await prestacionesRepository.crear({ codigo: valores.codigo.trim(), ...datos })
        avisarExito('Prestación creada correctamente.')
      }

      onGuardado(resultado)
      onCerrar()
    } catch (error) {
      avisarError(error)
    } finally {
      setGuardando(false)
    }
  }

  return { form, guardando, esEdicion, guardar }
}
