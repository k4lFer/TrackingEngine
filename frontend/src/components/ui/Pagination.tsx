import { ChevronLeftIcon, ChevronRightIcon } from './Icons'

interface PaginationProps {
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  onPageChange: (page: number) => void
}

export default function Pagination({ page, totalPages, totalCount, pageSize, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  const from = totalCount === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalCount)

  return (
    <div className="pagination">
      <span className="pagination-info mono">
        {from}–{to} de {totalCount}
      </span>
      <div className="pagination-buttons">
        <button className="btn" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          <ChevronLeftIcon size={14} /> Anterior
        </button>
        <span className="pagination-page mono">
          Pág. {page} de {totalPages}
        </span>
        <button className="btn" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>
          Siguiente <ChevronRightIcon size={14} />
        </button>
      </div>
    </div>
  )
}