export const DEVICE_KIND_LABELS: Record<number, string> = {
  0: 'Genérico',
  1: 'Traccar',
  2: 'TK103',
  3: 'Teltonika',
  4: 'Queclink',
  5: 'SinoTrack',
}

export const deviceKindLabel = (kind: number | null | undefined): string =>
  kind == null ? '' : DEVICE_KIND_LABELS[kind] ?? String(kind)

export interface Vehicle {
  id: string
  code: string
  plate: string
  brand: string
  model: string
  active: boolean
  state: string
  deviceIdentifier?: string | null
  deviceKind?: number | null
  deviceModel?: string | null
}

/** Estado operativo con última posición (GET /vehicles/status). */
export interface VehicleStatus {
  id: string
  code: string
  plate: string
  state: string
  lastLat: number | null
  lastLon: number | null
  lastReportedAt: string | null
  activeTripId: string | null
  stoppedSince: string | null
  deviceIdentifier?: string | null
  deviceKind?: number | null
  deviceModel?: string | null
}

/** Entrada de alta (refleja CreateVehicleRequest del backend). */
export interface CreateVehicleInput {
  code: string
  plate: string
  brand: string
  model: string
  deviceIdentifier?: string | null
  deviceKind?: number | null
  deviceModel?: string | null
}

/** Entrada de edición (refleja UpdateVehicleRequest del backend; sin code). */
export interface UpdateVehicleInput {
  plate: string
  brand: string
  model: string
  active: boolean
  deviceIdentifier?: string | null
  deviceKind?: number | null
  deviceModel?: string | null
}