import { useState, useEffect } from 'react'
import {
  Button,
  Group,
  Loader,
  Modal,
  SegmentedControl,
  Select,
  Stack,
  Text,
  TextInput,
} from '@mantine/core'
import { IconUserPlus } from '@tabler/icons-react'
import { useTurnoForm } from '../../../application/turnos/useTurnoForm'
import { useOdontologos } from '../../../application/odontologos/useOdontologos'
import { PacienteFormModal } from '../PacienteFormModal/PacienteFormModal'
import type { PacienteDto } from '../../../domain/pacientes/types'

interface Props {
  abierto: boolean
  onCerrar: () => void
  onGuardado: () => void
  odontologoInicialId?: number
  fechaInicial?: string
}

/**
 * Organism: modal para crear un turno programado o un entre-turno.
 * Orquesta useOdontologos + useTurnoForm (DIP, ISP).
 * El estado de formulario vive en los hooks, no aquí.
 */
export function TurnoFormModal({
  abierto,
  onCerrar,
  onGuardado,
  odontologoInicialId,
  fechaInicial,
}: Props) {
  const { odontologos } = useOdontologos(abierto)
  const [subModalPaciente, setSubModalPaciente] = useState(false)

  const turno = useTurnoForm({ abierto, odontologoInicialId, fechaInicial, onGuardado, onCerrar })

  // Auto-selecciona el primer odontólogo si aún no hay selección
  useEffect(() => {
    if (!turno.odontologoId && odontologos.length > 0) {
      turno.cambiarOdontologo(String(odontologos[0].id))
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [odontologos])

  function manejarPacienteCreado(nuevo: PacienteDto) {
    turno.agregarPacienteNuevo(nuevo)
    setSubModalPaciente(false)
  }

  return (
    <>
      <Modal opened={abierto} onClose={turno.limpiarYcerrar} title="Asignar turno" size="lg">
        <form
          onSubmit={(e) => {
            e.preventDefault()
            void turno.guardar()
          }}
        >
          <Stack>
            <SegmentedControl
              fullWidth
              value={turno.tipo}
              onChange={(val) => turno.cambiarTipo(val as 'programado' | 'entreturno')}
              data={[
                { label: 'Turno programado', value: 'programado' },
                { label: 'Entre-turno (sin ficha)', value: 'entreturno' },
              ]}
            />

            <Group grow>
              <Select
                label="Odontólogo"
                placeholder="Seleccionar profesional"
                required
                value={turno.odontologoId}
                onChange={turno.cambiarOdontologo}
                data={odontologos.map((o) => ({
                  value: String(o.id),
                  label: `${o.apellido}, ${o.nombre} (${o.especialidad})`,
                }))}
              />
              <TextInput
                label="Fecha"
                type="date"
                required
                value={turno.fecha}
                onChange={(e) => turno.cambiarFecha(e.currentTarget.value)}
              />
            </Group>

            {turno.tipo === 'programado' ? (
              <>
                <Group align="flex-end" wrap="nowrap">
                  <Select
                    style={{ flex: 1 }}
                    label="Paciente"
                    placeholder="Buscar por DNI, apellido o teléfono..."
                    searchable
                    searchValue={turno.busquedaPaciente}
                    onSearchChange={turno.setBusquedaPaciente}
                    value={turno.pacienteId}
                    onChange={turno.setPacienteId}
                    nothingFoundMessage="Sin coincidencias"
                    data={turno.opcionesPacientes.map((p) => ({
                      value: String(p.id),
                      label: `${p.apellido}, ${p.nombre} — DNI ${p.dni}`,
                    }))}
                  />
                  <Button
                    variant="light"
                    leftSection={<IconUserPlus size={16} />}
                    onClick={() => setSubModalPaciente(true)}
                  >
                    Nuevo paciente
                  </Button>
                </Group>

                <div>
                  <Text size="sm" fw={500} mb={6}>
                    Franjas horarias libres
                  </Text>
                  {turno.cargandoFranjas ? (
                    <Group gap="xs">
                      <Loader size="xs" />
                      <Text size="sm" c="dimmed">
                        Consultando disponibilidad...
                      </Text>
                    </Group>
                  ) : turno.franjas.length === 0 ? (
                    <Text size="sm" c="dimmed">
                      No hay franjas libres disponibles para esta fecha.
                    </Text>
                  ) : (
                    <Group gap="xs">
                      {turno.franjas.map((f) => (
                        <Button
                          key={f.horaInicio}
                          size="xs"
                          variant={
                            turno.franjaSeleccionada === f.horaInicio ? 'filled' : 'outline'
                          }
                          onClick={() => turno.setFranjaSeleccionada(f.horaInicio)}
                        >
                          {f.horaInicio} - {f.horaFin}
                        </Button>
                      ))}
                    </Group>
                  )}
                </div>
              </>
            ) : (
              <>
                <Group grow>
                  <TextInput
                    label="Nombre completo"
                    placeholder="Ej: Carlos Gómez"
                    required
                    value={turno.nombreEntreTurno}
                    onChange={(e) => turno.setNombreEntreTurno(e.currentTarget.value)}
                  />
                  <TextInput
                    label="Teléfono"
                    placeholder="Ej: 3815551234"
                    required
                    value={turno.telefonoEntreTurno}
                    onChange={(e) => turno.setTelefonoEntreTurno(e.currentTarget.value)}
                  />
                </Group>

                <Group grow>
                  <TextInput
                    label="DNI (opcional)"
                    placeholder="Sin puntos"
                    value={turno.dniEntreTurno}
                    onChange={(e) => turno.setDniEntreTurno(e.currentTarget.value)}
                  />
                  <TextInput
                    label="Hora estimada"
                    type="time"
                    required
                    value={turno.horaEntreTurno}
                    onChange={(e) => turno.setHoraEntreTurno(e.currentTarget.value)}
                  />
                </Group>
              </>
            )}

            <TextInput
              label="Motivo de consulta (opcional)"
              placeholder="Ej: Dolor agudo, control, limpieza..."
              value={turno.motivo}
              onChange={(e) => turno.setMotivo(e.currentTarget.value)}
            />

            <Group justify="flex-end" mt="md">
              <Button
                variant="default"
                onClick={turno.limpiarYcerrar}
                disabled={turno.guardando}
              >
                Cancelar
              </Button>
              <Button type="submit" loading={turno.guardando}>
                {turno.tipo === 'programado' ? 'Confirmar turno' : 'Registrar entre-turno'}
              </Button>
            </Group>
          </Stack>
        </form>
      </Modal>

      <PacienteFormModal
        abierto={subModalPaciente}
        onCerrar={() => setSubModalPaciente(false)}
        onGuardado={manejarPacienteCreado}
      />
    </>
  )
}
