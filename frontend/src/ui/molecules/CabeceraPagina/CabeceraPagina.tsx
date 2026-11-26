import { Button, Group, Text, Title } from '@mantine/core'
import type { ReactNode } from 'react'

interface Props {
  titulo: string
  descripcion?: string
  etiquetaAccion?: string
  iconoAccion?: ReactNode
  onAccion?: () => void
}

/**
 * Molecule: cabecera de página con título, descripción y botón de acción principal.
 * Reutilizable en cualquier listado con acción de creación.
 */
export function CabeceraPagina({
  titulo,
  descripcion,
  etiquetaAccion,
  iconoAccion,
  onAccion,
}: Props) {
  return (
    <Group justify="space-between" align="center">
      <div>
        <Title order={2}>{titulo}</Title>
        {descripcion && (
          <Text size="sm" c="dimmed">
            {descripcion}
          </Text>
        )}
      </div>
      {etiquetaAccion && onAccion && (
        <Button leftSection={iconoAccion} onClick={onAccion}>
          {etiquetaAccion}
        </Button>
      )}
    </Group>
  )
}
