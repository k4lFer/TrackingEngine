import type { ReactNode } from 'react'

interface ModalProps {
  title: string
  wide?: boolean
  full?: boolean
  side?: ReactNode
  className?: string
  onClose: () => void
  children: ReactNode
}

export default function Modal({ title, wide, full, side, className, onClose, children }: ModalProps) {
  return (
    <div
      className={`modal-backdrop ${full ? 'modal-backdrop-full' : ''}`.trim()}
      onClick={onClose}
    >
      <div
        className={`modal ${full ? 'modal-full' : ''} ${wide ? 'modal-wide' : ''} ${className ?? ''}`.trim()}
        onClick={(e) => e.stopPropagation()}
      >
        {full ? (
          <>
            <aside className="modal-side">
              <div className="modal-side-head">
                <h3>{title}</h3>
                <button type="button" className="modal-close" onClick={onClose} aria-label="Cerrar">
                  ✕
                </button>
              </div>
              {side}
            </aside>
            <div className="modal-body">{children}</div>
          </>
        ) : (
          <>
            <div className="modal-head">
              <h3>{title}</h3>
              <button type="button" className="modal-close" onClick={onClose} aria-label="Cerrar">
                ✕
              </button>
            </div>
            {children}
          </>
        )}
      </div>
    </div>
  )
}