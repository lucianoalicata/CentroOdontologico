import { Text } from '@mantine/core'

interface Props {
  mensaje: string
}

/**
 * Atom: mensaje de lista vacía centrado con estilo dimmed.
 * Sin estado propio. Solo presentación.
 */
export function EstadoVacio({ mensaje }: Props) {
  return (
    <Text c="dimmed" ta="center" py="xl">
      {mensaje}
    </Text>
  )
}
