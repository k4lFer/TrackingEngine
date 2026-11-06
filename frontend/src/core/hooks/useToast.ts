import { useCallback } from 'react'
import { toast as sonner } from 'sonner'

export type ToastState = { message: string; tone: 'success' | 'error' } | null

/**
 * Toasts del cliente basados en sonner. Las mutaciones del core/api avisan con
 * el mensaje del backend (ApiResult.messages) a través de este hook.
 */
export function useToast(defaultTone: 'success' | 'error' = 'success') {
  const notify = useCallback(
    (message: string, tone: 'success' | 'error' = defaultTone) => {
      if (tone === 'error') sonner.error(message)
      else sonner.success(message)
    },
    [defaultTone],
  )

  const dismiss = useCallback(() => sonner.dismiss(), [])

  // `toast` se mantiene para compatibilidad (las vistas ya no lo renderizan).
  const toast: ToastState = null

  return { toast, notify, dismiss }
}