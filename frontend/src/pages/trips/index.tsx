import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { tripsApi } from '../../core/api'
import { useApi } from '../../core/hooks/useApi'
import { useDashboardData } from '../../core/hooks/useDashboardData'
import { useSignalR } from '../../core/hooks/useSignalR'
import { useConfirm } from '../../core/hooks/useConfirm'
import { useToast } from '../../core/hooks/useToast'
import { summarizeMessages } from '../../core/api/http/alerts'
import type { TripSummary } from '../../core/api/trips'
import PageHeader from '../../components/ui/PageHeader'
import { PlusIcon } from '../../components/ui/Icons'
import TripLiveTab from './components/TripLiveTab'
import TripManageTab from './components/TripManageTab'
import TripHistoryTab from './components/TripHistoryTab'
import TripFormModal from './components/TripFormModal'
import TripEditModal from './components/TripEditModal'

type TripsTab = 'live' | 'manage' | 'history'

const TABS: { key: TripsTab; label: string }[] = [
  { key: 'live', label: 'En curso' },
  { key: 'manage', label: 'Administrar viaje' },
  { key: 'history', label: 'Historial' },
]

export default function Trips() {
  const navigate = useNavigate()
  const { dataVersion, connected } = useSignalR()
  const dashboard = useDashboardData(dataVersion)
  const [refreshKey, setRefreshKey] = useState(0)
  const activeTrips = useApi<TripSummary[]>(
    () => tripsApi.list({ NumberPage: 1, PageSize: 500, Status: 'Active' }),
    [dataVersion, refreshKey],
  )
  const confirm = useConfirm()
  const { notify } = useToast()
  const [tab, setTab] = useState<TripsTab>('live')
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<TripSummary | null>(null)

  const openTrip = (id: string) => navigate(`/trips/${id}`)

  const bump = () => setRefreshKey((k) => k + 1)

  const cancelTrip = async (trip: TripSummary) => {
    const ok = await confirm({
      title: 'Cancelar viaje',
      message: `¿Cancelar el viaje de ${trip.vehicleCode}?`,
      confirmLabel: 'Cancelar viaje',
      danger: true,
    })
    if (!ok) return
    try {
      const result = await tripsApi.cancel(trip.id)
      notify(summarizeMessages(result?.messages) ?? 'Viaje cancelado')
      bump()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Error al cancelar', 'error')
    }
  }

  return (
    <div>
      <PageHeader
        title="Viajes"
        subtitle="Recorridos, carga transportada y operación"
        actions={
          <>
            {connected ? <span className="badge green">En vivo</span> : null}
            <button className="btn primary" onClick={() => setShowCreate(true)}>
              <PlusIcon size={16} /> Nuevo viaje
            </button>
          </>
        }
      />

      <div className="tabs">
        {TABS.map((t) => (
          <button
            key={t.key}
            className={`tab ${tab === t.key ? 'active' : ''}`}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'live' && (
        <TripLiveTab
          trips={activeTrips.data ?? []}
          vehicles={dashboard.vehicles.data ?? []}
          geofences={dashboard.geofences.data ?? []}
          routes={dashboard.routes.data ?? []}
          loadingTrips={activeTrips.loading}
          refreshKey={refreshKey}
          onOpen={openTrip}
          onEdit={setEditing}
          onCancel={cancelTrip}
        />
      )}
      {tab === 'manage' && (
        <TripManageTab refreshKey={refreshKey} onOpen={openTrip} onEdit={setEditing} onCancel={cancelTrip} />
      )}
      {tab === 'history' && (
        <TripHistoryTab refreshKey={refreshKey} onOpen={openTrip} onEdit={setEditing} onCancel={cancelTrip} />
      )}

      {showCreate && (
        <TripFormModal
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false)
            notify('Viaje iniciado')
            bump()
          }}
        />
      )}

      {editing && (
        <TripEditModal
          trip={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null)
            notify('Viaje actualizado')
            bump()
          }}
        />
      )}
    </div>
  )
}