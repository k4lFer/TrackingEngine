/** DTO en vivo recibido por SignalR en el evento 'vehicleMoved'. */
export interface VehicleLive {
  vehicleId: string
  vehicleCode: string
  tripId: string | null
  lat: number
  lon: number
  speedKmh: number | null
  headingDeg: number
  distanceKm: number
  progress: number
  etaSec: number
  loadTonnes: number | null
  materialName: string | null
  originName: string | null
  destinationName: string | null
  lastUpdated: string
}

/** DTO en vivo recibido por SignalR en el evento 'tripChanged'. */
export interface TripLive {
  tripId: string
  vehicleId: string
  vehicleCode: string
  status: string
  startedAt: string
  endedAt: string | null
  distanceKm: number | null
  durationS: number | null
  loadTonnes: number | null
  materialName: string | null
  originName: string | null
  destinationName: string | null
}