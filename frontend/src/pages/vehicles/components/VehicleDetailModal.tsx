import { vehiclesApi, tripsApi } from '../../../core/api'
import { useApi } from '../../../core/hooks/useApi'
import { formatDateTime, formatDistance } from '../../../core/format'
import type { Vehicle } from '../../../core/api/vehicles'
import type { TripSummary } from '../../../core/api/trips'
import DataTable from '../../../components/ui/DataTable'
import Modal from '../../../components/ui/Modal'

const STATUS_BADGE: Record<string, string> = {
  Active: 'green',
  Completed: 'blue',
  Cancelled: 'gray',
}

interface VehicleDetailModalProps {
  vehicleId: string
  onClose: () => void
}

/**
 * Detalle de un vehículo (datos + estado) con su historial de viajes.
 * Ambos datos llegan de endpoints propios: /vehicles/{id} y /trips/vehicle/{id}.
 */
export default function VehicleDetailModal({ vehicleId, onClose }: VehicleDetailModalProps) {
  const vehicle = useApi<Vehicle>(() => vehiclesApi.getById(vehicleId), [vehicleId])
  const trips = useApi<TripSummary[]>(() => tripsApi.byVehicle(vehicleId), [vehicleId])

  const v = vehicle.data

  return (
    <Modal title={v ? `${v.code} · ${v.plate}` : 'Vehículo'} wide onClose={onClose}>
      {vehicle.loading ? (
        <p className="muted">Cargando vehículo…</p>
      ) : v ? (
        <div className="field-row" style={{ marginBottom: 20 }}>
          <div className="field">
            <label>Placa</label>
            <div className="mono">{v.plate}</div>
          </div>
          <div className="field">
            <label>Marca / Modelo</label>
            <div className="mono">
              {v.brand} {v.model}
            </div>
          </div>
          <div className="field">
            <label>Estado</label>
            <span className={`badge ${v.active ? 'green' : 'gray'}`}>
              {v.active ? 'Activo' : 'Inactivo'}
            </span>
          </div>
        </div>
      ) : (
        <p className="muted">No se pudo cargar el vehículo.</p>
      )}

      <div className="side-section-title">Historial de viajes</div>
      <DataTable
        columns={['Origen → Destino', 'Material', 'Inicio', 'Distancia', 'Estado']}
        loading={trips.loading}
        error={trips.error}
        rows={(trips.data ?? []).length}
        empty="Este vehículo no tiene viajes registrados."
      >
        {(trips.data ?? []).map((t) => (
          <tr key={t.id}>
            <td>
              {t.originName ?? t.routeCode ?? '—'} → {t.destinationName ?? '—'}
            </td>
            <td>{t.materialName ?? '—'}</td>
            <td className="mono">{formatDateTime(t.startedAt)}</td>
            <td className="mono">{formatDistance(t.distanceKm)}</td>
            <td>
              <span className={`badge ${STATUS_BADGE[t.status] ?? 'gray'}`}>
                {t.status === 'Active' ? 'En curso' : t.status}
              </span>
            </td>
          </tr>
        ))}
      </DataTable>
    </Modal>
  )
}