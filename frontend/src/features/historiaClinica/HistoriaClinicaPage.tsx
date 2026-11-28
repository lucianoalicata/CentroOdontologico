import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Anchor, Center, Group, Loader, Stack, Text } from '@mantine/core'
import { IconArrowLeft, IconPlus } from '@tabler/icons-react'
import { useAuth } from '../../auth/useAuth'
import { useHistoriaClinica } from '../../application/historiaClinica/useHistoriaClinica'
import { RUTAS } from '../../routes/rutas'
import { EstadoVacio } from '../../ui/atoms/EstadoVacio/EstadoVacio'
import { CabeceraPagina } from '../../ui/molecules/CabeceraPagina/CabeceraPagina'
import { AdjuntosEntrada } from '../../ui/organisms/AdjuntosEntrada/AdjuntosEntrada'
import { AlertasMedicasPanel } from '../../ui/organisms/AlertasMedicasPanel/AlertasMedicasPanel'
import { EvolucionHistoriaClinica } from '../../ui/organisms/EvolucionHistoriaClinica/EvolucionHistoriaClinica'
import { RegistrarEntradaModal } from '../../ui/organisms/RegistrarEntradaModal/RegistrarEntradaModal'
import type { EntradaHistoriaClinicaDto } from '../../domain/historiaClinica/types'

/**
 * Page: historia clínica de un paciente (ruta con el id del paciente).
 * Todo el personal puede verla; solo el odontólogo registra entradas y edita alertas.
 */
export function HistoriaClinicaPage() {
  const { pacienteId: parametro } = useParams()
  const pacienteId = Number(parametro)
  const { tieneRol } = useAuth()
  const esOdontologo = tieneRol('Odontologo')

  const { paciente, historia, cargando, guardandoAlertas, guardarAlertas, forzarRecarga } =
    useHistoriaClinica(pacienteId)

  const [modalAbierto, setModalAbierto] = useState(false)
  const [corrigeEntrada, setCorrigeEntrada] = useState<EntradaHistoriaClinicaDto | null>(null)

  function abrirNueva() {
    setCorrigeEntrada(null)
    setModalAbierto(true)
  }

  function abrirCorreccion(entrada: EntradaHistoriaClinicaDto) {
    setCorrigeEntrada(entrada)
    setModalAbierto(true)
  }

  const volver = (
    <Anchor component={Link} to={RUTAS.pacientes} size="sm">
      <Group gap={4}>
        <IconArrowLeft size={14} />
        Volver a pacientes
      </Group>
    </Anchor>
  )

  if (cargando) {
    return (
      <Center py="xl">
        <Loader />
      </Center>
    )
  }

  if (!paciente || !historia) {
    return (
      <Stack>
        {volver}
        <EstadoVacio mensaje="No se pudo cargar la historia clínica del paciente." />
      </Stack>
    )
  }

  return (
    <Stack>
      {volver}

      <CabeceraPagina
        titulo={`Historia clínica: ${paciente.apellido}, ${paciente.nombre}`}
        descripcion={`DNI ${paciente.dni} · ${historia.entradas.length} entradas`}
        etiquetaAccion={esOdontologo ? 'Nueva entrada' : undefined}
        iconoAccion={<IconPlus size={16} />}
        onAccion={esOdontologo ? abrirNueva : undefined}
      />

      <AlertasMedicasPanel
        alertas={historia.alertasMedicas}
        puedeEditar={esOdontologo}
        guardando={guardandoAlertas}
        onGuardar={guardarAlertas}
      />

      <Text fw={600}>Evolución</Text>
      <EvolucionHistoriaClinica
        entradas={historia.entradas}
        puedeCorregir={esOdontologo}
        onCorregir={abrirCorreccion}
        renderExtra={(entrada) => (
          <AdjuntosEntrada entradaId={entrada.id} puedeSubir={esOdontologo} />
        )}
      />

      {esOdontologo && (
        <RegistrarEntradaModal
          abierto={modalAbierto}
          pacienteId={pacienteId}
          corrigeEntrada={corrigeEntrada}
          onCerrar={() => setModalAbierto(false)}
          onRegistrada={forzarRecarga}
        />
      )}
    </Stack>
  )
}
