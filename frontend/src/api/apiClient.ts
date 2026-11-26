// Mantiene compatibilidad con imports desde api/apiClient.
// La fuente de verdad es infrastructure/http/apiClient.ts

export {
  apiClient,
  registrarAlNoAutorizado,
} from '../infrastructure/http/apiClient'
