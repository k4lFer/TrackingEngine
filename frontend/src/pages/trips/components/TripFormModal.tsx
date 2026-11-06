import { useEffect, useMemo, useState } from 'react'
import { geofencesApi, materialsApi, routesApi, tripsApi, vehiclesApi } from '../../../core/api'
import { useApi } from '../../../core/hooks/useApi'
import { useMutation } from '../../../core/hooks/useMutation'
import type { Geofence } from '../../../core/api/geofences'
import type { Material } from '../../../core/api/materials'
import type { RouteItem } from '../../../core/api/routes'
import type { Vehicle } from '../../../core/api/vehicles'
import type { CreateTripInput } from '../../../core/api/trips'
import Modal from '../../../components/ui/Modal'

interface TripFormModalProps {
  onClose: () => void
  onSaved: () => void
}

const EMPTY = {
  vehicleId: '',
  originGeofenceId: '',
  destinationGeofenceId: '',
  routeId: '',
  materialId: '',
  loadTonnes: '',
}

function geofenceLabel(g: Geofence) {
  return `${g.code} · ${g.name}`
}

function materialLabel(m: Material) {
  return `${m.code} · ${m.name}`
}

export default function TripFormModal({ onClose, onSaved }: TripFormModalProps) {
  const [form, setForm] = useState({ ...EMPTY })

  const vehicles = useApi<Vehicle[]>(() =>
    vehiclesApi.list({ NumberPage: 1, PageSize: 500 }),
  )
  const geofences = useApi<Geofence[]>(() => geofencesApi.list())
  const routes = useApi<RouteItem[]>(() => routesApi.list())
  const materials = useApi<Material[]>(() =>
    materialsApi.list({ NumberPage: 1, PageSize: 500 }),
  )

  const mutation = useMutation(() =>
    tripsApi.create({
      vehicleId: form.vehicleId,
      originGeofenceId: form.originGeofenceId || null,
      destinationGeofenceId: form.destinationGeofenceId || null,
      routeId: form.routeId || null,
      materialId: form.materialId || null,
      loadTonnes: form.loadTonnes === '' ? null : Number(form.loadTonnes),
    } as CreateTripInput),
  )

  const set = (k: keyof typeof form, v: string) => setForm((f) => ({ ...f, [k]: v }))

  const activeVehicles = useMemo(() => (vehicles.data ?? []).filter((v) => v.active), [vehicles.data])
  const chooseableOrigin = (geofences.data ?? []).filter((g) => g.active)
  const chooseableDestination = chooseableOrigin.filter((g) => g.id !== form.originGeofenceId)

  const availableRoutes = useMemo(() => {
    const activeRoutes = (routes.data ?? []).filter((r) => r.active)
    const origin = form.originGeofenceId
    const dest = form.destinationGeofenceId
    if (!origin && !dest) return activeRoutes
    return activeRoutes.filter(
      (r) =>
        (!origin || r.originGeofenceId === origin) &&
        (!dest || r.destinationGeofenceId === dest),
    )
  }, [routes.data, form.originGeofenceId, form.destinationGeofenceId])

  useEffect(() => {
    if (form.routeId && !availableRoutes.some((r) => r.id === form.routeId)) {
      setForm((f) => ({ ...f, routeId: '' }))
    }
  }, [availableRoutes, form.routeId])

  const activeMaterials = useMemo(() => (materials.data ?? []).filter((m) => m.active), [materials.data])

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.vehicleId) return
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal title="Nuevo viaje" wide onClose={onClose}>
      <form onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}
        <div className="field">
          <label>Vehículo *</label>
          <select value={form.vehicleId} onChange={(e) => set('vehicleId', e.target.value)} required>
            <option value="">Selecciona un vehículo activo…</option>
            {activeVehicles.map((v) => (
              <option key={v.id} value={v.id}>
                {v.code} · {v.brand} {v.model} ({v.plate})
              </option>
            ))}
          </select>
        </div>
        <div className="grid-2">
          <div className="field">
            <label>Origen</label>
            <select value={form.originGeofenceId} onChange={(e) => set('originGeofenceId', e.target.value)}>
              <option value="">—</option>
              {chooseableOrigin.map((g) => (
                <option key={g.id} value={g.id}>
                  {geofenceLabel(g)}
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label>Destino</label>
            <select
              value={form.destinationGeofenceId}
              onChange={(e) => set('destinationGeofenceId', e.target.value)}
            >
              <option value="">—</option>
              {chooseableDestination.map((g) => (
                <option key={g.id} value={g.id}>
                  {geofenceLabel(g)}
                </option>
              ))}
            </select>
          </div>
        </div>
        <div className="grid-2">
          <div className="field">
            <label>Ruta</label>
            <select value={form.routeId} onChange={(e) => set('routeId', e.target.value)}>
              <option value="">—</option>
              {availableRoutes.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.code} · {r.name}
                </option>
              ))}
            </select>
            {!form.originGeofenceId && !form.destinationGeofenceId && (
              <span className="hint">Se sugiere elegir origen y destino para filtrar rutas.</span>
            )}
          </div>
          <div className="field">
            <label>Material</label>
            <select value={form.materialId} onChange={(e) => set('materialId', e.target.value)}>
              <option value="">—</option>
              {activeMaterials.map((m) => (
                <option key={m.id} value={m.id}>
                  {materialLabel(m)}
                </option>
              ))}
            </select>
          </div>
        </div>
        <div className="field">
          <label>Carga (toneladas)</label>
          <input
            type="number"
            min={0}
            step="0.1"
            value={form.loadTonnes}
            onChange={(e) => set('loadTonnes', e.target.value)}
            placeholder="0.0"
          />
        </div>
        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>Cancelar</button>
          <button type="submit" className="btn primary" disabled={mutation.saving || !form.vehicleId}>
            {mutation.saving ? 'Guardando…' : 'Iniciar viaje'}
          </button>
        </div>
      </form>
    </Modal>
  )
}