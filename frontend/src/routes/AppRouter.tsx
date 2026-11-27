import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { LoginPage } from '../features/identidad/LoginPage'
import { PacientesPage } from '../features/identidad/PacientesPage'
import { PrestacionesPage } from '../features/historiaClinica/PrestacionesPage'
import { DisponibilidadPage } from '../features/odotologos/DisponibilidadPage'
import { LayoutPrincipal } from '../layouts/LayoutPrincipal'
import { NoEncontradoPage } from '../shared/pages/NoEncontradoPage'
import { PanelProvisorioPage } from '../shared/pages/PanelProvisorioPage'
import { ProhibidoPage } from '../shared/pages/ProhibidoPage'
import { Spinner } from '../ui/atoms/Spinner/Spinner'
import { RutaProtegida } from './RutaProtegida'
import { RUTAS, rutaInicial } from './rutas'

/** Redirige al usuario a su pantalla inicial según su sesión. */
function RedirigirSegunSesion() {
  const { usuario, cargando } = useAuth()

  if (cargando) return <Spinner />

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
            <Route path={RUTAS.prestaciones} element={<PrestacionesPage />} />
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

          <Route element={<RutaProtegida roles={['Secretaria', 'Odontologo', 'Administrador']} />}>
            <Route path={RUTAS.pacientes} element={<PacientesPage />} />
            <Route path={RUTAS.disponibilidad} element={<DisponibilidadPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<NoEncontradoPage />} />
    </Routes>
  )
}