export interface TripSummary {
  id: string
  vehicleId: string
  vehicleCode: string
  routeId: string | null
  originGeofenceId: string | null
  destinationGeofenceId: string | null
  originName: string | null
  destinationName: string | null
  routeCode: string | null
  materialName: string | null
  loadTonnes: number | null
  startedAt: string
  endedAt: string | null
  distanceKm: number | null
  durationS: number | null
  status: string
}

export interface TripDetail extends TripSummary {
  routeName: string | null
  maxSpeedKmh: number | null
  deviationCount: number
  speedAlertCount: number
  trackGeoJson: string | null
}

/** Entrada de nuevo viaje (refleja CreateTripRequest del backend). */
export interface CreateTripInput {
  vehicleId: string
  originGeofenceId?: string | null
  destinationGeofenceId?: string | null
  routeId?: string | null
  materialId?: string | null
  loadTonnes?: number | null
}

/** Entrada de edición de un viaje en curso (refleja UpdateTripRequest del backend). */
export interface UpdateTripInput {
  materialId?: string | null
  loadTonnes?: number | null
}