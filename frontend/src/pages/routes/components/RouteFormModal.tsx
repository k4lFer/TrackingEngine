import { useEffect, useMemo, useState } from 'react'
import { routesApi } from '../../../core/api'
import { useMutation } from '../../../core/hooks/useMutation'
import type { CreateRouteInput, RouteItem, UpdateRouteInput } from '../../../core/api/routes'
import type { Geofence } from '../../../core/api/geofences'
import Modal from '../../../components/ui/Modal'
import PathEditor, { type PathEditorState } from '../../../components/PathEditor'
import RouteFormSide from './RouteFormSide'
import { formFrom, pointsEq, seedFrom, type RouteForm } from './routeFormHelpers'

const LINE_COLOR = '#5aa2e8'

interface RouteFormModalProps {
  initial?: RouteItem | null
  geofences: Geofence[]
  onClose: () => void
  onSaved: () => void
}

export default function RouteFormModal({ initial, geofences, onClose, onSaved }: RouteFormModalProps) {
  const isEdit = Boolean(initial)
  const seed = useMemo(() => seedFrom(initial), [initial])
  const [form, setForm] = useState<RouteForm>(() => formFrom(initial))
  const [path, setPath] = useState<PathEditorState>(seed)
  const unchanged = pointsEq(path.points, seed.points)
  const [preview, setPreview] = useState<RouteItem | null>(null)
  const [previewLoading, setPreviewLoading] = useState(seed.points.length >= 2)
  const [noRouteDialog, setNoRouteDialog] = useState(false)
  const mutation = useMutation(() => {
    const base = {
      name: form.name,
      waypoints: path.points,
      toleranceM: form.toleranceM,
      maxSpeedKmh: form.maxSpeedKmh ? Number(form.maxSpeedKmh) : null,
      alternativesCount: form.alternativesCount,
      originGeofenceId: path.startGeofenceId || null,
      destinationGeofenceId: path.endGeofenceId || null,
    }
    if (initial) {
      const payload: UpdateRouteInput = { ...base, active: form.active }
      return routesApi.update(initial.id, payload)
    }
    const payload: CreateRouteInput = { code: form.code, ...base }
    return routesApi.create(payload)
  })

  const requestedAlts = form.alternativesCount
  const foundAlts = (preview?.alternatives ?? []).length

  /** La preview es un trazado real por vías; "none" = fallback en línea recta (zona inaccesible). */
  const usablePreview = preview != null && preview.provider !== 'none'
  const noRoute = path.points.length >= 2 && !previewLoading && (preview == null || !usablePreview)
  const pendingCheck = path.points.length >= 2 && previewLoading && !preview

  const altNotice = useMemo(() => {
    if (requestedAlts <= 0 || !preview || !usablePreview) return null
    if (preview.provider === 'mine') {
      return 'Este trazado usa la red interna de la mina: no se generan alternativas en este tramo.'
    }
    if (foundAlts < requestedAlts) {
      return foundAlts === 0
        ? 'No hay trayectos alternos para este trazado: solo se guardará la ruta principal.'
        : `Solo se encontraron ${foundAlts} de ${requestedAlts} alternativas posibles.`
    }
    return null
  }, [requestedAlts, foundAlts, preview, usablePreview])

  const routeGeo = usablePreview
    ? preview!.geoJsonLine
    : unchanged && initial && preview == null
      ? initial.geoJsonLine
      : undefined

  const previewStatus = noRoute ? (
    <span className="hint" style={{ color: '#c0392b', fontWeight: 600 }}>
      ⚠ Zona no accesible: no hay una vía que conecte estos puntos.
    </span>
  ) : pendingCheck ? (
    <span className="hint">Validando trazado…</span>
  ) : null

  useEffect(() => {
    if (path.points.length < 2) {
      setPreview(null)
      setPreviewLoading(false)
      return
    }
    setPreviewLoading(true)
    const timer = setTimeout(async () => {
      const res = await routesApi
        .preview({
          waypoints: path.points,
          toleranceM: form.toleranceM,
          maxSpeedKmh: form.maxSpeedKmh ? Number(form.maxSpeedKmh) : null,
          alternativesCount: form.alternativesCount,
        })
        .catch(() => null)
      setPreview(res?.data ?? null)
      setPreviewLoading(false)
    }, 350)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [path.points, path.startGeofenceId, path.endGeofenceId, form.toleranceM, form.maxSpeedKmh, form.alternativesCount])

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (noRoute) {
      setNoRouteDialog(true)
      return
    }
    const res = await mutation.run()
    if (res.ok) onSaved()
  }

  return (
    <Modal
      title={isEdit ? 'Editar ruta' : 'Nueva ruta'}
      full
      onClose={onClose}
      side={
        <RouteFormSide
          isEdit={isEdit}
          form={form}
          setForm={setForm}
          preview={preview}
          previewLoading={previewLoading}
          usablePreview={usablePreview}
          noRoute={noRoute}
          altNotice={altNotice}
          pendingCheck={pendingCheck}
          pointCount={path.points.length}
          saving={mutation.saving}
          onClose={onClose}
        />
      }
    >
      <form id="route-form" onSubmit={submit}>
        {mutation.error && <div className="error-box">{mutation.error}</div>}

        <PathEditor
          geofences={geofences}
          color={LINE_COLOR}
          startLabel="Geocerca de origen"
          endLabel="Geocerca de destino"
          onChange={setPath}
          initial={seed}
          preview={routeGeo}
          variants={usablePreview ? (preview?.alternatives ?? []).map((a) => a.geoJsonLine).filter((g): g is string => Boolean(g)) : []}
          previewStatus={previewStatus}
        />
      </form>

      {noRouteDialog && (
        <Modal title="No se pudo trazar la ruta" onClose={() => setNoRouteDialog(false)}>
          <div className="text-[13.5px] leading-relaxed text-ink-dim pb-1">
            No se encontró una <b>ruta válida y accesible</b> para los puntos seleccionados: el trazado
            sería solo una línea recta entre ellos (zona sin vías accesibles).
            <br />
            Selecciona puntos sobre una vía de la mina o ajusta el recorrido e inténtalo de nuevo.
          </div>
          <div className="flex justify-end gap-2.5 mt-4">
            <button
              type="button"
              className="btn primary"
              autoFocus
              onClick={() => setNoRouteDialog(false)}
            >
              Entendido
            </button>
          </div>
        </Modal>
      )}
    </Modal>
  )
}
