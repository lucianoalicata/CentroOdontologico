import { Button, Group, Modal, Stack, TextInput } from '@mantine/core'
import { usePacienteForm } from '../../../application/pacientes/usePacienteForm'
import type { PacienteDto } from '../../../domain/pacientes/types'
import type { ValoresInicialesPaciente } from '../../../application/pacientes/usePacienteForm'

interface Props {
  abierto: boolean
  onCerrar: () => void
  onGuardado: (paciente: PacienteDto) => void
  pacienteEditar?: PacienteDto | null
  valoresIniciales?: ValoresInicialesPaciente
}

/**
 * Organism: modal de creación/edición de paciente.
 * Delega toda la lógica al hook usePacienteForm (DIP).
 */
export function PacienteFormModal({
  abierto,
  onCerrar,
  onGuardado,
  pacienteEditar,
  valoresIniciales,
}: Props) {
  const { form, guardando, esEdicion, guardar } = usePacienteForm({
    abierto,
    pacienteEditar,
    valoresIniciales,
    onGuardado,
    onCerrar,
  })

  return (
    <Modal
      opened={abierto}
      onClose={onCerrar}
      title={esEdicion ? 'Editar paciente' : 'Nuevo paciente'}
      size="md"
    >
      <form onSubmit={form.onSubmit(guardar)}>
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
