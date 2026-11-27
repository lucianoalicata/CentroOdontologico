import {
  ActionIcon,
  Badge,
  Center,
  Group,
  Loader,
  Paper,
  Select,
  Stack,
  Switch,
  Table,
  Tooltip,
} from '@mantine/core'
import { IconRestore } from '@tabler/icons-react'
import { formatearPrecio } from '../../../shared/utils/moneda'
import { AccionesTabla } from '../../molecules/AccionesTabla/AccionesTabla'
import { BuscadorTexto } from '../../molecules/BuscadorTexto/BuscadorTexto'
import { EstadoVacio } from '../../atoms/EstadoVacio/EstadoVacio'
import { CATEGORIAS_PRESTACION, ETIQUETAS_CATEGORIA } from '../../../domain/prestaciones/types'
import type { CategoriaPrestacion, PrestacionDto } from '../../../domain/prestaciones/types'

interface Props {
  prestaciones: PrestacionDto[]
  cargando: boolean
  filtro: string
  categoria: CategoriaPrestacion | null
  mostrarInactivas: boolean
  onCambiarFiltro: (valor: string) => void
  onCambiarCategoria: (categoria: CategoriaPrestacion | null) => void
  onCambiarMostrarInactivas: (valor: boolean) => void
  onEditar: (prestacion: PrestacionDto) => void
  onDarDeBaja: (prestacion: PrestacionDto) => void
  onReactivar: (prestacion: PrestacionDto) => void
}

const OPCIONES_CATEGORIA = CATEGORIAS_PRESTACION.map((c) => ({
  value: c,
  label: ETIQUETAS_CATEGORIA[c],
}))

/**
 * Organism: tabla del catálogo de prestaciones con buscador y filtros.
 * Recibe datos y callbacks por props; no llama al repositorio.
 */
export function TablaPrestaciones({
  prestaciones,
  cargando,
  filtro,
  categoria,
  mostrarInactivas,
  onCambiarFiltro,
  onCambiarCategoria,
  onCambiarMostrarInactivas,
  onEditar,
  onDarDeBaja,
  onReactivar,
}: Props) {
  return (
    <Paper withBorder p="md" radius="md">
      <Stack>
        <Group align="flex-end" wrap="wrap">
          <BuscadorTexto
            value={filtro}
            onChange={onCambiarFiltro}
            placeholder="Buscar por código o título..."
          />
          <Select
            placeholder="Todas las categorías"
            data={OPCIONES_CATEGORIA}
            value={categoria}
            onChange={(v) => onCambiarCategoria(v as CategoriaPrestacion | null)}
            clearable
            w={220}
          />
          <Switch
            label="Mostrar dadas de baja"
            checked={mostrarInactivas}
            onChange={(e) => onCambiarMostrarInactivas(e.currentTarget.checked)}
          />
        </Group>

        {cargando ? (
          <Center py="xl">
            <Loader />
          </Center>
        ) : prestaciones.length === 0 ? (
          <EstadoVacio mensaje="No se encontraron prestaciones que coincidan con la búsqueda." />
        ) : (
          <Table.ScrollContainer minWidth={760}>
            <Table striped highlightOnHover verticalSpacing="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Código</Table.Th>
                  <Table.Th>Título</Table.Th>
                  <Table.Th>Categoría</Table.Th>
                  <Table.Th ta="right">Duración</Table.Th>
                  <Table.Th ta="right">Precio particular</Table.Th>
                  <Table.Th>Autorización</Table.Th>
                  <Table.Th ta="right">Acciones</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {prestaciones.map((p) => (
                  <Table.Tr key={p.id} opacity={p.activo ? 1 : 0.55}>
                    <Table.Td ff="monospace">{p.codigo}</Table.Td>
                    <Table.Td fw={500}>
                      {p.titulo}
                      {!p.activo && (
                        <Badge ml="xs" size="xs" color="gray" variant="light">
                          Dada de baja
                        </Badge>
                      )}
                    </Table.Td>
                    <Table.Td>{ETIQUETAS_CATEGORIA[p.categoria] ?? p.categoria}</Table.Td>
                    <Table.Td ta="right">{p.duracionEstimadaMinutos} min</Table.Td>
                    <Table.Td ta="right">{formatearPrecio(p.precioParticular)}</Table.Td>
                    <Table.Td>
                      {p.requiereAutorizacionPrevia ? (
                        <Badge color="orange" variant="light">
                          Requiere
                        </Badge>
                      ) : (
                        <Badge color="gray" variant="light">
                          No
                        </Badge>
                      )}
                    </Table.Td>
                    <Table.Td>
                      {p.activo ? (
                        <AccionesTabla
                          onEditar={() => onEditar(p)}
                          onEliminar={() => onDarDeBaja(p)}
                          etiquetaEditar="Editar prestación"
                          etiquetaEliminar="Dar de baja"
                        />
                      ) : (
                        <Group justify="flex-end">
                          <Tooltip label="Reactivar">
                            <ActionIcon
                              variant="subtle"
                              color="teal"
                              aria-label="Reactivar"
                              onClick={() => onReactivar(p)}
                            >
                              <IconRestore size={16} />
                            </ActionIcon>
                          </Tooltip>
                        </Group>
                      )}
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        )}
      </Stack>
    </Paper>
  )
}
