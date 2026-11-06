import { useEffect, useRef, useState } from 'react'
import { MapContainer, Polygon, Popup } from 'react-leaflet'
import L from 'leaflet'
import type { Geofence } from '../../../core/api/geofences'
import { parseGeoJsonPolygon } from '../../../core/format'
import BaseMap from '../../../components/BaseMap'
import { FILL_COLOR, GEO_CENTER } from '../geofenceConstants'

interface GeofencesMapProps {
  items: Geofence[]
}

export default function GeofencesMap({ items }: GeofencesMapProps) {
  const [map, setMap] = useState<L.Map | null>(null)
  const fitted = useRef(false)

  useEffect(() => {
    if (!map || fitted.current || items.length === 0) return
    const pts: [number, number][] = []
    items.forEach((g) => parseGeoJsonPolygon(g.geoJsonPolygon).forEach((c) => pts.push([c.lat, c.lon])))
    if (pts.length > 0) {
      fitted.current = true
      map.fitBounds(L.latLngBounds(pts), { padding: [40, 40] })
    }
  }, [map, items.length])

  return (
    <MapContainer ref={setMap} center={GEO_CENTER} zoom={13}>
      <BaseMap />
      {items.map((g) => {
        const ring = parseGeoJsonPolygon(g.geoJsonPolygon)
        if (ring.length < 4) return null
        const pos: [number, number][] = ring.map((c) => [c.lat, c.lon])
        return (
          <Polygon
            key={g.id}
            positions={pos}
            pathOptions={{ color: g.color || FILL_COLOR, weight: 3, fillOpacity: 0.18 }}
          >
            <Popup>
              <strong>{g.name}</strong>
              <br />
              <span className="mono">{g.kind}</span>
            </Popup>
          </Polygon>
        )
      })}
    </MapContainer>
  )
}