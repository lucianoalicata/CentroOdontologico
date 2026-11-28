import { Button, Checkbox, Group, Modal, NumberInput, Select, Stack, TextInput } from '@mantine/core'
import {
  DURACION_MAXIMA_MINUTOS,
  LONGITUD_MAXIMA_CODIGO,
  LONGITUD_MAXIMA_TITULO,
  usePrestacionForm,
} from '../../../application/prestaciones/usePrestacionForm'
import { CATEGORIAS_PRESTACION, ETIQUETAS_CATEGORIA } from '../../../domain/prestaciones/types'
import type { PrestacionDto } from '../../../domain/prestaciones/types'

interface Props {
  abierto: boolean
  onCerrar: () => void
  onGuardado: (prestacion: PrestacionDto) => void
  prestacionEditar?: PrestacionDto | null
}

const OPCIONES_CATEGORIA = CATEGORIAS_PRESTACION.map((c) => ({
  value: c,
  label: ETIQUETAS_CATEGORIA[c],
}))

/**
 * Organism: modal de alta y edición de una prestación del catálogo.
 * Delega la lógica en usePrestacionForm.
 */
export function PrestacionFormModal({ abierto, onCerrar, onGuardado, prestacionEditar }: Props) {
  const { form, guardando, esEdicion, guardar } = usePrestacionForm({
    abierto,
    prestacionEditar,
    onGuardado,
    onCerrar,
  })

  return (
    <Modal
      opened={abierto}
      onClose={onCerrar}
      title={esEdicion ? 'Editar prestación' : 'Nueva prestación'}
      size="md"
    >
      <form onSubmit={form.onSubmit(guardar)}>
        <Stack>
          <Group grow align="flex-start">
            <TextInput
              label="Código"
              placeholder="Ej. 02.01"
              required={!esEdicion}
              disabled={esEdicion}
              maxLength={LONGITUD_MAXIMA_CODIGO}
              {...form.getInputProps('codigo')}
            />
            <Select
              label="Categoría"
              placeholder="Seleccionar"
              required
              data={OPCIONES_CATEGORIA}
              allowDeselect={false}
              {...form.getInputProps('categoria')}
            />
          </Group>

          <TextInput
            label="Título"
            required
            maxLength={LONGITUD_MAXIMA_TITULO}
            {...form.getInputProps('titulo')}
          />

          <Group grow align="flex-start">
            <NumberInput
              label="Duración estimada (min)"
              required
              min={1}
              max={DURACION_MAXIMA_MINUTOS}
              step={5}
              allowDecimal={false}
              {...form.getInputProps('duracionEstimadaMinutos')}
            />
            <NumberInput
              label="Precio particular"
              required
              min={0}
              decimalScale={2}
              thousandSeparator="."
              decimalSeparator=","
              prefix="$ "
              {...form.getInputProps('precioParticular')}
            />
          </Group>

          <Checkbox
            label="Requiere autorización previa de la obra social"
            {...form.getInputProps('requiereAutorizacionPrevia', { type: 'checkbox' })}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onCerrar} disabled={guardando}>
              Cancelar
            </Button>
            <Button type="submit" loading={guardando}>
              {esEdicion ? 'Guardar cambios' : 'Crear prestación'}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
