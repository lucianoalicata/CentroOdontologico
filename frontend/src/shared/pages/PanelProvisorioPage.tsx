import { Badge, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { useAuth } from '../../auth/useAuth'

interface Props {
  titulo: string
}

// Reemplazada por el panel real de cada rol a medida que se construyan los módulos.
export function PanelProvisorioPage({ titulo }: Props) {
  const { usuario } = useAuth()

  if (!usuario) return null

  return (
    <Stack>
      <Title order={2}>{titulo}</Title>
      <Text c="dimmed">
        Bienvenido/a, {usuario.nombre}. Este panel se completará con los módulos del sistema.
      </Text>

      <Paper withBorder p="md" radius="md">
        <Stack gap="xs">
          <Text fw={600}>Datos de la sesión (solo durante el desarrollo)</Text>
          <Text size="sm">
            {usuario.nombre} {usuario.apellido} · {usuario.email}
          </Text>
          <Group gap="xs">
            {usuario.roles.map((rol) => (
              <Badge key={rol} variant="light">
                {rol}
              </Badge>
            ))}
          </Group>
          <Text size="sm" c="dimmed">
            Paciente: {usuario.pacienteId ?? '—'} · Odontólogo: {usuario.odontologoId ?? '—'} ·
            Secretaría: {usuario.secretariaId ?? '—'}
          </Text>
        </Stack>
      </Paper>
    </Stack>
  )
}
