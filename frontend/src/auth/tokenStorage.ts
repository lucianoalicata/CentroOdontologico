// Único lugar de la aplicación que lee o escribe el token en el almacenamiento del navegador.

const CLAVE = 'sigo.sesion'

interface SesionGuardada {
  token: string
  expiraEn: string
}

export function guardarSesion(token: string, expiraEn: string): void {
  const sesion: SesionGuardada = { token, expiraEn }
  localStorage.setItem(CLAVE, JSON.stringify(sesion))
}

// Devuelve null si no hay sesión, si está dañada o si el token ya venció.
export function leerSesion(): SesionGuardada | null {
  const crudo = localStorage.getItem(CLAVE)
  if (!crudo) return null

  try {
    const sesion = JSON.parse(crudo) as SesionGuardada
    if (new Date(sesion.expiraEn).getTime() <= Date.now()) {
      borrarSesion()
      return null
    }
    return sesion
  } catch {
    borrarSesion()
    return null
  }
}

export function borrarSesion(): void {
  localStorage.removeItem(CLAVE)
}