import { useState } from 'react'
import { Alert, Button, Group, Text, Textarea } from '@mantine/core'
import { IconAlertTriangle, IconEdit } from '@tabler/icons-react'
import { LONGITUD_MAXIMA_ALERTAS } from '../../../domain/historiaClinica/types'

interface Props {
  alertas: string | null
  puedeEditar: boolean
  guardando: boolean
  onGuardar: (alertas: string) => Promise<boolean>
}

/**
 * Organism: alertas médicas del paciente (alergias, medicación, enfermedades de base).
 * Se muestran destacadas al tope de la historia clínica.
 */
export function AlertasMedicasPanel({ alertas, puedeEditar, guardando, onGuardar }: Props) {
  const [editando, setEditando] = useState(false)
  const [texto, setTexto] = useState('')

  function empezarEdicion() {
    setTexto(alertas ?? '')
    setEditando(true)
  }

  async function guardar() {
    const ok = await onGuardar(texto)
    if (ok) setEditando(false)
  }

  const hayAlertas = Boolean(alertas)

  return (
    <Alert
      variant={hayAlertas ? 'filled' : 'light'}
      color={hayAlertas ? 'red' : 'gray'}
      title="Alertas médicas"
      icon={<IconAlertTriangle />}
    >
      {editando ? (
        <>
          <Textarea
            value={texto}
            onChange={(e) => setTexto(e.currentTarget.value)}
            placeholder="Alergias, medicación, enfermedades de base..."
            autosize
            minRows={2}
            maxLength={LONGITUD_MAXIMA_ALERTAS}
            description={`${texto.length}/${LONGITUD_MAXIMA_ALERTAS}`}
          />
          <Group justify="flex-end" mt="sm">
            <Button variant="white" color="gray" onClick={() => setEditando(false)} disabled={guardando}>
              Cancelar
            </Button>
            <Button variant="white" onClick={guardar} loading={guardando}>
              Guardar alertas
            </Button>
          </Group>
        </>
      ) : (
        <Group justify="space-between" align="flex-start" wrap="nowrap">
          <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
            {alertas ?? 'Sin alertas registradas.'}
          </Text>
          {puedeEditar && (
            <Button
              size="xs"
              variant={hayAlertas ? 'white' : 'light'}
              color={hayAlertas ? 'red' : 'gray'}
              leftSection={<IconEdit size={14} />}
              onClick={empezarEdicion}
            >
              Editar
            </Button>
          )}
        </Group>
      )}
    </Alert>
  )
}
