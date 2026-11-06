import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import Modal from '../../components/ui/Modal'

interface ConfirmOptions {
  title?: string
  message?: string
  confirmLabel?: string
  cancelLabel?: string
  danger?: boolean
}

type ConfirmFn = (options?: ConfirmOptions) => Promise<boolean>

interface ConfirmState {
  options: ConfirmOptions
  resolve: (value: boolean) => void
}

const ConfirmContext = createContext<ConfirmFn | null>(null)

export function ConfirmProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<ConfirmState | null>(null)

  const confirm = useCallback<ConfirmFn>(
    (options = {}) => new Promise<boolean>((resolve) => setState({ options, resolve })),
    [],
  )

  const close = (value: boolean) => {
    state?.resolve(value)
    setState(null)
  }

  return (
    <ConfirmContext.Provider value={confirm}>
      {children}
      {state && (
        <Modal title={state.options.title ?? '¿Confirmar?'} onClose={() => close(false)}>
          <div className="text-[13.5px] leading-relaxed text-ink-dim pb-1">
            {state.options.message ?? '¿Desea continuar?'}
          </div>
          <div className="flex justify-end gap-2.5 mt-4">
            <button type="button" className="btn" onClick={() => close(false)} autoFocus>
              {state.options.cancelLabel ?? 'Cancelar'}
            </button>
            <button
              type="button"
              className={`btn ${state.options.danger ? 'danger' : 'primary'}`}
              onClick={() => close(true)}
            >
              {state.options.confirmLabel ?? 'Confirmar'}
            </button>
          </div>
        </Modal>
      )}
    </ConfirmContext.Provider>
  )
}

export function useConfirm() {
  const ctx = useContext(ConfirmContext)
  if (!ctx) throw new Error('useConfirm debe usarse dentro de <ConfirmProvider>')
  return ctx
}