import { Fragment, useEffect, useRef, useState } from 'react'
import { CircleMarker, MapContainer, Polygon, Polyline, Popup, Tooltip } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import type { Geofence } from '../../../core/api/geofences'
import type { VehicleLive } from '../../../core/api/live'
import type { RouteItem } from '../../../core/api/routes'
import type { TripSummary } from '../../../core/api/trips'
import type { VehicleStatus } from '../../../core/api/vehicles'
import { parseGeoJsonLine, parseGeoJsonPolygon } from '../../../core/format'
import { useSignalR } from '../../../core/hooks/useSignalR'
import BaseMap from '../../../components/BaseMap'

const CENTER: [number, number] = [-13.53, -72.87]

const ROUTE_COLOR = '#5aa2e8'
const ALT_COLOR = '#9cc7f0'
const ORIGIN_COLOR = '#28a745'
const DEST_COLOR = '#dc3545'
const VEHICLE_COLOR = '#41b883'

interface TripLiveMapProps {
  trips: TripSummary[]
  vehicles: VehicleStatus[]
  geofences: Geofence[]
  routes: RouteItem[]
  live?: Record<string, VehicleLive>
  onSelectTrip?: (id: string) => void
  loading?: boolean
}

function polygonCenter(geoJson: string): [number, number] | null {
  const c = parseGeoJsonPolygon(geoJson)
  if (c.length === 0) return null
  return [
    c.reduce((s, p) => s + p.lat, 0) / c.length,
    c.reduce((s, p) => s + p.lon, 0) / c.length,
  ]
}

function tripPosition(
  trip: TripSummary,
  vehicles: VehicleStatus[],
  live: Record<string, VehicleLive>,
): [number, number] | null {
  const lv = live[trip.vehicleId]
  if (lv != null && Number.isFinite(lv.lat) && Number.isFinite(lv.lon)) return [lv.lat, lv.lon]
  const v = vehicles.find((x) => x.id === trip.vehicleId)
  if (v?.lastLat != null && v?.lastLon != null) return [v.lastLat, v.lastLon]
  return null
}

export default function TripLiveMap({
  trips,
  vehicles,
  geofences,
  routes,
  live = {},
  onSelectTrip,
  loading = false,
}: TripLiveMapProps) {
  const { connected } = useSignalR()
  const [map, setMap] = useState<L.Map | null>(null)
  const fitted = useRef(false)
  const [focused, setFocused] = useState<string | null>(null)

  const geofenceById = new Map(geofences.map((g) => [g.id, g]))
  const routeById = new Map(routes.map((r) => [r.id, r]))

  useEffect(() => {
    if (!map) return
    const t = setTimeout(() => map.invalidateSize(), 200)
    return () => clearTimeout(t)
  }, [map])

  useEffect(() => {
    if (!map || fitted.current) return
    const pts: [number, number][] = []
    trips.forEach((t) => {
      const p = tripPosition(t, vehicles, live)
      if (p) pts.push(p)
      const r = routeById.get(t.routeId ?? '')
      if (r) parseGeoJsonLine(r.geoJsonLine).forEach((c) => pts.push([c.lat, c.lon]))
    })
    geofences.forEach((g) => {
      parseGeoJsonPolygon(g.geoJsonPolygon).forEach((c) => pts.push([c.lat, c.lon]))
    })
    if (pts.length > 0) {
      fitted.current = true
      map.fitBounds(L.latLngBounds(pts), { padding: [45, 45] })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map])

  if (loading) {
    return (
      <div className="card map-card" style={{ marginBottom: 16 }}>
        <div className="card-header"><h3>Viajes en vivo</h3></div>
        <div className="loading" style={{ minHeight: 360 }}>Cargando posiciones…</div>
      </div>
    )
  }

  if (trips.length === 0) {
    return (
      <div className="card map-card" style={{ marginBottom: 16 }}>
        <div className="card-header">
          <h3>Viajes en vivo <span className={`live-dot ${connected ? 'on' : ''}`} /></h3>
        </div>
        <div className="empty" style={{ minHeight: 240 }}>
          No hay viajes en curso. Usa «Nuevo viaje» para iniciar uno.
        </div>
      </div>
    )
  }

  const originMarkers = new Map<string, [number, number]>()
  const destMarkers = new Map<string, [number, number]>()
  trips.forEach((t) => {
    if (t.originGeofenceId) {
      const g = geofenceById.get(t.originGeofenceId)
      const c = g ? polygonCenter(g.geoJsonPolygon) : null
      if (c) originMarkers.set(t.originGeofenceId, c)
    }
    if (t.destinationGeofenceId) {
      const g = geofenceById.get(t.destinationGeofenceId)
      const c = g ? polygonCenter(g.geoJsonPolygon) : null
      if (c) destMarkers.set(t.destinationGeofenceId, c)
    }
  })

  const seenRoutes = new Set<string>()

  return (
    <div className="card map-card" style={{ marginBottom: 16 }}>
      <div className="card-header">
        <h3>Viajes en vivo <span className={`live-dot ${connected ? 'on' : ''}`} /></h3>
        <div className="map-legend">
          <span className="legend-item"><span className="dot" style={{ background: VEHICLE_COLOR }} /> En movimiento</span>
          <span className="legend-item"><span className="dot" style={{ background: ROUTE_COLOR }} /> Ruta</span>
          {trips.some((t) => (routeById.get(t.routeId ?? '')?.alternatives?.length ?? 0) > 0) && (
            <span className="legend-item"><span className="dot" style={{ background: ALT_COLOR }} /> Alternativas</span>
          )}
          <span className="legend-item"><span className="dot" style={{ background: ORIGIN_COLOR }} /> Origen</span>
          <span className="legend-item"><span className="dot" style={{ background: DEST_COLOR }} /> Destino</span>
        </div>
      </div>
      <div className="map-body" style={{ minHeight: 460 }}>
        <MapContainer center={CENTER} zoom={13} ref={setMap}>
          <BaseMap />

          {geofences.map((g) => {
            const coords = parseGeoJsonPolygon(g.geoJsonPolygon)
            if (coords.length < 4) return null
            const latLngs: [number, number][] = coords.map((c) => [c.lat, c.lon])
            return (
              <Polygon
                key={g.id}
                positions={latLngs}
                pathOptions={{ color: g.color || '#e09b3d', fillOpacity: 0.08, weight: 2, dashArray: '4 6' }}
              >
                <Popup>
                  <strong>{g.name}</strong>
                  <br />
                  <span className="mono">{g.code}</span>
                </Popup>
              </Polygon>
            )
          })}

          {trips.map((t) => {
            if (!t.routeId || seenRoutes.has(t.routeId)) return null
            const r = routeById.get(t.routeId)
            if (!r) return null
            seenRoutes.add(t.routeId)
            const coords = parseGeoJsonLine(r.geoJsonLine).map((c): [number, number] => [c.lat, c.lon])
            if (coords.length < 2) return null
            const isFocused = focused === t.routeId
            return (
              <Fragment key={t.routeId}>
                <Polyline
                  positions={coords}
                  pathOptions={
                    isFocused
                      ? { color: ROUTE_COLOR, weight: 6, opacity: 1 }
                      : { color: ROUTE_COLOR, weight: 3, opacity: 0.22 }
                  }
                  eventHandlers={{
                    mouseover: () => setFocused(t.routeId),
                    mouseout: () => setFocused(null),
                    click: () => setFocused(t.routeId),
                  }}
                >
                  {isFocused && (
                    <Tooltip permanent direction="top" opacity={1}>
                      <strong>Ruta: {r.name}</strong>
                    </Tooltip>
                  )}
                  <Popup>
                    <strong>{r.code} · {r.name}</strong>
                    <br />
                    <span className="mono">≈ {r.distanceKm.toFixed(2)} km</span>
                  </Popup>
                </Polyline>
                {(r.alternatives ?? []).map((a, i) => {
                  if (!a.geoJsonLine) return null
                  const aCoords = parseGeoJsonLine(a.geoJsonLine).map((c): [number, number] => [c.lat, c.lon])
                  if (aCoords.length < 2) return null
                  return (
                    <Polyline
                      key={`${r.id}-alt-${i}`}
                      positions={aCoords}
                      pathOptions={
                        isFocused
                          ? { color: ALT_COLOR, weight: 3, dashArray: '6 6', opacity: 0.9 }
                          : { color: ALT_COLOR, weight: 3, dashArray: '6 6', opacity: 0.12 }
                      }
                    >
                      {isFocused && (
                        <Tooltip direction="top" opacity={1}>
                          <strong>Alternativa {i + 1}</strong>
                        </Tooltip>
                      )}
                    </Polyline>
                  )
                })}
              </Fragment>
            )
          })}

          {[...originMarkers.entries()].map(([id, center]) => (
            <CircleMarker
              key={`o-${id}`}
              center={center}
              radius={focused ? 9 : 6}
              pathOptions={{
                color: ORIGIN_COLOR,
                fillColor: ORIGIN_COLOR,
                fillOpacity: focused ? 0.9 : 0.32,
                weight: 2,
                opacity: focused ? 1 : 0.45,
              }}
            >
              {focused && (
                <Tooltip direction="top" offset={[0, -12]} permanent>Origen</Tooltip>
              )}
            </CircleMarker>
          ))}
          {[...destMarkers.entries()].map(([id, center]) => (
            <CircleMarker
              key={`d-${id}`}
              center={center}
              radius={focused ? 9 : 6}
              pathOptions={{
                color: DEST_COLOR,
                fillColor: DEST_COLOR,
                fillOpacity: focused ? 0.9 : 0.32,
                weight: 2,
                opacity: focused ? 1 : 0.45,
              }}
            >
              {focused && (
                <Tooltip direction="top" offset={[0, -12]} permanent>Destino</Tooltip>
              )}
            </CircleMarker>
          ))}

          {trips.map((t) => {
            const pos = tripPosition(t, vehicles, live)
            if (!pos) return null
            const lv = live[t.vehicleId]
            return (
              <CircleMarker
                key={t.id}
                center={pos}
                radius={lv ? 10 : 8}
                pathOptions={{ color: '#1c2433', fillColor: VEHICLE_COLOR, fillOpacity: 1, weight: 3 }}
                eventHandlers={onSelectTrip ? { click: () => onSelectTrip(t.id) } : undefined}
              >
                <Tooltip direction="top" offset={[0, -14]}>
                  <strong>{t.vehicleCode}</strong> · En curso
                </Tooltip>
                <Popup>
                  <strong>{t.vehicleCode}</strong>
                  <br />
                  <span className="mono">{t.originName ?? '—'} → {t.destinationName ?? '—'}</span>
                  {t.materialName && (
                    <>
                      <br />
                      <span className="mono">{t.materialName}: {Math.round(t.loadTonnes ?? 0)} t</span>
                    </>
                  )}
                  {lv && lv.speedKmh != null && (
                    <>
                      <br />
                      <span className="mono">{Math.round(lv.speedKmh)} km/h</span>
                    </>
                  )}
                  {lv && lv.etaSec > 0 && (
                    <>
                      <br />
                      <span className="mono">ETA {Math.round(lv.etaSec / 60)} min</span>
                    </>
                  )}
                </Popup>
              </CircleMarker>
            )
          })}
        </MapContainer>
      </div>
    </div>
  )
}
