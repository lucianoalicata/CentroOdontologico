// Tipos genéricos compartidos por todos los módulos de dominio.

export interface ResultadoPaginado<T> {
  items: T[]
  totalItems: number
  pageIndex: number
  pageSize: number
  totalPages: number
}
