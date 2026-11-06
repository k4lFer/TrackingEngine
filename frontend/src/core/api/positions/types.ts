export interface PositionHistory {
  id: string
  vehicleId: string
  recordedAt: string
  lat: number
  lon: number
  speedKmh: number | null
  headingDeg: number | null
}

/** Entrada de reporte de posición (refleja PositionReportDto del backend). */
export interface PositionReportInput {
  vehicleId: string
  deviceId: string | null
  latitude: number
  longitude: number
  speedKmh: number | null
  headingDeg: number | null
  ignition: boolean | null
  odometerKm: number | null
  hdop: number | null
  satellites: number | null
  recordedAt: string
}

/** Respuesta de POST /positions/report. */
export interface PositionResponse {
  id: string
  vehicleId: string
  recordedAt: string
  lat: number
  lon: number
  speedKmh: number | null
  isValid: boolean
  rejectionReason: string | null
}