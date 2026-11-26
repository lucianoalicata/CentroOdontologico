import { notifications } from '@mantine/notifications'
import { parsearError } from '../../infrastructure/http/errorParser'

/**
 * Muestra una notificación de éxito usando Mantine Notifications.
 */
export function avisarExito(mensaje: string, titulo = 'Operación exitosa'): void {
  notifications.show({
    title: titulo,
    message: mensaje,
    color: 'teal',
  })
}

/**
 * Muestra una notificación de error.
 * Delega la extracción del mensaje a errorParser (SRP, DRY).
 */
export function avisarError(error: unknown, titulo = 'Error'): void {
  notifications.show({
    title: titulo,
    message: parsearError(error),
    color: 'red',
  })
}
