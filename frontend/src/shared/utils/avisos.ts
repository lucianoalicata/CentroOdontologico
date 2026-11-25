import { notifications } from '@mantine/notifications';

interface ErrorApi {
  response?: {
    data?: {
      mensaje?: string;
      message?: string;
      title?: string;
      detail?: string;
    };
  };
  message?: string;
}

function extraerMensajeError(error: unknown): string {
  if (typeof error === 'string' && error.trim().length > 0) {
    return error;
  }

  if (error && typeof error === 'object') {
    const err = error as ErrorApi;
    const data = err.response?.data;

    if (data?.mensaje) return data.mensaje;
    if (data?.message) return data.message;
    if (data?.detail) return data.detail;
    if (data?.title) return data.title;
    if (err.message) return err.message;
  }

  return 'Ocurrió un error inesperado.';
}

//Muestra una notificación de éxito usando Mantine Notifications.
export function avisarExito(mensaje: string, titulo = 'Operación exitosa'): void {
  notifications.show({
    title: titulo,
    message: mensaje,
    color: 'teal',
  });
}

//Muestra una notificación de error extrayendo el mensaje del backend (Axios) o de JS.
export function avisarError(error: unknown, titulo = 'Error'): void {
  notifications.show({
    title: titulo,
    message: extraerMensajeError(error),
    color: 'red',
  });
}