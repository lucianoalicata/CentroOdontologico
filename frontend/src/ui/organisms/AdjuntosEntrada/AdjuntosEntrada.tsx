import { ActionIcon, Button, FileButton, Group, Loader, Stack, Text, Tooltip } from '@mantine/core'
import { IconDownload, IconFileTypePdf, IconPaperclip, IconPhoto } from '@tabler/icons-react'
import { useAdjuntosEntrada } from '../../../application/historiaClinica/useAdjuntosEntrada'
import { ACEPTA_ADJUNTOS, formatearTamano } from '../../../domain/historiaClinica/adjuntos'
import { formatearFecha } from '../../../shared/utils/fechas'

interface Props {
  entradaId: number
  puedeSubir: boolean
}

/**
 * Organism: archivos adjuntos (radiografías, estudios) de una entrada de la historia clínica.
 * Permite subir JPG, PNG o PDF de hasta 10 MB y descargarlos.
 */
export function AdjuntosEntrada({ entradaId, puedeSubir }: Props) {
  const { adjuntos, cargando, subiendo, descargandoId, subir, descargar } =
    useAdjuntosEntrada(entradaId)

  if (cargando) return <Loader size="xs" />

  if (adjuntos.length === 0 && !puedeSubir) return null

  return (
    <Stack gap={4}>
      {adjuntos.map((a) => (
        <Group key={a.id} gap="xs" wrap="nowrap">
          {a.tipo === 'Imagen' ? <IconPhoto size={16} /> : <IconFileTypePdf size={16} />}
          <Text size="sm" truncate maw={320}>
            {a.nombreOriginal}
          </Text>
          <Text size="xs" c="dimmed">
            {formatearTamano(a.tamanoBytes)} · {formatearFecha(a.fechaCreacion)}
          </Text>
          <Tooltip label="Descargar">
            <ActionIcon
              variant="subtle"
              size="sm"
              aria-label={`Descargar ${a.nombreOriginal}`}
              loading={descargandoId === a.id}
              onClick={() => descargar(a)}
            >
              <IconDownload size={14} />
            </ActionIcon>
          </Tooltip>
        </Group>
      ))}

      {puedeSubir && (
        <Group>
          <FileButton onChange={subir} accept={ACEPTA_ADJUNTOS}>
            {(props) => (
              <Button
                {...props}
                size="xs"
                variant="subtle"
                leftSection={<IconPaperclip size={14} />}
                loading={subiendo}
              >
                Adjuntar archivo
              </Button>
            )}
          </FileButton>
          <Text size="xs" c="dimmed">
            JPG, PNG o PDF, hasta 10 MB
          </Text>
        </Group>
      )}
    </Stack>
  )
}
