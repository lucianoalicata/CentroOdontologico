import { useEffect, useState } from 'react'
import {
  ActionIcon,
  Badge,
  Button,
  Center,
  Group,
  Loader,
  Pagination,
  Paper,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
  Tooltip,
} from '@mantine/core'
import { useDebouncedValue } from '@mantine/hooks'
import { IconEdit, IconPlus, IconSearch, IconTrash } from '@tabler/icons-react'
import { avisarError, avisarExito } from '../../shared/utils/avisos'
import { formatearFecha } from '../../shared/utils/fechas'
import { PacienteFormModal } from './PacienteFormModal'
import { buscarPacientes, darDeBajaPaciente } from './pacientesApi'
import type { PacienteDto } from './pacientesApi'

export function PacientesPage() {
  const [pacientes, setPacientes] = useState<PacienteDto[]>([])
  const [cargando, setCargando] = useState(true)
  const [filtro, setFiltro] = useState('')
  const [filtroDebounced] = useDebouncedValue(filtro, 350)

  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [totalItems, setTotalItems] = useState(0)
  const [recarga, setRecarga] = useState(0)

  const [modalAbierto, setModalAbierto] = useState(false)
  const [pacienteEditar, setPacienteEditar] = useState<PacienteDto | null>(null)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      try {
        const res = await buscarPacientes(filtroDebounced, pagina, 10)
        if (cancelado) return
        setPacientes(res.items)
        setTotalPaginas(Math.max(1, res.totalPages))
        setTotalItems(res.totalItems)
      } catch (error) {
        if (!cancelado) avisarError(error)
      } finally {
        if (!cancelado) setCargando(false)
      }
    }

    cargar()

    return () => {
      cancelado = true
    }
  }, [filtroDebounced, pagina, recarga])

  function manejarCambioFiltro(valor: string) {
    setCargando(true)
    setFiltro(valor)
    setPagina(1)
  }

  function manejarCambioPagina(nuevaPagina: number) {
    setCargando(true)
    setPagina(nuevaPagina)
  }

  function forzarRecarga() {
    setCargando(true)
    setRecarga((n) => n + 1)
  }

  function abrirNuevo() {
    setPacienteEditar(null)
    setModalAbierto(true)
  }

  function abrirEditar(paciente: PacienteDto) {
    setPacienteEditar(paciente)
    setModalAbierto(true)
  }

  async function manejarBaja(paciente: PacienteDto) {
    const confirmado = window.confirm(
      `¿Confirma que desea dar de baja al paciente ${paciente.apellido}, ${paciente.nombre} (DNI ${paciente.dni})?`,
    )
    if (!confirmado) return

    try {
      await darDeBajaPaciente(paciente.id)
      avisarExito('Paciente dado de baja correctamente.')
      forzarRecarga()
    } catch (error) {
      avisarError(error)
    }
  }

  return (
    <Stack>
      <Group justify="space-between" align="center">
        <div>
          <Title order={2}>Pacientes</Title>
          <Text size="sm" c="dimmed">
            Padrón de pacientes registrados ({totalItems} en total)
          </Text>
        </div>
        <Button leftSection={<IconPlus size={16} />} onClick={abrirNuevo}>
          Nuevo paciente
        </Button>
      </Group>

      <Paper withBorder p="md" radius="md">
        <Stack>
          <TextInput
            placeholder="Buscar por DNI, apellido o teléfono..."
            leftSection={<IconSearch size={16} />}
            value={filtro}
            onChange={(e) => manejarCambioFiltro(e.currentTarget.value)}
            maw={420}
          />

          {cargando ? (
            <Center py="xl">
              <Loader />
            </Center>
          ) : pacientes.length === 0 ? (
            <Text c="dimmed" ta="center" py="xl">
              No se encontraron pacientes que coincidan con la búsqueda.
            </Text>
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
                        <Badge color={p.tieneCuentaWeb ? 'teal' : 'gray'} variant="light">
                          {p.tieneCuentaWeb ? 'Activo' : 'Sin cuenta'}
                        </Badge>
                      </Table.Td>
                      <Table.Td>
                        <Group gap="xs" justify="flex-end">
                          <Tooltip label="Editar">
                            <ActionIcon
                              variant="subtle"
                              color="blue"
                              aria-label="Editar paciente"
                              onClick={() => abrirEditar(p)}
                            >
                              <IconEdit size={16} />
                            </ActionIcon>
                          </Tooltip>
                          <Tooltip label="Dar de baja">
                            <ActionIcon
                              variant="subtle"
                              color="red"
                              aria-label="Dar de baja paciente"
                              onClick={() => manejarBaja(p)}
                            >
                              <IconTrash size={16} />
                            </ActionIcon>
                          </Tooltip>
                        </Group>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
          )}

          {totalPaginas > 1 && (
            <Group justify="flex-end" mt="xs">
              <Pagination value={pagina} onChange={manejarCambioPagina} total={totalPaginas} />
            </Group>
          )}
        </Stack>
      </Paper>

      <PacienteFormModal
        abierto={modalAbierto}
        onCerrar={() => setModalAbierto(false)}
        onGuardado={forzarRecarga}
        pacienteEditar={pacienteEditar}
      />
    </Stack>
  )
}