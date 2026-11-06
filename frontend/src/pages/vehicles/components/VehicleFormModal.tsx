import { useState } from 'react'
import { vehiclesApi } from '../../../core/api'
import { useMutation } from '../../../core/hooks/useMutation'
import type { UpdateVehicleInput, CreateVehicleInput, Vehicle } from '../../../core/api/vehicles'
import { DEVICE_KIND_LABELS } from '../../../core/api/vehicles'
import Modal from '../../../components/ui/Modal'

interface VehicleFormModalProps {
  initial?: Vehicle | null
  onClose: () => void
  onSaved: () => void
}

interface VehicleForm {
  code: string
  plate: string
  brand: string
  model: string
  active: boolean
  deviceIdentifier: string
  deviceKind: string
  deviceModel: string
}

const EMPTY: VehicleForm = {
  code: '',
  plate: '',
  brand: '',
  model: '',
  active: true,
  deviceIdentifier: '',
  deviceKind: '1',
  deviceModel: '',
}

export default function VehicleFormModal({ initial, onClose, onSaved }: VehicleFormModalProps) {
  const editing = Boolean(initial)
  const [form, setForm] = useState<VehicleForm>(() =>
    initial
      ? {
          code: initial.code,
          plate: initial.plate,
          brand: initial.brand,
          model: initial.model,
          active: initial.active,
          deviceIdentifier: initial.deviceIdentifier ?? '',
          deviceKind: initial.deviceKind == null ? '1' : String(initial.deviceKind),
          deviceModel: initial.deviceModel ?? '',
        }
      : EMPTY,
  )
  const body = (): UpdateVehicleInput | CreateVehicleInput => {
    const identifier = form.deviceIdentifier.trim()
    return {
      code: form.code.trim(),
      plate: form.plate.trim(),
      brand: form.brand.trim(),
      model: form.model.trim(),
      active: form.active,
      deviceIdentifier: identifier === '' ? null : identifier,
      deviceKind: identifier === '' ? null : Number.parseInt(form.deviceKind, 10),
      deviceModel: identifier === '' || form.deviceModel.trim() === '' ? null : form.deviceModel.trim(),
    }
  }

  const mutation = useMutation(() =>
    initial
      ? vehiclesApi.update(initial.id, body() as UpdateVehicleInput)
      : vehiclesApi.create(body() as CreateVehicleInput),
  )

  const set = (k: keyof VehicleForm, v: string | boolean) =>
    setForm((f) => ({ ...f, [k]: v }))

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal title={editing ? 'Editar vehículo' : 'Nuevo vehículo'} onClose={onClose}>
      <form onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}
        <div className="field">
          <label>Código</label>
          <input
            value={form.code}
            onChange={(e) => set('code', e.target.value)}
            required={!editing}
            disabled={editing}
            placeholder="CAM-01"
          />
          {editing && <span className="hint">El código no se puede cambiar.</span>}
        </div>
        <div className="field">
          <label>Placa</label>
          <input value={form.plate} onChange={(e) => set('plate', e.target.value)} required placeholder="ABC-123" />
        </div>
        <div className="field">
          <label>Marca</label>
          <input value={form.brand} onChange={(e) => set('brand', e.target.value)} required placeholder="Volvo" />
        </div>
        <div className="field">
          <label>Modelo</label>
          <input value={form.model} onChange={(e) => set('model', e.target.value)} required placeholder="FH16" />
        </div>
        <div className="field">
          <label>Tipo de dispositivo</label>
          <select value={form.deviceKind} onChange={(e) => set('deviceKind', e.target.value)}>
            <option value="0">Sin dispositivo</option>
            {Object.entries(DEVICE_KIND_LABELS).map(([value, label]) => (
              <option key={value} value={value}>{label}</option>
            ))}
          </select>
          <span className="hint">
            El identificador y el modelo se habilitan al elegir un tipo de rastreo.
          </span>
        </div>

        {form.deviceKind !== '0' && form.deviceKind !== '' && (
          <div className="grid-2">
            <div className="field">
              <label>Identificador del dispositivo (tracker)</label>
              <input
                value={form.deviceIdentifier}
                onChange={(e) => set('deviceIdentifier', e.target.value)}
                placeholder="IMEI o id del Traccar Client (ej. 67507852)"
              />
              <span className="hint">
                Código con el que reporta el dispositivo; vacío = sin enlazar.
              </span>
            </div>
            <div className="field">
              <label>Modelo de hardware</label>
              <input
                value={form.deviceModel}
                onChange={(e) => set('deviceModel', e.target.value)}
                placeholder="Traccar Android, ST-901…"
              />
              <span className="hint">Opcional.</span>
            </div>
          </div>
        )}
        {editing && (
          <div className="field">
            <label className="check">
              <input type="checkbox" checked={form.active} onChange={(e) => set('active', e.target.checked)} />
              Vehículo activo
            </label>
          </div>
        )}
        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>Cancelar</button>
          <button type="submit" className="btn primary" disabled={mutation.saving}>
            {mutation.saving ? 'Guardando…' : editing ? 'Guardar' : 'Crear'}
          </button>
        </div>
      </form>
    </Modal>
  )
}