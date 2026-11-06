import { CircleMarker, MapContainer, Marker, Polygon, Polyline, Popup, Tooltip, useMapEvents } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import type { CoordinateDto } from '../core/api/common'
import type { Geofence } from '../core/api/geofences'
import { parseGeoJsonLine, parseGeoJsonPolygon } from '../core/format'
import BaseMap from './BaseMap'

const DEFAULT_CENTER: [number, number] = [-13.53, -72.87]

const PREVIEW_COLOR = '#2fbf71'
const VARIANT_COLOR = 'rgba(245, 166, 35, 0.8)'

interface ClickMapProps {
  points: CoordinateDto[]
  onAdd: (lat: number, lon: number) => void
  color?: string
  shape?: 'polygon' | 'line'
  closed?: boolean
  onMovePoint?: (index: number, lat: number, lon: number) => void
  onRemovePoint?: (index: number) => void
  onVertexClick?: (index: number) => void
  lockEnds?: boolean
  preview?: string
  variants?: string[]
  existing?: Geofence[]
}

function ClickHandler({ onAdd }: { onAdd: (lat: number, lon: number) => void }) {
  useMapEvents({
    click(e: { latlng: { lat: number; lng: number } }) {
      onAdd(e.latlng.lat, e.latlng.lng)
    },
    contextmenu(e: { originalEvent: Event }) {
      e.originalEvent.preventDefault()
      e.originalEvent.stopPropagation()
    },
  })
  return null
}

function vertexIcon(color: string): L.DivIcon {
  return L.divIcon({
    className: '',
    html: `<div style="width:14px;height:14px;border-radius:50%;background:${color};border:2px solid #fff;box-shadow:0 1px 4px rgba(0,0,0,.6)"></div>`,
    iconSize: [14, 14],
    iconAnchor: [7, 7],
  })
}

function endpointIcon(color: string): L.DivIcon {
  return L.divIcon({
    className: '',
    html: `<div style="width:16px;height:16px;border-radius:50%;background:${color};border:2px solid #fff;box-shadow:0 1px 4px rgba(0,0,0,.7)"></div>`,
    iconSize: [16, 16],
    iconAnchor: [8, 8],
  })
}

function fmtDMS(lat: number, lon: number): string {
  const dms = (deg: number, n: string, s: string) => {
    const abs = Math.abs(deg)
    const d = Math.floor(abs)
    const mFull = (abs - d) * 60
    const m = Math.floor(mFull)
    const sec = (mFull - m) * 60
    return `${d}°${m}'${sec.toFixed(1)}"${deg < 0 ? s : n}`
  }
  return `${dms(lat, 'N', 'S')} ${dms(lon, 'E', 'O')}`
}

export default function ClickMap({
  points,
  onAdd,
  color = '#e09b3d',
  shape = 'polygon',
  closed = false,
  onMovePoint,
  onRemovePoint,
  onVertexClick,
  lockEnds = false,
  preview,
  variants = [],
  existing = [],
}: ClickMapProps) {
  const latLngs: [number, number][] = points.map((p) => [p.lat, p.lon])
  const isPolygon = shape === 'polygon'

  return (
    <MapContainer center={DEFAULT_CENTER} zoom={13} style={{ height: '100%', width: '100%' }}>
      <BaseMap />
      {existing.map((g) => {
        const ring = parseGeoJsonPolygon(g.geoJsonPolygon)
        if (ring.length < 4) return null
        const pos: [number, number][] = ring.map((c) => [c.lat, c.lon])
        return (
          <Polygon
            key={g.id}
            positions={pos}
            pathOptions={{ color: g.color || '#e09b3d', weight: 2, dashArray: '4 6', fillOpacity: 0.06 }}
          />
        )
      })}
      <ClickHandler onAdd={onAdd} />
      {variants.map((line, i) => {
        const pts = previewLine(line)
        if (pts.length < 2) return null
        return (
          <Polyline
            key={`variant-${i}`}
            positions={pts}
            pathOptions={{ color: VARIANT_COLOR, weight: 3, dashArray: '2 6', opacity: 0.9 }}
          />
        )
      })}
      {preview && previewLine(preview).length >= 2 && (
        <Polyline
          positions={previewLine(preview)}
          pathOptions={{ color: PREVIEW_COLOR, weight: 3, opacity: 0.95 }}
        />
      )}
      {isPolygon && closed && latLngs.length >= 3 && (
        <Polygon positions={latLngs} pathOptions={{ color, weight: 3, fillOpacity: 0.25 }} />
      )}
      {isPolygon && !closed && latLngs.length >= 2 && (
        <Polyline positions={latLngs} pathOptions={{ color, weight: 3 }} />
      )}
      {isPolygon && !closed && latLngs.length >= 3 && (
        <Polyline
          positions={[latLngs[latLngs.length - 1], latLngs[0]]}
          pathOptions={{ color, weight: 2, dashArray: '6 6' }}
        />
      )}
      {points.map((p, i) => {
        const isEnd = lockEnds && (i === 0 || i === points.length - 1)
        if (isEnd) {
          return (
            <Marker
              key={i}
              position={[p.lat, p.lon]}
              icon={endpointIcon(color)}
              interactive
            >
              <Tooltip direction="top" offset={[0, -10]}>
                <strong>{i === 0 ? 'Inicio' : 'Fin'}</strong>
                <br />
                <span className="mono">{p.lat.toFixed(5)}, {p.lon.toFixed(5)}</span>
              </Tooltip>
            </Marker>
          )
        }
        return onMovePoint ? (
          <Marker
            key={i}
            position={[p.lat, p.lon]}
            icon={vertexIcon(color)}
            draggable
            eventHandlers={{
              dragend: (e) => {
                const c = e.target.getLatLng()
                onMovePoint(i, c.lat, c.lng)
              },
              click: () => {
                onVertexClick?.(i)
              },
              contextmenu: (e) => {
                e.originalEvent.preventDefault()
                e.originalEvent.stopPropagation()
                onRemovePoint?.(i)
              },
            }}
          >
            <Tooltip direction="top" offset={[0, -8]}>
              <span className="mono">{fmtDMS(p.lat, p.lon)}</span>
            </Tooltip>
            {!onVertexClick && (
              <Popup>
                <div style={{ fontSize: 12 }}>
                  <div className="mono"><b>Lat</b> {p.lat.toFixed(6)}</div>
                  <div className="mono"><b>Lon</b> {p.lon.toFixed(6)}</div>
                  <div className="mono"><b>DMS</b> {fmtDMS(p.lat, p.lon)}</div>
                  <div className="mono"><b>Alt</b> —</div>
                  <div style={{ display: 'flex', gap: 6, marginTop: 6 }}>
                    <button type="button" className="btn btn-sm" onClick={() => navigator.clipboard?.writeText(`${p.lat.toFixed(6)}, ${p.lon.toFixed(6)}`)}>Copiar lat,lon</button>
                    <button type="button" className="btn btn-sm" onClick={() => navigator.clipboard?.writeText(fmtDMS(p.lat, p.lon))}>Copiar DMS</button>
                  </div>
                </div>
              </Popup>
            )}
          </Marker>
        ) : (
          <CircleMarker
            key={i}
            center={[p.lat, p.lon]}
            radius={i === 0 ? 5 : 4}
            pathOptions={{ color, fillColor: color, fillOpacity: 1 }}
          >
              <Tooltip direction="top" offset={[0, -8]}>
                <span className="mono">{fmtDMS(p.lat, p.lon)}</span>
              </Tooltip>
          </CircleMarker>
        )
      })}
    </MapContainer>
  )
}

function previewLine(geoJson: string): [number, number][] {
  const pts = parseGeoJsonLine(geoJson)
  return pts.map((c) => [c.lat, c.lon])
}