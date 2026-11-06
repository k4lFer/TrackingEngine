import type { CoordinateDto } from '../../../core/api/common'
import type { RouteItem } from '../../../core/api/routes'
import { parseGeoJsonLine } from '../../../core/format'
import type { PathEditorState } from '../../../components/PathEditor'

export const SEED_MAX_POINTS = 8

export interface RouteForm {
  code: string
  name: string
  toleranceM: number
  maxSpeedKmh: string
  alternativesCount: number
  active: boolean
}

/**
 * Reduce el trazado calculado (geometría densa de Valhalla, cientos de
 * vértices) a un puñado de via-points editables. El backend re-enruta desde
 * esos pocos waypoints, así que cargar toda la línea en el editor no sirve.
 */
export function sparseWaypoints(line: CoordinateDto[]): CoordinateDto[] {
  if (line.length <= SEED_MAX_POINTS) return line
  const step = (line.length - 1) / (SEED_MAX_POINTS - 1)
  const out: CoordinateDto[] = []
  for (let i = 0; i < SEED_MAX_POINTS; i++) {
    out.push(line[Math.round(i * step)])
  }
  return out
}

/**
 * Puntos con los que se siembra el editor al abrir: se usan los waypoints
 * reales persistidos (los que dibujó el usuario) para que la edición
 * reproduzca el mismo trazo; si la ruta es antigua y no los tiene, se cae a
 * una muestra escasa de la geometría calculada.
 */
export function pointsFrom(route: RouteItem | null | undefined): CoordinateDto[] {
  if (!route) return []
  if (route.waypoints != null && route.waypoints.length >= 2) return route.waypoints
  return sparseWaypoints(parseGeoJsonLine(route.geoJsonLine))
}

export function seedFrom(route: RouteItem | null | undefined): PathEditorState {
  return route
    ? {
        points: pointsFrom(route),
        startGeofenceId: route.originGeofenceId ?? '',
        endGeofenceId: route.destinationGeofenceId ?? '',
      }
    : { points: [], startGeofenceId: '', endGeofenceId: '' }
}

/** True si los puntos actuales siguen siendo exactamente los sembrados al abrir (sin editar). */
export function pointsEq(a: CoordinateDto[], b: CoordinateDto[]): boolean {
  return a.length === b.length && a.every((p, i) => p.lat === b[i].lat && p.lon === b[i].lon)
}

export function formFrom(route: RouteItem | null | undefined): RouteForm {
  return {
    code: route?.code ?? '',
    name: route?.name ?? '',
    toleranceM: route?.toleranceM ?? 50,
    maxSpeedKmh: route?.maxSpeedKmh != null ? String(route.maxSpeedKmh) : '',
    alternativesCount: route?.alternatives?.length ?? 2,
    active: route?.active ?? true,
  }
}
