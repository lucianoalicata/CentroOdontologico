import { Badge } from '@mantine/core'

interface Props {
  activo: boolean
}

/**
 * Atom: badge que indica si un paciente tiene cuenta web activa.
 * Sin estado propio. Solo presentación.
 */
export function EstadoPortalBadge({ activo }: Props) {
  return (
    <Badge color={activo ? 'teal' : 'gray'} variant="light">
      {activo ? 'Activo' : 'Sin cuenta'}
    </Badge>
  )
}
