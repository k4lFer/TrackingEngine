import type { CoordinateDto } from '../common';

export interface RouteAlternativeItem {
  id: string
  code: string
  name: string
  alternativeRank: number
  geoJsonLine: string | null
  distanceKm: number
  estimatedDurationS: number
}

export interface RouteItem {
  id: string
  code: string
  name: string
  geoJsonLine: string
  waypoints: CoordinateDto[] | null
  toleranceM: number
  maxSpeedKmh: number | null
  active: boolean
  distanceKm: number
  estimatedDurationS: number
  originGeofenceId: string | null
  originGeofenceName: string | null
  destinationGeofenceId: string | null
  destinationGeofenceName: string | null
  provider?: string | null
  alternatives: RouteAlternativeItem[] | null
}

/** Entrada de alta (refleja CreateRouteRequest del backend). */
export interface CreateRouteInput {
  code: string
  name: string
  waypoints: CoordinateDto[]
  toleranceM: number
  maxSpeedKmh: number | null
  alternativesCount?: number
  originGeofenceId: string | null
  destinationGeofenceId: string | null
}

/** Entrada de edición (refleja UpdateRouteRequest del backend; sin code). */
export interface UpdateRouteInput {
  name: string
  waypoints: CoordinateDto[]
  toleranceM: number
  maxSpeedKmh: number | null
  alternativesCount?: number
  originGeofenceId: string | null
  destinationGeofenceId: string | null
  active: boolean
}

/** Entrada de cálculo de ruta sin persistir (POST /routes/preview). */
export interface RoutePreviewInput {
  waypoints: CoordinateDto[]
  toleranceM: number
  maxSpeedKmh: number | null
  alternativesCount?: number
}