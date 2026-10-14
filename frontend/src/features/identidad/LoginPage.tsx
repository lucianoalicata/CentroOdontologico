import { useState } from 'react'
import type { FormEvent } from 'react'
import { Navigate } from 'react-router-dom'
import { mensajeDeError } from '../../api/errores'
import { useAuth } from '../../auth/useAuth'
import { rutaInicial } from '../../routes/rutas'

// Pantalla provisoria: la lógica es la definitiva, el estilo se agrega luego.
export function LoginPage() {
  const { usuario, cargando, login } = useAuth()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  if (cargando) return <p>Cargando…</p>

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
    <main>
      <h1>Iniciar sesión</h1>

      <form onSubmit={enviar}>
        <div>
          <label htmlFor="email">Correo electrónico</label>
          <input
            id="email"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div>
          <label htmlFor="password">Contraseña</label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        {error && <p role="alert">{error}</p>}

        <button type="submit" disabled={enviando}>
          {enviando ? 'Ingresando…' : 'Ingresar'}
        </button>
      </form>
    </main>
  )
}
