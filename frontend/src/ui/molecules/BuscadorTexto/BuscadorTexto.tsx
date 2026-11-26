import { TextInput } from '@mantine/core'
import { IconSearch } from '@tabler/icons-react'

interface Props {
  value: string
  onChange: (valor: string) => void
  placeholder?: string
  maxAncho?: number
}

/**
 * Molecule: campo de búsqueda con ícono.
 * Encapsula el icono y la semántica de búsqueda.
 */
export function BuscadorTexto({
  value,
  onChange,
  placeholder = 'Buscar...',
  maxAncho = 420,
}: Props) {
  return (
    <TextInput
      placeholder={placeholder}
      leftSection={<IconSearch size={16} />}
      value={value}
      onChange={(e) => onChange(e.currentTarget.value)}
      maw={maxAncho}
    />
  )
}
