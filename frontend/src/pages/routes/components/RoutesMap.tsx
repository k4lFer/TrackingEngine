import { useMemo } from 'react'
import { MapContainer, Polyline, Popup, Polygon } from 'react-leaflet'
import type { Geofence } from '../../../core/api/geofences'
import type { RouteAlternativeItem, RouteItem } from '../../../core/api/routes'
import { parseGeoJsonLine, parseGeoJsonPolygon } from '../../../core/format'
import BaseMap from '../../../components/BaseMap'

const GEO_CENTER: [number, number] = [-13.53, -72.87]

const ROUTE_COLORS = [
  '#2563eb', '#d97706', '#16a34a', '#9333ea', '#dc2626', '#0d9488',
  '#ca8a04', '#1d4ed8', '#ea580c', '#059669', '#7c3aed', '#0891b2',
]

const ROUTE_DASHES = [
  '8 4', '14 6 4 6', '4 4', '10 3 2 3', '6 2 2 2', '12 5 3 5',
]

const VARIANT_COLOR = 'rgba(112, 117, 128, 0.65)'

interface RoutesMapProps {
  items: RouteItem[]
  variants: RouteAlternativeItem[]
  geofences: Geofence[]
}

/** Hash estable por id para que cada ruta conserve su color entre renders. */
function hashCode(str: string): number {
  let h = 0
  for (let i = 0; i < str.length; i++) h = (h * 31 + str.charCodeAt(i)) | 0
  return Math.abs(h)
}

/** Asigna un color distinto (y su dash) a cada ruta; si se agotan, recorre la paleta. */
function assignStyles(items: RouteItem[]): Map<string, { color: string; dash: string }> {
  const used = new Set<number>()
  const map = new Map<string, { color: string; dash: string }>()
  for (const r of items) {
    let idx = hashCode(r.id) % ROUTE_COLORS.length
    while (used.has(idx)) idx = (idx + 1) % ROUTE_COLORS.length
    used.add(idx)
    map.set(r.id, {
      color: ROUTE_COLORS[idx],
      dash: ROUTE_DASHES[idx % ROUTE_DASHES.length],
    })
  }
  return map
}

export default function RoutesMap({ items, variants, geofences }: RoutesMapProps) {
  const styles = useMemo(() => assignStyles(items), [items])

  return (
    <>
      <div className="map-fill">
        <MapContainer center={GEO_CENTER} zoom={13}>
          <BaseMap />
          {geofences.map((g) => {
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
        {variants.map((a) => {
          if (!a.geoJsonLine) return null
          const pts = parseGeoJsonLine(a.geoJsonLine)
          if (pts.length < 2) return null
          const pos: [number, number][] = pts.map((c) => [c.lat, c.lon])
          return (
            <Polyline key={a.id} positions={pos} pathOptions={{ color: VARIANT_COLOR, weight: 2, dashArray: '3 5', opacity: 0.55 }}>
              <Popup>
                <strong>{a.name}</strong>
                <br />
                <span className="mono">Alternativa {a.alternativeRank}</span>
                <br />
                <span className="mono">≈ {a.distanceKm.toFixed(2)} km</span>
              </Popup>
            </Polyline>
          )
        })}
        {items.map((r) => {
          const pts = parseGeoJsonLine(r.geoJsonLine)
          if (pts.length < 2) return null
          const pos: [number, number][] = pts.map((c) => [c.lat, c.lon])
          const style = styles.get(r.id)
          return (
            <Polyline
              key={r.id}
              positions={pos}
              pathOptions={{
                color: style?.color ?? ROUTE_COLORS[0],
                weight: 4,
                dashArray: style?.dash ?? '8 4',
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
        </MapContainer>
      </div>

      <div className="legend">
        {items.map((r) => (
          <span key={r.id} className="legend-item">
            <span className="legend-chip" style={{ background: styles.get(r.id)?.color ?? ROUTE_COLORS[0] }} />
            {r.name}
            <span className="muted">· {r.distanceKm.toFixed(2)} km</span>
          </span>
        ))}
        {variants.length > 0 && (
          <span className="legend-item">
            <span className="legend-chip legend-chip--variant" />
            Alternativas ({variants.length})
          </span>
        )}
      </div>
    </>
  )
}