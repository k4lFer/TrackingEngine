import { useEffect, useState } from 'react'
import { tripsApi } from '../../../core/api'
import { usePaged } from '../../../core/hooks/usePaged'
import { useSignalR } from '../../../core/hooks/useSignalR'
import type { TripSummary } from '../../../core/api/trips'
import Pagination from '../../../components/ui/Pagination'
import { SearchIcon } from '../../../components/ui/Icons'
import TripTable from './TripTable'

const STATUS_FILTERS: { value: string; label: string }[] = [
  { value: '', label: 'Todos los estados' },
  { value: 'Active', label: 'En curso' },
  { value: 'Completed', label: 'Completados' },
  { value: 'Cancelled', label: 'Cancelados' },
  { value: 'Incomplete', label: 'Incompletos' },
]

interface TripManageTabProps {
  refreshKey: number
  onOpen: (id: string) => void
  onEdit: (trip: TripSummary) => void
  onCancel: (trip: TripSummary) => void
}

export default function TripManageTab({ refreshKey, onOpen, onEdit, onCancel }: TripManageTabProps) {
  const { dataVersion } = useSignalR()
  const [statusFilter, setStatusFilter] = useState('')
  const list = usePaged<TripSummary>({
    fetch: (params) => tripsApi.list(params),
    pageSize: 20,
    filters: statusFilter ? { Status: statusFilter } : {},
  })

  useEffect(() => {
    list.reload()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataVersion, refreshKey])

  return (
    <div>
      <div className="table-toolbar">
        <h3 className="table-title">
          Todos los viajes
          <span className="mono">({list.totalCount})</span>
        </h3>
        <div className="toolbar-controls">
          <select
            className="filter-select"
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value)
              list.setPage(1)
            }}
          >
            {STATUS_FILTERS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <div className="search-box">
            <SearchIcon size={14} />
            <input
              className="search-input"
              placeholder="Buscar por unidad, ruta o material…"
              value={list.search}
              onChange={(e) => list.setSearch(e.target.value)}
            />
          </div>
        </div>
      </div>

      <TripTable
        trips={list.items}
        loading={list.loading}
        error={list.error}
        empty="No hay viajes registrados."
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