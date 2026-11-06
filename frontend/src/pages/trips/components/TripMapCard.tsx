import { useEffect, useRef, useState } from 'react'
import { CircleMarker, MapContainer, Polygon, Polyline, Popup, Tooltip } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import type { Geofence } from '../../../core/api/geofences'
import type { RouteItem } from '../../../core/api/routes'
import { parseGeoJsonLine, parseGeoJsonPolygon } from '../../../core/format'
import BaseMap from '../../../components/BaseMap'

const ROUTE_COLOR = '#5aa2e8'
const TRACK_COLOR = '#e09b3d'
const VEHICLE_COLOR = '#41b883'
const ORIGIN_COLOR = '#28a745'
const DEST_COLOR = '#dc3545'

interface TripMapCardProps {
  latLngs: [number, number][]
  geofences: Geofence[]
  route: RouteItem | null
  vehicle: [number, number] | null
  vehicleCode?: string
  origin: [number, number] | null
  destination: [number, number] | null
  highlight?: string[]
  loading?: boolean
}

export default function TripMapCard({
  latLngs,
  geofences,
  route,
  vehicle,
  vehicleCode,
  origin,
  destination,
  highlight = [],
  loading = false,
}: TripMapCardProps) {
  return (
    <div className="card" style={{ marginBottom: 16 }}>
      <div className="card-header">
        <h3>Recorrido</h3>
        <div className="map-legend">
          {origin && <span className="legend-item"><span className="dot" style={{ background: ORIGIN_COLOR }} /> Salida</span>}
          <span className="legend-item"><span className="dot" style={{ background: TRACK_COLOR }} /> Recorrido</span>
          {route && <span className="legend-item"><span className="dot" style={{ background: ROUTE_COLOR }} /> Ruta</span>}
          <span className="legend-item"><span className="dot" style={{ background: VEHICLE_COLOR }} /> Vehículo</span>
          {destination && <span className="legend-item"><span className="dot" style={{ background: DEST_COLOR }} /> Destino</span>}
        </div>
      </div>
      {loading ? (
        <div className="loading">Cargando recorrido…</div>
      ) : hasData(latLngs, geofences) ? (
        <div className="map-body" style={{ minHeight: 460 }}>
          <TripMap
            latLngs={latLngs}
            geofences={geofences}
            route={route}
            vehicle={vehicle}
            vehicleCode={vehicleCode}
            origin={origin}
            destination={destination}
            highlight={highlight}
          />
        </div>
      ) : (
        <div className="empty">Sin datos geográficos para este viaje.</div>
      )}
    </div>
  )
}

function hasData(latLngs: [number, number][], geofences: Geofence[]): boolean {
  return latLngs.length > 0 || geofences.length > 0
}

interface TripMapProps {
  latLngs: [number, number][]
  geofences: Geofence[]
  route: RouteItem | null
  vehicle: [number, number] | null
  vehicleCode?: string
  origin: [number, number] | null
  destination: [number, number] | null
  highlight: string[]
}

function TripMap({
  latLngs,
  geofences,
  route,
  vehicle,
  vehicleCode,
  origin,
  destination,
  highlight,
}: TripMapProps) {
  const [map, setMap] = useState<L.Map | null>(null)
  const fitted = useRef(false)

  useEffect(() => {
    if (!map) return
    const t = setTimeout(() => map.invalidateSize(), 200)
    return () => clearTimeout(t)
  }, [map])

  useEffect(() => {
    if (!map || fitted.current) return
    const pts: [number, number][] = [...latLngs]
    if (vehicle) pts.push(vehicle)
    if (origin) pts.push(origin)
    if (destination) pts.push(destination)
    geofences.forEach((g) => parseGeoJsonPolygon(g.geoJsonPolygon).forEach((c) => pts.push([c.lat, c.lon])))
    if (route) parseGeoJsonLine(route.geoJsonLine).forEach((c) => pts.push([c.lat, c.lon]))
    if (pts.length > 0) {
      fitted.current = true
      map.fitBounds(L.latLngBounds(pts), { padding: [45, 45] })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map])

  return (
    <MapContainer center={latLngs[Math.floor(latLngs.length / 2)] ?? [-13.53, -72.87]} zoom={14} ref={setMap}>
      <BaseMap />

      {geofences.map((g) => {
        const pts = parseGeoJsonPolygon(g.geoJsonPolygon)
        if (pts.length < 4) return null
        const pos: [number, number][] = pts.map((c): [number, number] => [c.lat, c.lon])
        const hl = highlight.includes(g.id)
        return (
          <Polygon
            key={g.id}
            positions={pos}
            pathOptions={{
              color: g.color || '#e09b3d',
              weight: hl ? 4 : 2,
              fillOpacity: hl ? 0.2 : 0.1,
              dashArray: hl ? '8 4' : '4 6',
            }}
          >
            <Popup>
              <strong>{g.name}</strong>
              <br />
              <span className="mono">{g.kind}</span>
            </Popup>
          </Polygon>
        )
      })}

      {route && (
        <Polyline
          positions={parseGeoJsonLine(route.geoJsonLine).map((c): [number, number] => [c.lat, c.lon])}
          pathOptions={{ color: ROUTE_COLOR, weight: 5, opacity: 1 }}
        >
          <Tooltip permanent direction="top" opacity={1}>
            <strong>Ruta: {route.name}</strong>
          </Tooltip>
          <Popup>
            <strong>Ruta: {route.name}</strong>
          </Popup>
        </Polyline>
      )}

      {latLngs.length > 1 && (
        <Polyline positions={latLngs} pathOptions={{ color: TRACK_COLOR, weight: 4, opacity: 1 }}>
          <Tooltip permanent direction="top" opacity={1}>
            <strong>Recorrido</strong>
          </Tooltip>
          <Popup>
            <strong>Recorrido real</strong>
          </Popup>
        </Polyline>
      )}

      {origin && (
        <CircleMarker center={origin} radius={10} pathOptions={{ color: ORIGIN_COLOR, fillColor: ORIGIN_COLOR, fillOpacity: 0.9, weight: 2 }}>
          <Tooltip direction="top" offset={[0, -12]}>Salida</Tooltip>
        </CircleMarker>
      )}
      {destination && (
        <CircleMarker center={destination} radius={10} pathOptions={{ color: DEST_COLOR, fillColor: DEST_COLOR, fillOpacity: 0.9, weight: 2 }}>
          <Tooltip direction="top" offset={[0, -12]}>Destino</Tooltip>
        </CircleMarker>
      )}

      {vehicle && (
        <CircleMarker
          center={vehicle}
          radius={9}
          pathOptions={{ color: '#1c2433', fillColor: VEHICLE_COLOR, fillOpacity: 1, weight: 3 }}
        >
          <Tooltip direction="top" offset={[0, -14]}>
            <strong>{vehicleCode}</strong>
          </Tooltip>
        </CircleMarker>
      )}
    </MapContainer>
  )
}