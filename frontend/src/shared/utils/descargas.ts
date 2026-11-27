// Ofrece al usuario guardar un archivo recibido como Blob.
export function guardarArchivo(contenido: Blob, nombre: string): void {
  const url = URL.createObjectURL(contenido)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombre
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()
  // Se libera después de que el navegador inició la descarga
  setTimeout(() => URL.revokeObjectURL(url), 1000)
}
