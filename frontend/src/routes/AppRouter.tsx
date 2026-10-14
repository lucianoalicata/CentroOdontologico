import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { LoginPage } from '../features/identidad/LoginPage'
import { LayoutPrincipal } from '../layouts/LayoutPrincipal'
import { PantallaCarga } from '../shared/components/PantallaCarga'
import { NoEncontradoPage } from '../shared/pages/NoEncontradoPage'
import { PanelProvisorioPage } from '../shared/pages/PanelProvisorioPage'
import { ProhibidoPage } from '../shared/pages/ProhibidoPage'
import { RutaProtegida } from './RutaProtegida'
import { RUTAS, rutaInicial } from './rutas'

// La raíz "/" manda a cada usuario a su pantalla inicial
function RedirigirSegunSesion() {
  const { usuario, cargando } = useAuth()

  if (cargando) return <PantallaCarga />

  return <Navigate to={usuario ? rutaInicial(usuario.roles) : RUTAS.login} replace />
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<RedirigirSegunSesion />} />
      <Route path={RUTAS.login} element={<LoginPage />} />

      {/* Zona autenticada: exige sesión y muestra el layout principal.
          Cada módulo agrega sus rutas dentro del bloque del rol que corresponda. */}
      <Route element={<RutaProtegida />}>
        <Route element={<LayoutPrincipal />}>
          <Route path={RUTAS.prohibido} element={<ProhibidoPage />} />

          <Route element={<RutaProtegida roles={['Administrador']} />}>
            <Route
              path={RUTAS.administrador}
              element={<PanelProvisorioPage titulo="Administración" />}
            />
          </Route>

          <Route element={<RutaProtegida roles={['Secretaria']} />}>
            <Route path={RUTAS.secretaria} element={<PanelProvisorioPage titulo="Recepción" />} />
          </Route>

          <Route element={<RutaProtegida roles={['Odontologo']} />}>
            <Route path={RUTAS.odontologo} element={<PanelProvisorioPage titulo="Mi agenda" />} />
          </Route>

          <Route element={<RutaProtegida roles={['Paciente']} />}>
            <Route
              path={RUTAS.paciente}
              element={<PanelProvisorioPage titulo="Portal del paciente" />}
            />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<NoEncontradoPage />} />
    </Routes>
  )
}
