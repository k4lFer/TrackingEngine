import { useEffect, useState, type ReactNode } from 'react'
import type { CoordinateDto } from '../core/api/common'
import type { Geofence } from '../core/api/geofences'
import { parseGeoJsonPolygon, polygonCentroid } from '../core/format'
import { useGeofenceRoutePoints } from '../core/hooks/useGeofenceRoutePoints'
import ClickMap from './ClickMap'

export interface PathEditorState {
  points: CoordinateDto[]
  startGeofenceId: string
  endGeofenceId: string
}

interface PathEditorProps {
  geofences: Geofence[]
  color?: string
  startLabel?: string
  endLabel?: string
  minPoints?: number
  initial?: PathEditorState
  preview?: string
  previewStatus?: ReactNode
  variants?: string[]
  onChange: (state: PathEditorState) => void
}

/**
 * Editor de geometrías lineales para rutas: permite dibujar con clics,
 * fijar extremos desde geocercas y arrastrar los puntos intermedios.
 */
export default function PathEditor({
  geofences,
  color = '#e09b3d',
  startLabel = 'Geocerca de inicio',
  endLabel = 'Geocerca de destino',
  minPoints = 2,
  initial,
  preview,
  previewStatus,
  variants = [],
  onChange,
}: PathEditorProps) {
  const { points, addPoint, setOrigin, setDest, removeLast, removePoint, movePoint } = useGeofenceRoutePoints(initial?.points ?? [])
  const [startGeofenceId, setStartGeofenceId] = useState(initial?.startGeofenceId ?? '')
  const [endGeofenceId, setEndGeofenceId] = useState(initial?.endGeofenceId ?? '')

  const activeGeofences = geofences.filter((g) => g.active)
  const endpointsLocked = Boolean(startGeofenceId || endGeofenceId)

  useEffect(() => {
    onChange({ points, startGeofenceId, endGeofenceId })
    // La única intención es propagar los cambios de estado al padre.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [points, startGeofenceId, endGeofenceId])

  const onStartChange = (id: string) => {
    if (!id) {
      setOrigin(null)
      setStartGeofenceId('')
      return
    }
    const geo = activeGeofences.find((g) => String(g.id) === id)
    const c = geo ? polygonCentroid(parseGeoJsonPolygon(geo.geoJsonPolygon)) : null
    setOrigin(c)
    setStartGeofenceId(String(geo?.id ?? ''))
  }

  const onEndChange = (id: string) => {
    if (!id) {
      setDest(null)
      setEndGeofenceId('')
      return
    }
    const geo = activeGeofences.find((g) => String(g.id) === id)
    const c = geo ? polygonCentroid(parseGeoJsonPolygon(geo.geoJsonPolygon)) : null
    setDest(c)
    setEndGeofenceId(String(geo?.id ?? ''))
  }

  return (
    <>
      <div className="field-row" style={{ marginBottom: 8 }}>
        <div className="field">
          <label>{startLabel}</label>
          <select value={startGeofenceId} onChange={(e) => onStartChange(e.target.value)}>
            <option value="">— A mano (clic en el mapa) —</option>
            {activeGeofences.map((g) => (
              <option key={g.id} value={g.id}>
                {g.name} ({g.kind})
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>{endLabel}</label>
          <select value={endGeofenceId} onChange={(e) => onEndChange(e.target.value)}>
            <option value="">— A mano (clic en el mapa) —</option>
            {activeGeofences.map((g) => (
              <option key={g.id} value={g.id}>
                {g.name} ({g.kind})
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="polygon-editor">
        <ClickMap
          points={points}
          onAdd={addPoint}
          color={color}
          shape="line"
          existing={activeGeofences}
          lockEnds={endpointsLocked}
          preview={preview}
          variants={variants}
          onMovePoint={movePoint}
          onRemovePoint={removePoint}
        />
        <div className="polygon-meta">
          <span className="mono">
            {points.length} puntos
            {endpointsLocked ? ' · extremos fijos por geocerca' : ''}
          </span>
          {endpointsLocked && (
            <span className="hint">Clic en el mapa añade intermedios; arrástralos para ajustar</span>
          )}
          {points.length > 0 && points.length < minPoints && (
            <span className="hint">Necesitas al menos {minPoints} puntos.</span>
          )}
          {points.length > 0 && (
            <span className="hint">Clic derecho sobre un punto lo elimina</span>
          )}
          {previewStatus}
          <button type="button" className="btn" onClick={removeLast} disabled={points.length === 0}>
            Quitar último
          </button>
        </div>
      </div>
    </>
  )
}