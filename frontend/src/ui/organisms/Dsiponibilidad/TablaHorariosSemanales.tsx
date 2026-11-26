import { Badge, Table } from '@mantine/core'
import { DIAS_SEMANA } from '../../../domain/horarios/types'
import type { FranjaHorariaDto } from '../../../domain/horarios/types'
import { EstadoVacio } from '../../atoms/EstadoVacio/EstadoVacio'
import { Spinner } from '../../atoms/Spinner/Spinner'
import { AccionesTabla } from '../../molecules/AccionesTabla/AccionesTabla'

interface Props {
  franjas: FranjaHorariaDto[]
  cargando: boolean
  onEditar: (franja: FranjaHorariaDto) => void
  onEliminar: (franja: FranjaHorariaDto) => void
}

function nombreDia(dia: number): string {
  return DIAS_SEMANA.find((d) => d.valor === dia)?.etiqueta ?? `Día ${dia}`
}

export function TablaHorariosSemanales({
  franjas,
  cargando,
  onEditar,
  onEliminar,
}: Props) {
  if (cargando) return <Spinner />

  if (franjas.length === 0) {
    return (
      <EstadoVacio mensaje="No hay horarios semanales configurados para este profesional." />
    )
  }

  return (
    <Table.ScrollContainer minWidth={550}>
      <Table striped highlightOnHover verticalSpacing="sm">
        <Table.Thead>
          <Table.Tr>
            <Table.Th>Día</Table.Th>
            <Table.Th>Horario de atención</Table.Th>
            <Table.Th>Duración por turno</Table.Th>
            <Table.Th ta="right">Acciones</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {franjas.map((f) => (
            <Table.Tr key={f.id}>
              <Table.Td fw={500}>{nombreDia(f.diaSemana)}</Table.Td>
              <Table.Td>
                {f.horaInicio} — {f.horaFin}
              </Table.Td>
              <Table.Td>
                <Badge variant="light" color="blue">
                  {f.duracionTurnoMinutos} min
                </Badge>
              </Table.Td>
              <Table.Td>
                <AccionesTabla
                  onEditar={() => onEditar(f)}
                  onEliminar={() => onEliminar(f)}
                  etiquetaEliminar="Eliminar horario"
                />
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  )
}