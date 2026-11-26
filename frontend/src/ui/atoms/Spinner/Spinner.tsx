import { Center, Loader } from '@mantine/core'
import type { MantineSize } from '@mantine/core'

interface Props {
  altura?: string | number
  tamanio?: MantineSize
}

/**
 * Atom: indicador de carga centrado.
 * Sin estado propio. Solo presentación.
 */
export function Spinner({ altura = '100vh', tamanio }: Props) {
  return (
    <Center h={altura}>
      <Loader size={tamanio} />
    </Center>
  )
}
