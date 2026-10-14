import { Button, Container, Stack, Text, Title } from '@mantine/core'
import { Link } from 'react-router-dom'

export function NoEncontradoPage() {
  return (
    <Container size="xs" py={60}>
      <Stack align="center" gap="xs">
        <Title order={2}>Página no encontrada</Title>
        <Text c="dimmed" ta="center">
          La dirección ingresada no existe.
        </Text>
        <Button component={Link} to="/" mt="md">
          Volver al inicio
        </Button>
      </Stack>
    </Container>
  )
}
