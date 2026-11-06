import { createContext, useContext, useState } from 'react'
import type { ReactNode } from 'react'
import { TileLayer } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import Graticule from './Graticule'

export type BaseMapKind = 'satellite' | 'topo' | 'hybrid' | 'osm'

export interface BaseMapConfig {
  url: string
  attribution: string
  maxZoom: number
}

// Mapas base disponibles. Se seleccionan en vivo desde el selector del topbar.
export const BASE_MAPS: Record<BaseMapKind, BaseMapConfig> = {
  satellite: {
    url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
    attribution: 'Imagery © Esri, Maxar, Earthstar Geographics',
    maxZoom: 19,
  },
  topo: {
    url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}',
    attribution: 'Tiles © Esri — World Topo Map',
    maxZoom: 19,
  },
  hybrid: {
    url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
    attribution: 'Imagery © Esri, Maxar, Earthstar Geographics',
    maxZoom: 19,
  },
  osm: {
    url: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
    attribution: '© OpenStreetMap contributors',
    maxZoom: 19,
  },
}

export const BASE_MAP_LABELS: Record<BaseMapKind, string> = {
  satellite: 'Satelital',
  topo: 'Topográfico',
  hybrid: 'Híbrido',
  osm: 'Calles (OSM)',
}

// Overlay de etiquetas / nombres de lugares para el modo híbrido (satelital + texto).
const LABELS_URL =
  'https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}'

interface MapBaseContextValue {
  kind: BaseMapKind
  setKind: (k: BaseMapKind) => void
}

const MapBaseContext = createContext<MapBaseContextValue>({ kind: 'osm', setKind: () => {} })

export function useMapBase() {
  return useContext(MapBaseContext)
}

export function MapBaseProvider({ children }: { children: ReactNode }) {
  const [kind, setKind] = useState<BaseMapKind>('osm')
  return (
    <MapBaseContext.Provider value={{ kind, setKind }}>{children}</MapBaseContext.Provider>
  )
}

// Capa base real (satelital, topográfica o híbrida). La capa Graticule se
// mantiene debajo como fondo de emergencia: si las tiles no cargan (sin
// conexión), la grilla sigue siendo visible y el mapa nunca queda en negro.
export default function BaseMap() {
  const { kind } = useMapBase()
  const cfg = BASE_MAPS[kind]
  const hybrid = kind === 'hybrid'
  return (
    <>
      <Graticule />
      <TileLayer url={cfg.url} attribution={cfg.attribution} maxZoom={cfg.maxZoom} />
      {hybrid && (
        <TileLayer url={LABELS_URL} attribution="Labels © Esri" maxZoom={19} opacity={1} />
      )}
    </>
  )
}
