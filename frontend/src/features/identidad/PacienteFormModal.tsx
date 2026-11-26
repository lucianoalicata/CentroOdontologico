import { useEffect, useState } from 'react'
import { Button, Group, Modal, Stack, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import { actualizarPaciente, registrarPaciente } from './pacientesApi'
import type { PacienteDto } from './pacientesApi'

export interface ValoresInicialesPaciente {
  nombre?: string
  apellido?: string
  dni?: string
  telefono?: string
  fechaNacimiento?: string
  domicilio?: string
}

interface Props {
  abierto: boolean
  onCerrar: () => void
  onGuardado: (paciente: PacienteDto) => void
  pacienteEditar?: PacienteDto | null
  valoresIniciales?: ValoresInicialesPaciente
}

export function PacienteFormModal({
  abierto,
  onCerrar,
  onGuardado,
  pacienteEditar,
  valoresIniciales,
}: Props) {
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

  async function manejarSubmit(valores: typeof form.values) {
    setGuardando(true)
    try {
      const domicilioLimpio = valores.domicilio.trim() || null
      let resultado: PacienteDto

      if (pacienteEditar) {
        resultado = await actualizarPaciente(pacienteEditar.id, {
          nombre: valores.nombre.trim(),
          apellido: valores.apellido.trim(),
          telefono: valores.telefono.trim(),
          fechaNacimiento: valores.fechaNacimiento,
          domicilio: domicilioLimpio,
        })
        avisarExito('Datos del paciente actualizados correctamente.')
      } else {
        resultado = await registrarPaciente({
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

  return (
    <Modal
      opened={abierto}
      onClose={onCerrar}
      title={esEdicion ? 'Editar paciente' : 'Nuevo paciente'}
      size="md"
    >
      <form onSubmit={form.onSubmit(manejarSubmit)}>
        <Stack>
          <Group grow>
            <TextInput label="Nombre" required {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required {...form.getInputProps('apellido')} />
          </Group>

          <Group grow>
            <TextInput
              label="DNI"
              placeholder="Sin puntos"
              required={!esEdicion}
              disabled={esEdicion}
              {...form.getInputProps('dni')}
            />
            <TextInput
              label="Fecha de nacimiento"
              type="date"
              required
              {...form.getInputProps('fechaNacimiento')}
            />
          </Group>

          <TextInput label="Teléfono" required {...form.getInputProps('telefono')} />
          <TextInput
            label="Domicilio"
            placeholder="Calle, número, localidad (opcional)"
            {...form.getInputProps('domicilio')}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onCerrar} disabled={guardando}>
              Cancelar
            </Button>
            <Button type="submit" loading={guardando}>
              {esEdicion ? 'Guardar cambios' : 'Registrar paciente'}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}