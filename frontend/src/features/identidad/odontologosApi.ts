// Mantiene compatibilidad con imports desde features/identidad/odontologosApi.
// La fuente de verdad está en domain/odontologos/types.ts e infrastructure/odontologos/.

export type { OdontologoActivoDto } from '../../domain/odontologos/types'

import { odontologosRepository } from '../../infrastructure/odontologos/odontologosRepository'

export const listarOdontologosActivos =
  odontologosRepository.listarActivos.bind(odontologosRepository)
