const FORMATO_PESOS = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'ARS',
  minimumFractionDigits: 2,
})

// Formatea un importe en pesos argentinos (ej. "$ 18.500,00").
export function formatearPrecio(valor: number | null | undefined): string {
  return valor === null || valor === undefined ? '-' : FORMATO_PESOS.format(valor)
}
