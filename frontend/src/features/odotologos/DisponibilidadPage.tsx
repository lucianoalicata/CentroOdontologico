import { useState } from 'react'
import { Paper, Select, Stack } from '@mantine/core'
import { useAuth } from '../../auth/useAuth'
import { useDisponibilidadOdontologo } from '../../application/horarios/useDisponibilidadOdontologo'
import { useOdontologos } from '../../application/odontologos/useOdontologos'
import type { FranjaHorariaDto } from '../../domain/horarios/types'
import { CabeceraPagina } from '../../ui/molecules/CabeceraPagina/CabeceraPagina'
import { FranjaHorariaModal } from '../../ui/organisms/Dsiponibilidad/FranjaHorariaModal'
import { TablaHorariosSemanales } from '../../ui/organisms/Dsiponibilidad/TablaHorariosSemanales'

export function DisponibilidadPage() {
  const { usuario } = useAuth()
  const esOdontologoPropio = Boolean(usuario?.odontologoId)

  const { odontologos } = useOdontologos()
  const [odontologoSeleccionado, setOdontologoSeleccionado] = useState<string | null>(null)

  const odontologoActivoId = esOdontologoPropio
    ? usuario!.odontologoId!
    : odontologoSeleccionado
      ? Number(odontologoSeleccionado)
      : odontologos[0]?.id ?? null

  const { franjas, cargando, guardando, guardarFranja, eliminarFranja } =
    useDisponibilidadOdontologo(odontologoActivoId)

  const [modalAbierto, setModalAbierto] = useState(false)
  const [franjaEditar, setFranjaEditar] = useState<FranjaHorariaDto | null>(null)

  function abrirNueva() {
    setFranjaEditar(null)
    setModalAbierto(true)
  }

  function abrirEditar(franja: FranjaHorariaDto) {
    setFranjaEditar(franja)
    setModalAbierto(true)
  }

  return (
    <Stack>
      <CabeceraPagina
        titulo="Disponibilidad horaria"
        descripcion="Configuración de días y franjas semanales de atención"
        etiquetaAccion="Agregar horario"
        onAccion={abrirNueva}
      />

      {!esOdontologoPropio && odontologos.length > 0 && (
        <Select
          label="Profesional"
          maw={360}
          value={odontologoActivoId ? String(odontologoActivoId) : null}
          onChange={setOdontologoSeleccionado}
          data={odontologos.map((o) => ({
            value: String(o.id),
            label: `${o.apellido}, ${o.nombre} (${o.especialidad})`,
          }))}
        />
      )}

      <Paper withBorder p="md" radius="md">
        <TablaHorariosSemanales
          franjas={franjas}
          cargando={cargando}
          onEditar={abrirEditar}
          onEliminar={(f) => eliminarFranja(f.id)}
        />
      </Paper>

      <FranjaHorariaModal
        key={`${modalAbierto}-${franjaEditar?.id ?? 'nueva'}`}
        abierto={modalAbierto}
        guardando={guardando}
        franjaEditar={franjaEditar}
        onCerrar={() => setModalAbierto(false)}
        onGuardar={guardarFranja}
      />
    </Stack>
  )
}