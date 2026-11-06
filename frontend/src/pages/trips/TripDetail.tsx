import { useNavigate, useParams } from 'react-router-dom'
import { geofencesApi, routesApi, tripsApi } from '../../core/api'
import { useApi } from '../../core/hooks/useApi'
import { useSignalR } from '../../core/hooks/useSignalR'
import type { Geofence } from '../../core/api/geofences'
import type { PositionHistory } from '../../core/api/positions'
import type { RouteItem } from '../../core/api/routes'
import type { TripDetail } from '../../core/api/trips'
import {
  formatDistance,
  formatDuration,
  parseGeoJsonPolygon,
  polygonCentroid,
} from '../../core/format'
import Stats from '../../components/ui/Stats'
import TripHeader from './components/TripHeader'
import LiveProgress from './components/LiveProgress'
import TripMapCard from './components/TripMapCard'
import TripDetailsCard from './components/TripDetailsCard'

export default function TripDetail() {
  const { id } = useParams()
  const tripId = id ?? ''
  const navigate = useNavigate()

  const { vehicles: live } = useSignalR()
  const trip = useApi(() => tripsApi.getById(tripId), [tripId])
  const track = useApi<PositionHistory[]>(() => tripsApi.track(tripId), [tripId])
  const geofences = useApi<Geofence[]>(() => geofencesApi.list(), [])
  const routes = useApi<RouteItem[]>(() => routesApi.list(), [])
  const detail = trip.data as TripDetail | null
  const latLngs: [number, number][] = (track.data ?? []).map((p): [number, number] => [p.lat, p.lon])

  const liveVehicle = detail ? live[detail.vehicleId] : undefined
  const isActive = detail?.status === 'Active'
  const progress = liveVehicle ? Math.round(liveVehicle.progress * 100) : null

  const routeItem = (routes.data ?? []).find((r) => r.code === detail?.routeCode) ?? null
  const geofenceList = geofences.data ?? []
  const originGeofence = detail?.originName ? geofenceList.find((g) => g.name === detail.originName) ?? null : null
  const destGeofence = detail?.destinationName ? geofenceList.find((g) => g.name === detail.destinationName) ?? null : null
  const vehiclePos: [number, number] | null =
    liveVehicle && isActive
      ? [liveVehicle.lat, liveVehicle.lon]
      : latLngs.length > 0
        ? latLngs[latLngs.length - 1]
        : null
  const origin = centroid(originGeofence) ?? (latLngs.length > 0 ? latLngs[0] : null)
  const destination = centroid(destGeofence) ?? (latLngs.length > 0 ? latLngs[latLngs.length - 1] : null)

  return (
    <div>
      <TripHeader tripId={tripId} isActive={isActive} onBack={() => navigate('/trips')} />

      {trip.error && <div className="error-box">{trip.error}</div>}

      {detail && (
        <Stats
          items={[
            { label: 'Distancia recorrida', value: formatDistance(liveVehicle?.distanceKm ?? detail.distanceKm) },
            { label: 'Duración', value: formatDuration(detail.durationS) },
            { label: 'Material', value: detail.materialName ?? '—' },
            { label: 'Carga', value: detail.loadTonnes != null ? `${Math.round(detail.loadTonnes)} t` : '—' },
            { label: 'Vel. máx. (km/h)', value: detail.maxSpeedKmh ?? '—' },
          ]}
        />
      )}

      {isActive && liveVehicle && (
        <LiveProgress liveVehicle={liveVehicle} progress={progress} />
      )}

      <TripMapCard
        latLngs={latLngs}
        geofences={geofenceList}
        route={routeItem}
        vehicle={vehiclePos}
        vehicleCode={detail?.vehicleCode}
        origin={origin}
        destination={destination}
        highlight={[originGeofence?.id, destGeofence?.id].filter((v): v is string => v != null)}
        loading={track.loading || geofences.loading}
      />

      {detail && <TripDetailsCard detail={detail} />}
    </div>
  )
}

function centroid(g: Geofence | null): [number, number] | null {
  if (!g) return null
  const c = polygonCentroid(parseGeoJsonPolygon(g.geoJsonPolygon))
  return c ? [c.lat, c.lon] : null
}