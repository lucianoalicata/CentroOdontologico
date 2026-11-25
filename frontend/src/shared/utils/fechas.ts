import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);

type FechaEntrada = string | Date | null | undefined;

function parsearUtc(fechaUtc: FechaEntrada) {
  if (!fechaUtc) return null;
  const parsed = dayjs.utc(fechaUtc);
  return parsed.isValid() ? parsed.local() : null;
}

// Convierte una fecha UTC a formato local DD/MM/YYYY (ej. "25/11/2026").
export function formatearFecha(fechaUtc: FechaEntrada): string {
  return parsearUtc(fechaUtc)?.format('DD/MM/YYYY') ?? '-';
}

//Convierte una fecha y hora UTC a formato local DD/MM/YYYY HH:mm (ej. "25/11/2026 11:30").
export function formatearFechaHora(fechaUtc: FechaEntrada): string {
  return parsearUtc(fechaUtc)?.format('DD/MM/YYYY HH:mm') ?? '-';
}

//Extrae la hora local HH:mm de una marca temporal UTC (ej. "11:30").
export function formatearHora(fechaUtc: FechaEntrada): string {
  return parsearUtc(fechaUtc)?.format('HH:mm') ?? '-';
}