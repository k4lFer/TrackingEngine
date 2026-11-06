export interface TrackingEvent {
  id: string
  vehicleId: string
  vehicleCode: string
  tripId: string | null
  type: string
  severity: string
  occurredAt: string
  geofenceName: string | null
  payloadJson: string | null
}

/** Respuesta de GET /events (agrupación por severidad/type del backend). */
export interface RecentEvents {
  events: TrackingEvent[]
  totalCount: number
}