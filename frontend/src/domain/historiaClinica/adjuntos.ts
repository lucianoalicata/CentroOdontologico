// Tipos y reglas de los archivos adjuntos de una entrada de la historia clínica.
// Las reglas replican las del dominio (ArchivoAdjunto.cs) para avisar antes de subir.

export type TipoAdjunto = 'Imagen' | 'Documento'

export interface ArchivoAdjuntoDto {
  id: number
  entradaHistoriaClinicaId: number
  nombreOriginal: string
  tipoContenido: string
  tipo: TipoAdjunto
  tamanoBytes: number
  fechaCreacion: string // ISO UTC
}

export const TAMANO_MAXIMO_ADJUNTO_BYTES = 10 * 1024 * 1024

export const FORMATOS_ADJUNTO: Record<string, string> = {
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.png': 'image/png',
  '.pdf': 'application/pdf',
}

export const ACEPTA_ADJUNTOS = Object.keys(FORMATOS_ADJUNTO).join(',')

/** Devuelve el motivo por el que el archivo no se puede subir, o null si es válido. */
export function validarArchivoAdjunto(archivo: File): string | null {
  const punto = archivo.name.lastIndexOf('.')
  const extension = punto >= 0 ? archivo.name.slice(punto).toLowerCase() : ''
  const esperado = FORMATOS_ADJUNTO[extension]

  if (!esperado) return 'El formato del archivo no está permitido. Se aceptan JPG, PNG y PDF.'
  if (archivo.type && archivo.type.toLowerCase() !== esperado)
    return 'El tipo de contenido no coincide con la extensión del archivo.'
  if (archivo.size <= 0) return 'El archivo está vacío.'
  if (archivo.size > TAMANO_MAXIMO_ADJUNTO_BYTES)
    return `El archivo no puede superar los ${TAMANO_MAXIMO_ADJUNTO_BYTES / (1024 * 1024)} MB.`
  return null
}

export function formatearTamano(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
