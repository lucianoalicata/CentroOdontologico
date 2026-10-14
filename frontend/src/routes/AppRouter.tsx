import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { LoginPage } from '../features/identidad/LoginPage'
import { NoEncontradoPage } from '../shared/pages/NoEncontradoPage'
import { PanelProvisorioPage } from '../shared/pages/PanelProvisorioPage'
import { ProhibidoPage } from '../shared/pages/ProhibidoPage'
import { RutaProtegida } from './RutaProtegida'
import { RUTAS, rutaInicial } from './rutas'

// La raíz "/" manda a cada usuario a su pantalla inicial
function RedirigirSegunSesion() {
  const { usuario, cargando } = useAuth()

  if (cargando) return <p>Cargando…</p>

  return <Navigate to={usuario ? rutaInicial(usuario.roles) : RUTAS.login} replace />
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<RedirigirSegunSesion />} />
      <Route path={RUTAS.login} element={<LoginPage />} />
      <Route path={RUTAS.prohibido} element={<ProhibidoPage />} />

      <Route element={<RutaProtegida roles={['Administrador']} />}>
        <Route path={RUTAS.administrador} element={<PanelProvisorioPage titulo="Administración" />} />
      </Route>
      <Route element={<RutaProtegida roles={['Secretaria']} />}>
        <Route path={RUTAS.secretaria} element={<PanelProvisorioPage titulo="Secretaría" />} />
      </Route>
      <Route element={<RutaProtegida roles={['Odontologo']} />}>
        <Route path={RUTAS.odontologo} element={<PanelProvisorioPage titulo="Odontólogo" />} />
      </Route>
      <Route element={<RutaProtegida roles={['Paciente']} />}>
        <Route path={RUTAS.paciente} element={<PanelProvisorioPage titulo="Portal del paciente" />} />
      </Route>

      <Route path="*" element={<NoEncontradoPage />} />
    </Routes>
  )
}
