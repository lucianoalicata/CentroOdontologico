import { useState } from 'react'
import type { FormEvent } from 'react'
import {
  Alert,
  Button,
  Container,
  Paper,
  PasswordInput,
  Stack,
  Text,
  TextInput,
  Title,
} from '@mantine/core'
import { Navigate } from 'react-router-dom'
import { mensajeDeError } from '../../api/errores'
import { useAuth } from '../../auth/useAuth'
import { rutaInicial } from '../../routes/rutas'
import { PantallaCarga } from '../../shared/components/PantallaCarga'

export function LoginPage() {
  const { usuario, cargando, login } = useAuth()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  if (cargando) return <PantallaCarga />

  // Con sesión iniciada: siempre a la pantalla inicial de su rol
  if (usuario) {
    return <Navigate to={rutaInicial(usuario.roles)} replace />
  }

  async function enviar(evento: FormEvent) {
    evento.preventDefault()
    setError(null)
    setEnviando(true)

    try {
      await login({ email, password })
    } catch (e) {
      setError(mensajeDeError(e))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Container size={420} my={80}>
      <Title ta="center" c="teal">
        SIGO
      </Title>
      <Text c="dimmed" size="sm" ta="center" mt={5}>
        Sistema Integral de Gestión Odontológica
      </Text>

      <Paper withBorder shadow="md" p={30} mt={30} radius="md">
        <form onSubmit={enviar}>
          <Stack>
            <TextInput
              label="Correo electrónico"
              type="email"
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.currentTarget.value)}
              required
            />
            <PasswordInput
              label="Contraseña"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.currentTarget.value)}
              required
            />

            {error && (
              <Alert color="red" variant="light" role="alert">
                {error}
              </Alert>
            )}

            <Button type="submit" fullWidth loading={enviando}>
              Ingresar
            </Button>
          </Stack>
        </form>
      </Paper>
    </Container>
  )
}
