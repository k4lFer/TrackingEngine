import { useState } from 'react'
import { geofencesApi } from '../../../core/api'
import { useMapPoints } from '../../../core/hooks/useMapPoints'
import { useMutation } from '../../../core/hooks/useMutation'
import type { CreateGeofenceInput, Geofence, UpdateGeofenceInput } from '../../../core/api/geofences'
import { parseGeoJsonPolygon } from '../../../core/format'
import ClickMap from '../../../components/ClickMap'
import Modal from '../../../components/ui/Modal'
import { KINDS, KIND_LABELS, KIND_HINTS, FILL_COLOR, type GeofenceShape } from '../geofenceConstants'

interface GeofenceFormModalProps {
  initial?: Geofence | null
  onClose: () => void
  onSaved: () => void
  existing: Geofence[]
}

function seededPoints(initial: Geofence) {
  const ring = parseGeoJsonPolygon(initial.geoJsonPolygon)
  if (ring.length >= 2) {
    const first = ring[0]
    const last = ring[ring.length - 1]
    if (first.lat === last.lat && first.lon === last.lon) return ring.slice(0, -1)
  }
  return ring
}

function makeCirclePoints(clat: number, clon: number, r: number): { lat: number; lon: number }[] {
  const n = 48
  const pts: { lat: number; lon: number }[] = []
  for (let i = 0; i < n; i++) {
    const a = (i / n) * Math.PI * 2
    pts.push({ lat: clat + r * Math.sin(a), lon: clon + r * Math.cos(a) })
  }
  return pts
}

export default function GeofenceFormModal({ initial, onClose, onSaved, existing }: GeofenceFormModalProps) {
  const isEdit = Boolean(initial)
  const initialPoints = initial ? seededPoints(initial) : []
  const { points, addPoint, clear, set, movePoint, removePoint } = useMapPoints(initialPoints)
  const [closed, setClosed] = useState(initialPoints.length >= 3)
  const [shape, setShape] = useState<GeofenceShape>('polygon')
  const [form, setForm] = useState({
    code: initial?.code ?? '',
    name: initial?.name ?? '',
    kind: initial?.kind ?? 'Load',
    priority: initial?.priority ?? 10,
    maxSpeedKmh: initial?.maxSpeedKmh != null ? String(initial.maxSpeedKmh) : '',
    color: initial?.color || FILL_COLOR,
    active: initial?.active ?? true,
  })
  const mutation = useMutation(() => {
    const polygon =
      points.length > 0 &&
      points[points.length - 1].lat === points[0].lat &&
      points[points.length - 1].lon === points[0].lon
        ? points
        : [...points, points[0]]
    if (initial) {
      const payload: UpdateGeofenceInput = {
        name: form.name,
        kind: form.kind,
        priority: form.priority,
        polygon,
        maxSpeedKmh: form.maxSpeedKmh ? Number(form.maxSpeedKmh) : null,
        active: form.active,
      }
      return geofencesApi.update(initial.id, payload)
    }
    const payload: CreateGeofenceInput = {
      code: form.code,
      name: form.name,
      kind: form.kind,
      priority: form.priority,
      polygon,
      maxSpeedKmh: form.maxSpeedKmh ? Number(form.maxSpeedKmh) : null,
      color: form.color,
    }
    return geofencesApi.create(payload)
  })

  const near = (lat: number, lon: number, ref: { lat: number; lon: number }) =>
    Math.abs(lat - ref.lat) < 0.0015 && Math.abs(lon - ref.lon) < 0.0015

  const handleAdd = (lat: number, lon: number) => {
    if (closed) return
    if (shape === 'rectangle') {
      if (points.length === 0) {
        addPoint(lat, lon)
        return
      }
      const a = points[0]
      set([a, { lat: a.lat, lon }, { lat, lon }, { lat, lon: a.lon }])
      setClosed(true)
      return
    }
    if (shape === 'circle') {
      if (points.length === 0) {
        addPoint(lat, lon)
        return
      }
      const c = points[0]
      set(makeCirclePoints(c.lat, c.lon, Math.hypot(c.lat - lat, c.lon - lon)))
      setClosed(true)
      return
    }
    if (points.length >= 3 && near(lat, lon, points[0])) {
      setClosed(true)
      return
    }
    addPoint(lat, lon)
  }

  /** Clic sobre un vértice: tocar el 1° cierra/reabre el polígono (en lugar del popup). */
  const handleVertexClick = (index: number) => {
    if (shape !== 'polygon' || index !== 0) return
    if (closed) {
      setClosed(false)
    } else if (points.length >= 3) {
      setClosed(true)
    }
  }

  /** Clic derecho sobre un vértice: lo elimina y, si quedan <3, abre un polígono de menos de 3 puntos. */
  const handleRemovePoint = (index: number) => {
    if (shape !== 'polygon') return
    removePoint(index)
    if (points.length - 1 < 3 && closed) setClosed(false)
  }

  const changeShape = (next: GeofenceShape) => {
    setShape(next)
    setClosed(false)
    clear()
  }

  const resetDrawing = () => {
    setClosed(false)
    clear()
  }

  const requiredPoints = shape === 'circle' ? 48 : shape === 'rectangle' ? 4 : 3
  const canSave = closed && points.length >= requiredPoints

  const missing: string[] = []
  if (!isEdit && form.code.trim() === '') missing.push('el código')
  if (form.name.trim() === '') missing.push('el nombre')
  const canSubmit = !mutation.saving && canSave && missing.length === 0
  const blockedHint = missing.length > 0 ? `Falta ${missing.join(' y ')} para guardar.` : null

  const baseHint =
    shape === 'rectangle'
      ? closed
        ? 'Rectángulo finalizado'
        : points.length === 0
          ? 'Clic en la 1ª esquina, luego en la opuesta'
          : 'Clic en la esquina opuesta para cerrar el rectángulo'
      : shape === 'circle'
        ? closed
          ? 'Círculo finalizado'
          : points.length === 0
            ? 'Clic en el centro del círculo'
            : 'Clic para fijar el radio'
        : closed
          ? 'Polígono cerrado: clic en el 1er vértice para reabrirlo'
          : points.length >= 3
            ? 'Clic en el 1er punto para cerrar el polígono'
            : points.length === 0
              ? 'Clic en el mapa para añadir vértices'
              : 'Sigue añadiendo vértices (mín. 3)'

  const hint = shape === 'polygon' ? `${baseHint} · Clic derecho en un vértice lo elimina` : baseHint

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!canSubmit) return
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal
      title={isEdit ? 'Editar geocerca' : 'Nueva geocerca'}
      full
      onClose={onClose}
      side={
        <>
          <div className="modal-side-body">
            <div className="side-section-title">Identificación</div>
            <div className="field">
              <label>
                Código
                {!isEdit && <span aria-hidden="true" className="text-alert"> *</span>}
              </label>
              <input
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
                form="geofence-form"
                required={!isEdit}
                disabled={isEdit}
                placeholder="ZONA-01"
                className={!isEdit && form.code.trim() === '' ? 'invalid' : undefined}
              />
              {isEdit && <span className="hint">El código no se puede cambiar.</span>}
            </div>
            <div className="field">
              <label>
                Nombre
                <span aria-hidden="true" className="text-alert"> *</span>
              </label>
              <input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                form="geofence-form"
                required
                placeholder="Planta de acopio"
                className={form.name.trim() === '' ? 'invalid' : undefined}
              />
            </div>
            <div className="field">
              <label>Tipo</label>
              <select value={form.kind} onChange={(e) => setForm({ ...form, kind: e.target.value })}>
                {KINDS.map((k) => (
                  <option key={k} value={k} title={KIND_HINTS[k]}>{KIND_LABELS[k]}</option>
                ))}
              </select>
              <span className="hint">{KIND_HINTS[form.kind]}</span>
            </div>

            <div className="side-section-title">Comportamiento</div>
            <div className="field-row">
              <div className="field">
                <label>Prioridad</label>
                <input
                  type="number"
                  value={form.priority}
                  onChange={(e) => setForm({ ...form, priority: Number(e.target.value) })}
                />
              </div>
              <div className="field">
                <label>Vel. máx (km/h)</label>
                <input
                  value={form.maxSpeedKmh}
                  onChange={(e) => setForm({ ...form, maxSpeedKmh: e.target.value })}
                  placeholder="Opcional"
                />
              </div>
            </div>
            {isEdit && (
              <div className="field">
                <label className="check">
                  <input
                    type="checkbox"
                    checked={form.active}
                    onChange={(e) => setForm({ ...form, active: e.target.checked })}
                    form="geofence-form"
                  />
                  Geocerca activa
                </label>
              </div>
            )}

            <div className="side-section-title">Geometría y color</div>
            <div className="field-row">
              <div className="field">
                <label>Forma</label>
                <select value={shape} onChange={(e) => changeShape(e.target.value as GeofenceShape)} disabled={isEdit}>
                  <option value="polygon">Poligonal</option>
                  <option value="rectangle">Rectángulo</option>
                  <option value="circle">Círculo</option>
                </select>
                {isEdit && <span className="hint">En edición la forma siempre es poligonal.</span>}
              </div>
              <div className="field">
                <label>Color</label>
                <input
                  type="color"
                  value={form.color}
                  onChange={(e) => setForm({ ...form, color: e.target.value })}
                  style={{ padding: 2, height: 34 }}
                  disabled={isEdit}
                />
              </div>
            </div>
          </div>

          {blockedHint && (
            <p className="mx-[18px] mb-2.5 text-[12px] leading-[1.4] text-ink-dim">{blockedHint}</p>
          )}

          <div className="modal-side-actions">
            <button type="button" className="btn" onClick={onClose}>Cancelar</button>
            <button
              type="submit"
              form="geofence-form"
              className="btn primary"
              disabled={!canSubmit}
            >
              {mutation.saving ? 'Guardando…' : isEdit ? 'Guardar' : 'Crear geocerca'}
            </button>
          </div>
        </>
      }
    >
      <form id="geofence-form" onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}

        <div className="polygon-editor">
          <ClickMap
            points={points}
            onAdd={handleAdd}
            onMovePoint={movePoint}
            onRemovePoint={handleRemovePoint}
            onVertexClick={handleVertexClick}
            color={form.color}
            shape="polygon"
            closed={closed}
            existing={existing}
          />
          <div className="polygon-meta">
            <span className="hint">{hint}</span>
            {shape === 'polygon' && closed && points.length >= 3 ? (
              <button
                type="button"
                className="btn"
                onClick={() => setClosed(false)}
                title="Seguir añadiendo vértices"
              >
                Deshacer cierre
              </button>
            ) : (
              <span className="mono">
                {shape === 'rectangle' && closed
                  ? 'rectángulo dibujado'
                  : shape === 'circle' && closed
                    ? 'círculo dibujado'
                    : shape === 'polygon'
                      ? `${points.length} vértices`
                      : `${points.length} punto(s)`}
              </span>
            )}
            <button type="button" className="btn" onClick={resetDrawing} disabled={points.length === 0}>
              Reiniciar dibujo
            </button>
          </div>
        </div>
      </form>
    </Modal>
  )
}