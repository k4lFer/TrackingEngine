import { eventsApi, geofencesApi, routesApi, vehiclesApi } from '../api'
import { useApi } from './useApi'
import { usePolling } from './usePolling'
import type { Geofence } from '../api/geofences'
import type { RouteItem } from '../api/routes'
import type { VehicleStatus } from '../api/vehicles'

const POLL_MS = 10000

/**
 * Carga los datos del tablero.
 *
 * - `dataVersion`: cada vez que SignalR emite `dataChanged` / `vehicleMoved`
 *   el hook DEBE re-fetchear (el evento ya lleva la BD actualizada).
 * - `liveEnabled`: cuando venga del socket en live, el polling de 10s se
 *   DESACTIVA (respaldo); queda SOLO el disparo por evento SignalR.
 */
export function useDashboardData(dataVersion = 0, liveEnabled = false) {
  const { tick, refresh } = usePolling(POLL_MS, !liveEnabled)
  const deps = [tick, dataVersion] as const

  const vehicles = useApi<VehicleStatus[]>(() => vehiclesApi.status({ OnlyOnline: true }), [...deps])
  const geofences = useApi<Geofence[]>(() => geofencesApi.list(), [...deps])
  const routes = useApi<RouteItem[]>(() => routesApi.list(), [...deps])
  const events = useApi(() => eventsApi.recent(15), [...deps])

  return { vehicles, geofences, routes, events, refresh }
}