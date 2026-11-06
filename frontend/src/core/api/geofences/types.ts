import type { CoordinateDto } from '../common';

export interface Geofence {
  id: string
  code: string
  name: string
  kind: string
  priority: number
  geoJsonPolygon: string
  maxSpeedKmh: number | null
  color: string
  active: boolean
}

/** Entrada de alta (refleja CreateGeofenceRequest del backend). */
export interface CreateGeofenceInput {
  code: string
  name: string
  kind: string
  priority: number
  polygon: CoordinateDto[]
  maxSpeedKmh: number | null
  color: string | null
}

/** Entrada de edición (refleja UpdateGeofenceRequest del backend; sin code ni color). */
export interface UpdateGeofenceInput {
  name: string
  kind: string
  priority: number
  polygon: CoordinateDto[]
  maxSpeedKmh: number | null
  active: boolean
}