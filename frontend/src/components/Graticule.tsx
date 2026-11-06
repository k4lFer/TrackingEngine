import L from 'leaflet'
import { useLayoutEffect } from 'react'
import { useMap } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'

// Capa de grilla lat/lon dibujada en canvas. Sirve de fondo del mapa cuando las
// tiles externas (OpenStreetMap) no están disponibles: garantiza que rutas,
// geocercas y vehículos siempre se vean sobre un mapa reconocible.
class GraticuleLayer extends L.GridLayer {
  createTile(coords: L.Coords): HTMLElement {
    const size = this.getTileSize()
    const canvas = L.DomUtil.create('canvas', 'leaflet-tile') as HTMLCanvasElement
    canvas.width = size.x
    canvas.height = size.y

    const ctx = canvas.getContext('2d')
    const map = (this as unknown as { _map: L.Map | null })._map
    if (!ctx || !map) return canvas

    const z = coords.z
    const nw = map.unproject([coords.x * size.x, coords.y * size.y], z)
    const se = map.unproject([(coords.x + 1) * size.x, (coords.y + 1) * size.y], z)

    const latLo = Math.min(nw.lat, se.lat)
    const latHi = Math.max(nw.lat, se.lat)
    const lonLo = Math.min(nw.lng, se.lng)
    const lonHi = Math.max(nw.lng, se.lng)
    const midLat = (latLo + latHi) / 2

    ctx.fillStyle = '#e9edf2'
    ctx.fillRect(0, 0, size.x, size.y)

    const minor = 0.02 / Math.pow(2, z - 12)
    const major = minor * 5

    ctx.textBaseline = 'middle'
    ctx.font = '10px ui-monospace, SFMono-Regular, Menlo, monospace'

    for (let lon = Math.ceil(lonLo / minor) * minor; lon <= lonHi; lon += minor) {
      const ex = map.project([midLat, lon], z)
      const x = ex.x - coords.x * size.x
      const isMajor = Math.abs(lon / major - Math.round(lon / major)) < 1e-9
      ctx.strokeStyle = isMajor ? 'rgba(28,36,51,0.18)' : 'rgba(28,36,51,0.09)'
      ctx.beginPath()
      ctx.moveTo(x, 0)
      ctx.lineTo(x, size.y)
      ctx.stroke()
      if (isMajor) {
        ctx.fillStyle = 'rgba(28,36,51,0.55)'
        ctx.fillText(lon.toFixed(3), x + 3, 10)
      }
    }

    for (let lat = Math.ceil(latLo / minor) * minor; lat <= latHi; lat += minor) {
      const ey = map.project([lat, lonLo + (lonHi - lonLo) / 2], z)
      const y = ey.y - coords.y * size.y
      const isMajor = Math.abs(lat / major - Math.round(lat / major)) < 1e-9
      ctx.strokeStyle = isMajor ? 'rgba(28,36,51,0.18)' : 'rgba(28,36,51,0.09)'
      ctx.beginPath()
      ctx.moveTo(0, y)
      ctx.lineTo(size.x, y)
      ctx.stroke()
      if (isMajor) {
        ctx.fillStyle = 'rgba(28,36,51,0.55)'
        ctx.fillText(lat.toFixed(3), 4, y + 8)
      }
    }

    return canvas
  }
}

export default function Graticule() {
  const map = useMap()

  useLayoutEffect(() => {
    const layer = new GraticuleLayer({ tileSize: 256, minZoom: 2, maxZoom: 20 })
    layer.addTo(map)
    return () => {
      map.removeLayer(layer)
    }
  }, [map])

  return null
}