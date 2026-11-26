import { useState } from 'react'
import { Button, Group, Modal, NumberInput, Select, Stack, TextInput } from '@mantine/core'
import { DIAS_SEMANA } from '../../../domain/horarios/types'
import type {
  DiaSemana,
  FranjaHorariaDto,
  GuardarFranjaHorariaDto,
} from '../../../domain/horarios/types'

interface Props {
  abierto: boolean
  guardando: boolean
  franjaEditar: FranjaHorariaDto | null
  onCerrar: () => void
  onGuardar: (dto: GuardarFranjaHorariaDto, franjaId?: number) => Promise<boolean>
}

export function FranjaHorariaModal({
  abierto,
  guardando,
  franjaEditar,
  onCerrar,
  onGuardar,
}: Props) {
  const [diaSemana, setDiaSemana] = useState<string>(
    franjaEditar ? String(franjaEditar.diaSemana) : '1',
  )
  const [horaInicio, setHoraInicio] = useState(franjaEditar?.horaInicio ?? '09:00')
  const [horaFin, setHoraFin] = useState(franjaEditar?.horaFin ?? '13:00')
  const [duracion, setDuracion] = useState<number>(franjaEditar?.duracionTurnoMinutos ?? 30)

  async function manejarSubmit(e: React.FormEvent) {
    e.preventDefault()
    const exito = await onGuardar(
      {
        diaSemana: Number(diaSemana) as DiaSemana,
        horaInicio,
        horaFin,
        duracionTurnoMinutos: duracion,
      },
      franjaEditar?.id,
    )
    if (exito) onCerrar()
  }

  return (
    <Modal
      opened={abierto}
      onClose={onCerrar}
      title={franjaEditar ? 'Editar franja horaria' : 'Agregar franja horaria'}
      size="md"
    >
      <form onSubmit={manejarSubmit}>
        <Stack>
          <Select
            label="Día de la semana"
            required
            value={diaSemana}
            onChange={(val) => val && setDiaSemana(val)}
            data={DIAS_SEMANA.map((d) => ({
              value: String(d.valor),
              label: d.etiqueta,
            }))}
          />

          <Group grow>
            <TextInput
              label="Hora inicio"
              type="time"
              required
              value={horaInicio}
              onChange={(e) => setHoraInicio(e.currentTarget.value)}
            />
            <TextInput
              label="Hora fin"
              type="time"
              required
              value={horaFin}
              onChange={(e) => setHoraFin(e.currentTarget.value)}
            />
          </Group>

          <NumberInput
            label="Duración de cada turno (minutos)"
            min={10}
            max={120}
            step={5}
            required
            value={duracion}
            onChange={(val) => setDuracion(Number(val) || 30)}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onCerrar} disabled={guardando}>
              Cancelar
            </Button>
            <Button type="submit" loading={guardando}>
              {franjaEditar ? 'Guardar cambios' : 'Agregar horario'}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}   