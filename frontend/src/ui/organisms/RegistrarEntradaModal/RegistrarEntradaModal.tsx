import {
  ActionIcon,
  Alert,
  Button,
  Group,
  Modal,
  NumberInput,
  Paper,
  Select,
  Stack,
  Text,
  Textarea,
} from '@mantine/core'
import { IconTrash } from '@tabler/icons-react'
import { useRegistrarEntrada } from '../../../application/historiaClinica/useRegistrarEntrada'
import { usePrestacionesActivas } from '../../../application/prestaciones/usePrestacionesActivas'
import { LONGITUD_MAXIMA_COMENTARIO } from '../../../domain/historiaClinica/types'
import { formatearFechaHora } from '../../../shared/utils/fechas'
import { formatearPrecio } from '../../../shared/utils/moneda'
import type { EntradaHistoriaClinicaDto } from '../../../domain/historiaClinica/types'

interface Props {
  abierto: boolean
  pacienteId: number
  corrigeEntrada?: EntradaHistoriaClinicaDto | null
  onCerrar: () => void
  onRegistrada: (entrada: EntradaHistoriaClinicaDto) => void
}

/**
 * Organism: alta de una entrada de la historia clínica con prestaciones del catálogo
 * (búsqueda por código y título) y comentario de texto libre.
 */
export function RegistrarEntradaModal({
  abierto,
  pacienteId,
  corrigeEntrada,
  onCerrar,
  onRegistrada,
}: Props) {
  const { prestaciones, cargando } = usePrestacionesActivas(abierto)
  const { form, guardando, total, agregarPrestacion, quitarPrestacion, guardar } =
    useRegistrarEntrada({ abierto, pacienteId, corrigeEntrada, onRegistrada, onCerrar })

  const opciones = prestaciones.map((p) => ({
    value: String(p.id),
    label: `${p.codigo} - ${p.titulo}`,
  }))

  function alElegir(valor: string | null) {
    const elegida = prestaciones.find((p) => String(p.id) === valor)
    if (elegida) agregarPrestacion(elegida)
  }

  return (
    <Modal
      opened={abierto}
      onClose={onCerrar}
      title={corrigeEntrada ? 'Registrar corrección' : 'Nueva entrada'}
      size="xl"
    >
      <form onSubmit={form.onSubmit(guardar)}>
        <Stack>
          {corrigeEntrada && (
            <Alert color="orange" variant="light">
              Esta entrada corrige la #{corrigeEntrada.id} del{' '}
              {formatearFechaHora(corrigeEntrada.fechaCreacion)}. La entrada original no se modifica.
            </Alert>
          )}

          <Select
            label="Agregar prestación realizada"
            placeholder={cargando ? 'Cargando catálogo...' : 'Buscar por código o título'}
            data={opciones}
            value={null}
            onChange={alElegir}
            searchable
            nothingFoundMessage="No hay prestaciones que coincidan"
            disabled={cargando}
          />

          {form.values.lineas.length > 0 && (
            <Stack gap="xs">
              {form.values.lineas.map((linea, i) => (
                <Paper key={linea.clave} withBorder p="xs" radius="sm">
                  <Group align="flex-start" wrap="nowrap">
                    <Stack gap={0} style={{ flex: 1 }}>
                      <Text size="sm" fw={500}>
                        {linea.prestacion.codigo} - {linea.prestacion.titulo}
                      </Text>
                      <Text size="xs" c="dimmed">
                        {formatearPrecio(linea.prestacion.precioParticular)} c/u
                        {linea.prestacion.requiereAutorizacionPrevia &&
                          ' · requiere autorización previa'}
                      </Text>
                    </Stack>
                    <NumberInput
                      label="Cant."
                      w={80}
                      min={1}
                      allowDecimal={false}
                      {...form.getInputProps(`lineas.${i}.cantidad`)}
                    />
                    <NumberInput
                      label="Pieza (FDI)"
                      placeholder="Opcional"
                      w={110}
                      allowDecimal={false}
                      {...form.getInputProps(`lineas.${i}.piezaDental`)}
                    />
                    <NumberInput
                      label="Garantía (meses)"
                      placeholder="Opcional"
                      w={130}
                      min={1}
                      allowDecimal={false}
                      {...form.getInputProps(`lineas.${i}.mesesGarantia`)}
                    />
                    <ActionIcon
                      mt={26}
                      variant="subtle"
                      color="red"
                      aria-label="Quitar prestación"
                      onClick={() => quitarPrestacion(i)}
                    >
                      <IconTrash size={16} />
                    </ActionIcon>
                  </Group>
                </Paper>
              ))}
              <Text size="sm" ta="right" fw={500}>
                Total estimado: {formatearPrecio(total)}
              </Text>
            </Stack>
          )}

          <Textarea
            label="Comentario"
            placeholder="Motivo de consulta, hallazgos, indicaciones..."
            autosize
            minRows={4}
            maxLength={LONGITUD_MAXIMA_COMENTARIO}
            {...form.getInputProps('comentario')}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onCerrar} disabled={guardando}>
              Cancelar
            </Button>
            <Button type="submit" loading={guardando}>
              Registrar entrada
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
