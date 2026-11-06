import { useState } from 'react'
import { materialsApi } from '../../../core/api'
import { useMutation } from '../../../core/hooks/useMutation'
import type { Material, UpdateMaterialInput } from '../../../core/api/materials'
import Modal from '../../../components/ui/Modal'

interface MaterialFormModalProps {
  initial?: Material | null
  onClose: () => void
  onSaved: () => void
}

const EMPTY = { code: '', name: '', unit: '', active: true }

export default function MaterialFormModal({ initial, onClose, onSaved }: MaterialFormModalProps) {
  const editing = Boolean(initial)
  const [form, setForm] = useState(() =>
    initial
      ? { code: initial.code, name: initial.name, unit: initial.unit, active: initial.active }
      : EMPTY,
  )
  const mutation = useMutation(() =>
    initial
      ? materialsApi.update(
          initial.id,
          {
            name: form.name.trim(),
            unit: form.unit.trim(),
            active: form.active,
          } as UpdateMaterialInput,
        )
      : materialsApi.create({
          code: form.code.trim(),
          name: form.name.trim(),
          unit: form.unit.trim(),
        }),
  )

  const set = (k: keyof typeof form, v: string | boolean) =>
    setForm((f) => ({ ...f, [k]: v }))

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal title={editing ? 'Editar material' : 'Nuevo material'} onClose={onClose}>
      <form onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}
        <div className="field">
          <label>Código</label>
          <input
            value={form.code}
            onChange={(e) => set('code', e.target.value)}
            required={!editing}
            disabled={editing}
            placeholder="MAT-01"
          />
          {editing && <span className="hint">El código no se puede cambiar.</span>}
        </div>
        <div className="field">
          <label>Nombre</label>
          <input value={form.name} onChange={(e) => set('name', e.target.value)} required placeholder="Mineral de hierro" />
        </div>
        <div className="field">
          <label>Unidad</label>
          <input value={form.unit} onChange={(e) => set('unit', e.target.value)} required placeholder="ton" />
        </div>
        {editing && (
          <div className="field">
            <label className="check">
              <input type="checkbox" checked={form.active} onChange={(e) => set('active', e.target.checked)} />
              Material activo
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