import { Group, Pagination } from '@mantine/core'

interface Props {
  pagina: number
  totalPaginas: number
  onChange: (pagina: number) => void
}

/**
 * Molecule: paginador de tabla alineado a la derecha.
 * Solo se renderiza si hay más de una página.
 */
export function PaginadorTabla({ pagina, totalPaginas, onChange }: Props) {
  if (totalPaginas <= 1) return null

  return (
    <Group justify="flex-end" mt="xs">
      <Pagination value={pagina} onChange={onChange} total={totalPaginas} />
    </Group>
  )
}
