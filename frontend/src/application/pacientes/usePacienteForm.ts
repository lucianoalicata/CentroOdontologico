import { useEffect, useState } from 'react'
import { useForm } from '@mantine/form'
import { pacientesRepository } from '../../infrastructure/pacientes/pacientesRepository'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import type { PacienteDto } from '../../domain/pacientes/types'

export interface ValoresInicialesPaciente {
  nombre?: string
  apellido?: string
  dni?: string
  telefono?: string
  fechaNacimiento?: string
  domicilio?: string
}

interface UsePacienteFormOptions {
  abierto: boolean
  pacienteEditar?: PacienteDto | null
  valoresIniciales?: ValoresInicialesPaciente
  onGuardado: (paciente: PacienteDto) => void
  onCerrar: () => void
}

/**
 * Caso de uso: crear o editar un paciente.
 * SRP: solo gestiona el formulario y la operación de guardado.
 * ISP: separado de usePacientes, que maneja la lista.
 */
export function usePacienteForm({
  abierto,
  pacienteEditar,
  valoresIniciales,
  onGuardado,
  onCerrar,
}: UsePacienteFormOptions) {
  const [guardando, setGuardando] = useState(false)
  const esEdicion = Boolean(pacienteEditar)

  const form = useForm({
    initialValues: {
      nombre: '',
      apellido: '',
      dni: '',
      telefono: '',
      fechaNacimiento: '',
      domicilio: '',
    },
    validate: {
      nombre: (v) => (v.trim().length === 0 ? 'El nombre es obligatorio.' : null),
      apellido: (v) => (v.trim().length === 0 ? 'El apellido es obligatorio.' : null),
      dni: (v) => {
        if (esEdicion) return null
        const limpio = v.replace(/\./g, '').trim()
        return /^\d{7,8}$/.test(limpio) ? null : 'El DNI debe tener 7 u 8 dígitos.'
      },
      telefono: (v) => (v.trim().length === 0 ? 'El teléfono es obligatorio.' : null),
      fechaNacimiento: (v) =>
        v.trim().length === 0 ? 'La fecha de nacimiento es obligatoria.' : null,
    },
  })

  // Sincroniza el formulario al abrir el modal
  useEffect(() => {
    if (!abierto) return

    if (pacienteEditar) {
      form.setValues({
        nombre: pacienteEditar.nombre,
        apellido: pacienteEditar.apellido,
        dni: pacienteEditar.dni,
        telefono: pacienteEditar.telefono,
        fechaNacimiento: pacienteEditar.fechaNacimiento,
        domicilio: pacienteEditar.domicilio ?? '',
      })
    } else {
      form.setValues({
        nombre: valoresIniciales?.nombre ?? '',
        apellido: valoresIniciales?.apellido ?? '',
        dni: valoresIniciales?.dni ?? '',
        telefono: valoresIniciales?.telefono ?? '',
        fechaNacimiento: valoresIniciales?.fechaNacimiento ?? '',
        domicilio: valoresIniciales?.domicilio ?? '',
      })
    }
    form.clearErrors()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [abierto, pacienteEditar, valoresIniciales])

  async function guardar(valores: typeof form.values) {
    setGuardando(true)
    try {
      const domicilioLimpio = valores.domicilio.trim() || null
      let resultado: PacienteDto

      if (pacienteEditar) {
        resultado = await pacientesRepository.actualizar(pacienteEditar.id, {
          nombre: valores.nombre.trim(),
          apellido: valores.apellido.trim(),
          telefono: valores.telefono.trim(),
          fechaNacimiento: valores.fechaNacimiento,
          domicilio: domicilioLimpio,
        })
        avisarExito('Datos del paciente actualizados correctamente.')
      } else {
        resultado = await pacientesRepository.registrar({
          nombre: valores.nombre.trim(),
          apellido: valores.apellido.trim(),
          dni: valores.dni.replace(/\./g, '').trim(),
          telefono: valores.telefono.trim(),
          fechaNacimiento: valores.fechaNacimiento,
          domicilio: domicilioLimpio,
        })
        avisarExito('Paciente registrado correctamente.')
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
