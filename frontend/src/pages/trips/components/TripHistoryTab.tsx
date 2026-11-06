import { useEffect, useState } from 'react'
import { tripsApi } from '../../../core/api'
import { usePaged } from '../../../core/hooks/usePaged'
import { useSignalR } from '../../../core/hooks/useSignalR'
import type { TripSummary } from '../../../core/api/trips'
import Pagination from '../../../components/ui/Pagination'
import { SearchIcon } from '../../../components/ui/Icons'
import TripTable from './TripTable'

const FINISHED = 'Completed,Cancelled,Incomplete'

const STATUS_FILTERS: { value: string; label: string }[] = [
  { value: FINISHED, label: 'Concluidos' },
  { value: 'Completed', label: 'Completados' },
  { value: 'Cancelled', label: 'Cancelados' },
  { value: 'Incomplete', label: 'Incompletos' },
]

function isoDate(daysAgo: number): string {
  return new Date(Date.now() - daysAgo * 86400000).toISOString().slice(0, 10)
}

function toISOStart(date: string): string | undefined {
  return date ? new Date(`${date}T00:00:00`).toISOString() : undefined
}

function toISOEnd(date: string): string | undefined {
  return date ? new Date(`${date}T23:59:59.999`).toISOString() : undefined
}

interface TripHistoryTabProps {
  refreshKey: number
  onOpen: (id: string) => void
  onEdit: (trip: TripSummary) => void
  onCancel: (trip: TripSummary) => void
}

export default function TripHistoryTab({ refreshKey, onOpen, onEdit, onCancel }: TripHistoryTabProps) {
  const { dataVersion } = useSignalR()
  const [statusFilter, setStatusFilter] = useState(FINISHED)
  const [fromDate, setFromDate] = useState(isoDate(30))
  const [toDate, setToDate] = useState(isoDate(0))

  const list = usePaged<TripSummary>({
    fetch: (params) => tripsApi.list(params),
    pageSize: 20,
    filters: {
      Status: statusFilter,
      ...(fromDate ? { FromDate: toISOStart(fromDate) } : {}),
      ...(toDate ? { ToDate: toISOEnd(toDate) } : {}),
    },
  })

  useEffect(() => {
    list.reload()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataVersion, refreshKey])

  const resetDays = (days: number) => {
    setFromDate(isoDate(days))
    setToDate(isoDate(0))
  }

  return (
    <div>
      <div className="table-toolbar">
        <h3 className="table-title">
          Historial de viajes
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
          <input
            type="date"
            className="filter-select filter-date"
            value={fromDate}
            onChange={(e) => {
              setFromDate(e.target.value)
              list.setPage(1)
            }}
            title="Desde"
          />
          <input
            type="date"
            className="filter-select filter-date"
            value={toDate}
            onChange={(e) => {
              setToDate(e.target.value)
              list.setPage(1)
            }}
            title="Hasta"
          />
          <button className="btn" onClick={() => resetDays(30)}>Últimos 30 días</button>
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
        empty="No hay viajes concluidos en el período."
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