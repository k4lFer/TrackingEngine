import { useEffect, useRef, useState } from 'react'
import { CircleMarker, MapContainer, Polyline, Popup, Polygon } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import type { Geofence } from '../core/api/geofences'
import type { VehicleLive } from '../core/api/live'
import type { RouteItem } from '../core/api/routes'
import type { VehicleStatus } from '../core/api/vehicles'
import { parseGeoJsonLine, parseGeoJsonPolygon, timeAgo } from '../core/format'
import BaseMap from './BaseMap'

const CENTER: [number, number] = [-13.53, -72.87]

const ROUTE_COLORS = ['#5aa2e8', '#f5a623', '#37b6c9']

const STATE_COLOR: Record<string, string> = {
  Moving: '#2f9e6e',
  Idle: '#b8861a',
  Stopped: '#b8861a',
  Off: '#6b7689',
  Unknown: '#8b98a8',
  Disponible: '#2f9e6e',
  EnRuta: '#2f9e6e',
  EnCarga: '#3d83d0',
  EnDescarga: '#2f9e6e',
  Detenido: '#b8861a',
  Offline: '#6b7689',
  FueraDeServicio: '#8b98a8',
}

interface Props {
  vehicles: VehicleStatus[]
  geofences?: Geofence[]
  routes?: RouteItem[]
  live?: Record<string, VehicleLive>
  onSelectVehicle?: (id: string) => void
}

function fmtKmh(speed: number | null | undefined): string {
  return speed != null ? `${Math.round(speed)} km/h` : '—'
}

export default function FleetMap({ vehicles, geofences = [], routes = [], live = {}, onSelectVehicle }: Props) {
  const [map, setMap] = useState<L.Map | null>(null)
  const fitted = useRef(false)

  useEffect(() => {
    if (!map) return
    const timer = setTimeout(() => map.invalidateSize(), 200)
    return () => clearTimeout(timer)
  }, [map])

  useEffect(() => {
    if (!map || fitted.current) return
    if (routes.length === 0 && geofences.length === 0 && vehicles.length === 0) return
    const pts: [number, number][] = []
    routes.forEach((r) => parseGeoJsonLine(r.geoJsonLine).forEach((c) => pts.push([c.lat, c.lon])))
    geofences.forEach((g) => parseGeoJsonPolygon(g.geoJsonPolygon).forEach((c) => pts.push([c.lat, c.lon])))
    vehicles.forEach((v) => {
      if (v.lastLat != null && v.lastLon != null) pts.push([v.lastLat, v.lastLon])
    })
    if (pts.length > 0) {
      fitted.current = true
      map.fitBounds(L.latLngBounds(pts), { padding: [40, 40] })
    }
  }, [map, routes.length, geofences.length, vehicles.length])

  return (
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
            pathOptions={{ color: g.color || '#e09b3d', fillOpacity: 0.12, weight: 2 }}
          >
            <Popup>
              <strong>{g.name}</strong>
              <br />
              <span className="mono">{g.kind}</span>
            </Popup>
          </Polygon>
        )
      })}

      {routes.map((r, i) => {
        const coords = parseGeoJsonLine(r.geoJsonLine)
        if (coords.length < 2) return null
        const latLngs: [number, number][] = coords.map((c) => [c.lat, c.lon])
        return (
          <Polyline
            key={r.id}
            positions={latLngs}
            pathOptions={{
              color: ROUTE_COLORS[i % ROUTE_COLORS.length],
              weight: 5,
              dashArray: '3 7',
              opacity: 0.95,
            }}
          >
            <Popup>
              <strong>{r.name}</strong>
              <br />
              <span className="mono">≈ {r.distanceKm.toFixed(2)} km</span>
            </Popup>
          </Polyline>
        )
      })}

      {vehicles.map((v) => {
        const lv = live[v.id]
        if (lv == null && (v.lastLat == null || v.lastLon == null)) return null
        const lat = lv?.lat ?? v.lastLat!
        const lon = lv?.lon ?? v.lastLon!
        const state = lv != null && lv.speedKmh != null && lv.speedKmh >= 3 ? 'Moving' : v.state
        const color = STATE_COLOR[state] ?? '#98a2b3'
        return (
          <CircleMarker
            key={v.id}
            center={[lat, lon]}
            radius={lv ? 9 : 8}
            pathOptions={{ color, fillColor: color, fillOpacity: 0.7, weight: 2 }}
            eventHandlers={onSelectVehicle ? { click: () => onSelectVehicle(v.id) } : undefined}
          >
            <Popup>
              <strong>{v.code}</strong> · {v.plate}
              <br />
              <span className="mono">{state}</span>
              {lv ? (
                <>
                  <br />
                  <span className="mono">{fmtKmh(lv.speedKmh)}</span>
                  {lv.materialName && (
                    <>
                      <br />
                      <span className="mono">
                      {lv.materialName}: {Math.round(lv.loadTonnes ?? 0)} t
                    </span>
                    </>
                  )}
                  {lv.etaSec > 0 && (
                    <>
                      <br />
                      <span className="mono">
                        ETA {Math.round(lv.etaSec / 60)} min
                      </span>
                    </>
                  )}
                </>
              ) : (
                <>
                  <br />
                  <span className="mono">{timeAgo(v.lastReportedAt)}</span>
                </>
              )}
            </Popup>
          </CircleMarker>
        )
      })}
    </MapContainer>
  )
}