import type { ReactNode } from 'react'
import { Badge, Button, Group, Paper, Stack, Table, Text, Timeline } from '@mantine/core'
import { IconArrowBackUp, IconNotes } from '@tabler/icons-react'
import { formatearFechaHora } from '../../../shared/utils/fechas'
import { formatearPrecio } from '../../../shared/utils/moneda'
import { EstadoVacio } from '../../atoms/EstadoVacio/EstadoVacio'
import type { EntradaHistoriaClinicaDto } from '../../../domain/historiaClinica/types'

interface Props {
  entradas: EntradaHistoriaClinicaDto[]
  puedeCorregir: boolean
  onCorregir: (entrada: EntradaHistoriaClinicaDto) => void
  /** Contenido extra por entrada (por ejemplo, los adjuntos). */
  renderExtra?: (entrada: EntradaHistoriaClinicaDto) => ReactNode
}

/**
 * Organism: evolución cronológica de la historia clínica (de la más reciente a la más antigua).
 * Las entradas son inmutables: un error se corrige con una entrada nueva que la referencia.
 */
export function EvolucionHistoriaClinica({ entradas, puedeCorregir, onCorregir, renderExtra }: Props) {
  if (entradas.length === 0) {
    return (
      <Paper withBorder p="md" radius="md">
        <EstadoVacio mensaje="Todavía no hay entradas en la historia clínica." />
      </Paper>
    )
  }

  const corregidas = new Set(entradas.map((e) => e.corrigeEntradaId).filter((id) => id !== null))

  return (
    <Paper withBorder p="md" radius="md">
      <Timeline bulletSize={26} lineWidth={2}>
        {entradas.map((entrada) => (
          <Timeline.Item
            key={entrada.id}
            bullet={<IconNotes size={14} />}
            title={
              <Group gap="xs">
                <Text fw={600}>{formatearFechaHora(entrada.fechaCreacion)}</Text>
                <Text size="sm" c="dimmed">
                  {entrada.odontologoNombre}
                </Text>
                {entrada.corrigeEntradaId !== null && (
                  <Badge size="sm" color="orange" variant="light">
                    Corrige la entrada #{entrada.corrigeEntradaId}
                  </Badge>
                )}
                {corregidas.has(entrada.id) && (
                  <Badge size="sm" color="gray" variant="light">
                    Corregida
                  </Badge>
                )}
              </Group>
            }
          >
            <Stack gap="xs" mt={4} mb="md">
              <Text size="xs" c="dimmed">
                Entrada #{entrada.id}
              </Text>

              {entrada.comentario && (
                <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
                  {entrada.comentario}
                </Text>
              )}

              {entrada.prestaciones.length > 0 && (
                <Table withTableBorder verticalSpacing={4} fz="sm">
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Código</Table.Th>
                      <Table.Th>Prestación</Table.Th>
                      <Table.Th>Pieza</Table.Th>
                      <Table.Th ta="right">Cant.</Table.Th>
                      <Table.Th ta="right">Subtotal</Table.Th>
                      <Table.Th>Garantía</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {entrada.prestaciones.map((p) => (
                      <Table.Tr key={p.id}>
                        <Table.Td ff="monospace">{p.codigo}</Table.Td>
                        <Table.Td>{p.titulo}</Table.Td>
                        <Table.Td>{p.piezaDental ?? '—'}</Table.Td>
                        <Table.Td ta="right">{p.cantidad}</Table.Td>
                        <Table.Td ta="right">{formatearPrecio(p.subtotal)}</Table.Td>
                        <Table.Td>{p.mesesGarantia ? `${p.mesesGarantia} meses` : '—'}</Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              )}

              {renderExtra?.(entrada)}

              {puedeCorregir && (
                <Group>
                  <Button
                    size="xs"
                    variant="subtle"
                    color="orange"
                    leftSection={<IconArrowBackUp size={14} />}
                    onClick={() => onCorregir(entrada)}
                  >
                    Registrar corrección
                  </Button>
                </Group>
              )}
            </Stack>
          </Timeline.Item>
        ))}
      </Timeline>
    </Paper>
  )
}
