import { useEffect } from 'react'
import { tripsApi } from '../../../core/api'
import { usePaged } from '../../../core/hooks/usePaged'
import { useSignalR } from '../../../core/hooks/useSignalR'
import type { Geofence } from '../../../core/api/geofences'
import type { RouteItem } from '../../../core/api/routes'
import type { TripSummary } from '../../../core/api/trips'
import type { VehicleStatus } from '../../../core/api/vehicles'
import Pagination from '../../../components/ui/Pagination'
import TripTable from './TripTable'
import TripLiveMap from './TripLiveMap'

interface TripLiveTabProps {
  trips: TripSummary[]
  vehicles: VehicleStatus[]
  geofences: Geofence[]
  routes: RouteItem[]
  loadingTrips?: boolean
  refreshKey: number
  onOpen: (id: string) => void
  onEdit: (trip: TripSummary) => void
  onCancel: (trip: TripSummary) => void
}

export default function TripLiveTab({
  trips,
  vehicles,
  geofences,
  routes,
  loadingTrips,
  refreshKey,
  onOpen,
  onEdit,
  onCancel,
}: TripLiveTabProps) {
  const { dataVersion } = useSignalR()
  const list = usePaged<TripSummary>({
    fetch: (params) => tripsApi.list(params),
    pageSize: 20,
    filters: { Status: 'Active' },
  })

  useEffect(() => {
    list.reload()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataVersion, refreshKey])

  return (
    <div>
      <TripLiveMap
        trips={trips}
        vehicles={vehicles}
        geofences={geofences}
        routes={routes}
        loading={loadingTrips}
        onSelectTrip={onOpen}
      />

      <div className="table-toolbar">
        <h3 className="table-title">
          En curso
          <span className="mono">({list.totalCount})</span>
        </h3>
      </div>

      <TripTable
        trips={list.items}
        loading={list.loading}
        error={list.error}
        empty="No hay viajes en curso."
        onOpen={onOpen}
        onEdit={onEdit}
        onCancel={onCancel}
      />

      <Pagination
        page={list.page}
        totalPages={list.totalPages}
        totalCount={list.totalCount}
        pageSize={list.pageSize}
        onPageChange={list.setPage}
      />
    </div>
  )
}