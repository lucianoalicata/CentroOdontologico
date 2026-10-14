import { Button, Container, Stack, Text, Title } from '@mantine/core'
import { Link } from 'react-router-dom'

export function ProhibidoPage() {
  return (
    <Container size="xs" py={60}>
      <Stack align="center" gap="xs">
        <Title order={2}>Acceso denegado</Title>
        <Text c="dimmed" ta="center">
          No tiene permisos para ver esta sección.
        </Text>
        <Button component={Link} to="/" mt="md">
          Volver al inicio
        </Button>
      </Stack>
    </Container>
  )
}
