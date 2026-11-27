import { ActionIcon, Center, Group, Loader, Paper, Stack, Table, Tooltip } from '@mantine/core'
import { IconFileText } from '@tabler/icons-react'
import { Link } from 'react-router-dom'
import { rutaHistoriaClinica } from '../../../routes/rutas'
import { formatearFecha } from '../../../shared/utils/fechas'
import { AccionesTabla } from '../../molecules/AccionesTabla/AccionesTabla'
import { BuscadorTexto } from '../../molecules/BuscadorTexto/BuscadorTexto'
import { PaginadorTabla } from '../../molecules/PaginadorTabla/PaginadorTabla'
import { EstadoPortalBadge } from '../../atoms/EstadoPortalBadge/EstadoPortalBadge'
import { EstadoVacio } from '../../atoms/EstadoVacio/EstadoVacio'
import type { PacienteDto } from '../../../domain/pacientes/types'

interface Props {
  pacientes: PacienteDto[]
  cargando: boolean
  filtro: string
  pagina: number
  totalPaginas: number
  onCambiarFiltro: (valor: string) => void
  onCambiarPagina: (pagina: number) => void
  onEditar: (paciente: PacienteDto) => void
  onDarDeBaja: (paciente: PacienteDto) => void
}

/**
 * Organism: tabla completa de pacientes con buscador y paginación.
 * Recibe datos y callbacks como props; no llama al repositorio directamente.
 */
export function TablaPacientes({
  pacientes,
  cargando,
  filtro,
  pagina,
  totalPaginas,
  onCambiarFiltro,
  onCambiarPagina,
  onEditar,
  onDarDeBaja,
}: Props) {
  return (
    <Paper withBorder p="md" radius="md">
      <Stack>
        <BuscadorTexto
          value={filtro}
          onChange={onCambiarFiltro}
          placeholder="Buscar por DNI, apellido o teléfono..."
        />

        {cargando ? (
          <Center py="xl">
            <Loader />
          </Center>
        ) : pacientes.length === 0 ? (
          <EstadoVacio mensaje="No se encontraron pacientes que coincidan con la búsqueda." />
        ) : (
          <Table.ScrollContainer minWidth={700}>
            <Table striped highlightOnHover verticalSpacing="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Apellido y Nombre</Table.Th>
                  <Table.Th>DNI</Table.Th>
                  <Table.Th>Teléfono</Table.Th>
                  <Table.Th>Nacimiento</Table.Th>
                  <Table.Th>Domicilio</Table.Th>
                  <Table.Th>Portal</Table.Th>
                  <Table.Th ta="right">Acciones</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {pacientes.map((p) => (
                  <Table.Tr key={p.id}>
                    <Table.Td fw={500}>
                      {p.apellido}, {p.nombre}
                    </Table.Td>
                    <Table.Td>{p.dni}</Table.Td>
                    <Table.Td>{p.telefono}</Table.Td>
                    <Table.Td>{formatearFecha(p.fechaNacimiento)}</Table.Td>
                    <Table.Td>{p.domicilio ?? '—'}</Table.Td>
                    <Table.Td>
                      <EstadoPortalBadge activo={p.tieneCuentaWeb} />
                    </Table.Td>
                    <Table.Td>
                      <Group gap="xs" justify="flex-end" wrap="nowrap">
                        <Tooltip label="Historia clínica">
                          <ActionIcon
                            component={Link}
                            to={rutaHistoriaClinica(p.id)}
                            variant="subtle"
                            color="teal"
                            aria-label="Historia clínica"
                          >
                            <IconFileText size={16} />
                          </ActionIcon>
                        </Tooltip>
                        <AccionesTabla
                          onEditar={() => onEditar(p)}
                          onEliminar={() => onDarDeBaja(p)}
                          etiquetaEditar="Editar paciente"
                          etiquetaEliminar="Dar de baja"
                        />
                      </Group>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        )}

        <PaginadorTabla
          pagina={pagina}
          totalPaginas={totalPaginas}
          onChange={onCambiarPagina}
        />
      </Stack>
    </Paper>
  )
}
