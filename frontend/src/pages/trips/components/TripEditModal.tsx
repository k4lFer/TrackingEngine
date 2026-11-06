import { useMemo, useState } from 'react'
import { materialsApi, tripsApi } from '../../../core/api'
import { useApi } from '../../../core/hooks/useApi'
import { useMutation } from '../../../core/hooks/useMutation'
import type { Material } from '../../../core/api/materials'
import type { TripSummary, UpdateTripInput } from '../../../core/api/trips'
import Modal from '../../../components/ui/Modal'

interface TripEditModalProps {
  trip: TripSummary
  onClose: () => void
  onSaved: () => void
}

export default function TripEditModal({ trip, onClose, onSaved }: TripEditModalProps) {
  const materials = useApi<Material[]>(() =>
    materialsApi.list({ NumberPage: 1, PageSize: 500 }),
  )

  const materialId = useMemo(
    () => (materials.data ?? []).find((m) => m.name === trip.materialName)?.id ?? '',
    [materials.data, trip.materialName],
  )

  const [form, setForm] = useState(() => ({
    materialId,
    loadTonnes: trip.loadTonnes != null ? String(trip.loadTonnes) : '',
  }))

  const mutation = useMutation(() =>
    tripsApi.update(trip.id, {
      materialId: form.materialId || null,
      loadTonnes: form.loadTonnes === '' ? null : Number(form.loadTonnes),
    } as UpdateTripInput),
  )

  const activeMaterials = (materials.data ?? []).filter((m) => m.active)

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal title={`Editar viaje · ${trip.vehicleCode}`} onClose={onClose}>
      <form onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}
        <div className="field">
          <label>Material</label>
          <select
            value={form.materialId}
            onChange={(e) => setForm((f) => ({ ...f, materialId: e.target.value }))}
          >
            <option value="">—</option>
            {activeMaterials.map((m) => (
              <option key={m.id} value={m.id}>
                {m.code} · {m.name}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Carga (toneladas)</label>
          <input
            type="number"
            min={0}
            step="0.1"
            value={form.loadTonnes}
            onChange={(e) => setForm((f) => ({ ...f, loadTonnes: e.target.value }))}
            placeholder="0.0"
          />
        </div>
        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>Cancelar</button>
          <button type="submit" className="btn primary" disabled={mutation.saving}>
            {mutation.saving ? 'Guardando…' : 'Guardar'}
          </button>
        </div>
      </form>
    </Modal>
  )
}