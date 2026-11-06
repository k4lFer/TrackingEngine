import * as polyline from '@mapbox/polyline'
import type { CoordinateDto } from './api/common'

export function parseGeoJsonPolygon(geoJson: string): CoordinateDto[] {
  try {
    const geom = JSON.parse(geoJson)
    const coords = geom?.coordinates?.[0] ?? geom?.coordinates ?? []
    return coords.map((c: number[]) => ({ lat: c[1], lon: c[0] }))
  } catch {
    return []
  }
}

export function parseGeoJsonLine(geoJson: string): CoordinateDto[] {
  try {
    const geom = JSON.parse(geoJson)
    const coords = geom?.coordinates ?? []
    return coords.map((c: number[]) => ({ lat: c[1], lon: c[0] }))
  } catch {
    // Valhalla devuelve el shape como Polyline6 codificado (encoded polyline,
    // precision 6). Si no es GeoJSON, decodificamos esa geometría para que la
    // ruta siga calles reales en vez de trazarse como una línea recta.
    return polyline.decode(geoJson, 6).map(([lat, lon]) => ({ lat, lon }))
  }
}

export function polygonCentroid(ring: CoordinateDto[]): CoordinateDto | null {
  if (ring.length === 0) return null
  const lat = ring.reduce((sum, p) => sum + p.lat, 0) / ring.length
  const lon = ring.reduce((sum, p) => sum + p.lon, 0) / ring.length
  return { lat, lon }
}

export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return '—'
  return d.toLocaleString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatDuration(seconds: number | null | undefined): string {
  if (seconds == null) return '—'
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = Math.floor(seconds % 60)
  if (h > 0) return `${h}h ${String(m).padStart(2, '0')}m`
  if (m > 0) return `${m}m ${String(s).padStart(2, '0')}s`
  return `${s}s`
}

export function formatDistance(km: number | null | undefined): string {
  if (km == null) return '—'
  if (km < 10) return `${km.toFixed(2)} km`
  return `${km.toFixed(1)} km`
}

export function timeAgo(iso: string | null | undefined): string {
  if (!iso) return 'sin datos'
  const diff = Date.now() - new Date(iso).getTime()
  if (diff < 0) return 'ahora'
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'hace <1 min'
  if (mins < 60) return `hace ${mins} min`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `hace ${hours} h`
  const days = Math.floor(hours / 24)
  return `hace ${days} d`
}

export const STATE_LABELS: Record<string, string> = {
  Moving: 'En movimiento',
  Idle: 'Detenido',
  Stopped: 'Detenido',
  Off: 'Apagado',
  Unknown: 'Desconocido',
  EnRuta: 'En ruta',
  EnCarga: 'En carga',
  EnDescarga: 'En descarga',
  Detenido: 'Detenido',
  Disponible: 'Disponible',
  Offline: 'Sin señal',
  FueraDeServicio: 'Fuera de servicio',
}

export const EVENT_LABELS: Record<string, string> = {
  GeofenceEntered: 'Ingreso a zona',
  GeofenceExited: 'Salida de zona',
  LongStopDetected: 'Parada larga',
  LongStopEnded: 'Fin de parada',
  TripStarted: 'Viaje iniciado',
  TripEnded: 'Viaje finalizado',
  SpeedLimitExceeded: 'Exceso de velocidad',
  SpeedLimitCleared: 'Fin de exceso de velocidad',
  RouteDeviationDetected: 'Desvío de ruta',
  RouteDeviationCleared: 'Fin de desvío',
  GpsConnectionLost: 'Señal perdida',
  GpsConnectionRestored: 'Señal recuperada',
}