import { useEffect, useState } from 'react'
import { useForm } from '@mantine/form'
import { historiaClinicaRepository } from '../../infrastructure/historiaClinica/historiaClinicaRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import {
  LONGITUD_MAXIMA_COMENTARIO,
  esPiezaFdiValida,
} from '../../domain/historiaClinica/types'
import type {
  EntradaHistoriaClinicaDto,
  RegistrarEntradaDto,
} from '../../domain/historiaClinica/types'
import type { PrestacionDto } from '../../domain/prestaciones/types'

export interface LineaPrestacion {
  /** Clave local para la lista (no viaja a la API). */
  clave: number
  prestacion: PrestacionDto
  cantidad: number | string
  piezaDental: number | string
  mesesGarantia: number | string
}

interface ValoresFormulario {
  comentario: string
  lineas: LineaPrestacion[]
}

interface Opciones {
  abierto: boolean
  pacienteId: number
  /** Si se indica, la entrada nueva corrige a esta (las entradas no se editan). */
  corrigeEntrada?: EntradaHistoriaClinicaDto | null
  onRegistrada: (entrada: EntradaHistoriaClinicaDto) => void
  onCerrar: () => void
}

const opcional = (v: number | string): number | null =>
  v === '' || v === null || v === undefined ? null : Number(v)

/**
 * Caso de uso: registrar una entrada de la historia clínica (solo odontólogo).
 * Las prestaciones se eligen del catálogo; la API copia código, título y precio vigentes.
 */
export function useRegistrarEntrada({
  abierto,
  pacienteId,
  corrigeEntrada,
  onRegistrada,
  onCerrar,
}: Opciones) {
  const [guardando, setGuardando] = useState(false)
  const [siguienteClave, setSiguienteClave] = useState(1)

  const form = useForm<ValoresFormulario>({
    initialValues: { comentario: '', lineas: [] },
    validate: (valores) => {
      const errores: Record<string, string> = {}
      const comentario = valores.comentario.trim()

      if (comentario.length === 0 && valores.lineas.length === 0)
        errores.comentario = 'Ingrese un comentario o al menos una prestación realizada.'
      if (comentario.length > LONGITUD_MAXIMA_COMENTARIO)
        errores.comentario = `El comentario no puede superar los ${LONGITUD_MAXIMA_COMENTARIO} caracteres.`

      valores.lineas.forEach((linea, i) => {
        const cantidad = Number(linea.cantidad)
        if (!Number.isInteger(cantidad) || cantidad < 1)
          errores[`lineas.${i}.cantidad`] = 'Debe ser al menos 1.'

        const pieza = opcional(linea.piezaDental)
        if (pieza !== null && !esPiezaFdiValida(pieza))
          errores[`lineas.${i}.piezaDental`] = 'Pieza FDI no válida.'

        const garantia = opcional(linea.mesesGarantia)
        if (garantia !== null && (!Number.isInteger(garantia) || garantia <= 0))
          errores[`lineas.${i}.mesesGarantia`] = 'Debe ser mayor a cero.'
      })

      return errores
    },
  })

  useEffect(() => {
    if (!abierto) return
    form.setValues({ comentario: '', lineas: [] })
    form.clearErrors()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [abierto])

  function agregarPrestacion(prestacion: PrestacionDto) {
    form.insertListItem('lineas', {
      clave: siguienteClave,
      prestacion,
      cantidad: 1,
      piezaDental: '',
      mesesGarantia: '',
    } satisfies LineaPrestacion)
    setSiguienteClave((n) => n + 1)
    form.clearFieldError('comentario')
  }

  function quitarPrestacion(indice: number) {
    form.removeListItem('lineas', indice)
  }

  async function guardar(valores: ValoresFormulario) {
    setGuardando(true)
    try {
      const dto: RegistrarEntradaDto = {
        comentario: valores.comentario.trim() || null,
        corrigeEntradaId: corrigeEntrada?.id ?? null,
        prestaciones: valores.lineas.map((l) => ({
          prestacionId: l.prestacion.id,
          cantidad: Number(l.cantidad),
          piezaDental: opcional(l.piezaDental),
          mesesGarantia: opcional(l.mesesGarantia),
        })),
      }

      const entrada = await historiaClinicaRepository.registrarEntrada(pacienteId, dto)
      avisarExito(corrigeEntrada ? 'Corrección registrada.' : 'Entrada registrada en la historia clínica.')
      onRegistrada(entrada)
      onCerrar()
    } catch (error) {
      avisarError(error)
    } finally {
      setGuardando(false)
    }
  }

  const total = form.values.lineas.reduce(
    (suma, l) => suma + l.prestacion.precioParticular * (Number(l.cantidad) || 0),
    0,
  )

  return { form, guardando, total, agregarPrestacion, quitarPrestacion, guardar }
}
