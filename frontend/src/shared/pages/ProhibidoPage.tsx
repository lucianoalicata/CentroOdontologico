import { Link } from 'react-router-dom'

export function ProhibidoPage() {
  return (
    <main>
      <h1>Acceso denegado</h1>
      <p>No tiene permisos para ver esta sección.</p>
      <Link to="/">Volver al inicio</Link>
    </main>
  )
}
