import type { RouteItem } from '../../../core/api/routes'
import { formatDistance, formatDuration } from '../../../core/format'
import type { RouteForm } from './routeFormHelpers'

interface RouteFormSideProps {
  isEdit: boolean
  form: RouteForm
  setForm: (form: RouteForm) => void
  preview: RouteItem | null
  previewLoading: boolean
  usablePreview: boolean
  noRoute: boolean
  altNotice: string | null
  pendingCheck: boolean
  pointCount: number
  saving: boolean
  onClose: () => void
}

export default function RouteFormSide({
  isEdit,
  form,
  setForm,
  preview,
  previewLoading,
  usablePreview,
  noRoute,
  altNotice,
  pendingCheck,
  pointCount,
  saving,
  onClose,
}: RouteFormSideProps) {
  return (
    <>
      <div className="modal-side-body">
        <div className="side-section-title">Detalles</div>
        <div className="field">
          <label>Código</label>
          <input
            value={form.code}
            onChange={(e) => setForm({ ...form, code: e.target.value })}
            form="route-form"
            required={!isEdit}
            disabled={isEdit}
            placeholder="RUTA-01"
          />
          {isEdit && <span className="hint">El código no se puede cambiar.</span>}
        </div>
        <div className="field">
          <label>Nombre</label>
          <input
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            form="route-form"
            required
            placeholder="Mina → Planta"
          />
        </div>
        <div className="field-row">
          <div className="field">
            <label>Tolerancia (m)</label>
            <input
              type="number"
              value={form.toleranceM}
              onChange={(e) => setForm({ ...form, toleranceM: Number(e.target.value) })}
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
        <div className="field">
          <label>Alternativas</label>
          <select
            value={form.alternativesCount}
            onChange={(e) => setForm({ ...form, alternativesCount: Number(e.target.value) })}
          >
            <option value={0}>Sin alternativas</option>
            <option value={1}>1 alternativa</option>
            <option value={2}>2 alternativas</option>
          </select>
          <span className="hint">
            Si no se encuentran, se guardan solo las posibles (la principal siempre).
          </span>
        </div>
        {isEdit && (
          <div className="field">
            <label className="check">
              <input
                type="checkbox"
                checked={form.active}
                onChange={(e) => setForm({ ...form, active: e.target.checked })}
                form="route-form"
              />
              Ruta activa
            </label>
          </div>
        )}
        {preview && usablePreview && (
          <div className="preview-box">
            <span className="side-section-title">Ruta calculada</span>
            <div className="mono">
              {formatDistance(preview.distanceKm)}
              {preview.estimatedDurationS > 0 && <> · {formatDuration(preview.estimatedDurationS)}</>}
              {previewLoading && ' …'}
            </div>
            {(preview.alternatives ?? []).map((a) => (
              <div key={a.alternativeRank} className="mono muted" style={{ fontSize: 12, lineHeight: 1.5 }}>
                ⤷ {a.name} · {formatDistance(a.distanceKm)}
              </div>
            ))}
            {altNotice && (
              <div className="mono muted" style={{ fontSize: 12, lineHeight: 1.5, marginTop: 6 }}>
                {altNotice}
              </div>
            )}
          </div>
        )}
        {noRoute && (
          <div className="preview-box">
            <span className="side-section-title">Ruta calculada</span>
            <div className="mono" style={{ color: '#c0392b', fontSize: 12, lineHeight: 1.5 }}>
              ⚠ No hay una vía accesible entre estos puntos. El trazado solo sería una línea recta,
              por lo que no se puede guardar. Ajusta los puntos o elige otro sector.
            </div>
          </div>
        )}
        {!preview && !noRoute && previewLoading && pointCount >= 2 && (
          <div className="preview-box">
            <span className="side-section-title">Ruta calculada</span>
            <div className="mono">Calculando…</div>
          </div>
        )}
      </div>

      <div className="modal-side-actions">
        <button type="button" className="btn" onClick={onClose}>Cancelar</button>
        <button
          type="submit"
          form="route-form"
          className="btn primary"
          disabled={saving || pointCount < 2 || pendingCheck}
        >
          {saving ? 'Guardando…' : pendingCheck ? 'Validando trazado…' : isEdit ? 'Guardar' : 'Crear ruta'}
        </button>
      </div>
    </>
  )
}
