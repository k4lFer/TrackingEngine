import type { CoordinateDto } from '../common';

export interface RoadItem {
  id: string
  code: string
  name: string
  geoJsonLine: string
  maxSpeedKmh: number | null
  active: boolean
  distanceKm: number
}

/** Entrada de alta (refleja CreateRoadDto del backend). */
export interface CreateRoadInput {
  code: string
  name: string
  waypoints: CoordinateDto[]
  maxSpeedKmh: number | null
}