import { ActionIcon, Group, Tooltip } from '@mantine/core'
import { IconEdit, IconTrash } from '@tabler/icons-react'

interface Props {
  onEditar: () => void
  onEliminar: () => void
  etiquetaEditar?: string
  etiquetaEliminar?: string
}

/**
 * Molecule: grupo de botones de acción (editar / eliminar) para filas de tabla.
 * Reutilizable en cualquier entidad que necesite estas dos acciones.
 */
export function AccionesTabla({
  onEditar,
  onEliminar,
  etiquetaEditar = 'Editar',
  etiquetaEliminar = 'Dar de baja',
}: Props) {
  return (
    <Group gap="xs" justify="flex-end">
      <Tooltip label={etiquetaEditar}>
        <ActionIcon
          variant="subtle"
          color="blue"
          aria-label={etiquetaEditar}
          onClick={onEditar}
        >
          <IconEdit size={16} />
        </ActionIcon>
      </Tooltip>
      <Tooltip label={etiquetaEliminar}>
        <ActionIcon
          variant="subtle"
          color="red"
          aria-label={etiquetaEliminar}
          onClick={onEliminar}
        >
          <IconTrash size={16} />
        </ActionIcon>
      </Tooltip>
    </Group>
  )
}
