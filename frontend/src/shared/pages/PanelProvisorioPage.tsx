import { useAuth } from '../../auth/useAuth'

interface Props {
  titulo: string
}

// Reemplazada por el panel real de cada rol a medida que se construyan los módulos.
export function PanelProvisorioPage({ titulo }: Props) {
  const { usuario, logout } = useAuth()

  if (!usuario) return null

  return (
    <main>
      <h1>{titulo}</h1>
      <p>
        Sesión iniciada como{' '}
        <strong>
          {usuario.nombre} {usuario.apellido}
        </strong>{' '}
        ({usuario.email})
      </p>
      <p>Roles: {usuario.roles.join(', ')}</p>
      <ul>
        <li>Paciente: {usuario.pacienteId ?? '—'}</li>
        <li>Odontólogo: {usuario.odontologoId ?? '—'}</li>
        <li>Secretaría: {usuario.secretariaId ?? '—'}</li>
      </ul>
      <button onClick={logout}>Cerrar sesión</button>
    </main>
  )
}
