import { useApi } from './useApi'
import { useToast } from './useToast'
import { useConfirm } from './useConfirm'
import { summarizeMessages } from '../api/http/alerts'
import type { ApiResult } from '../api/types'

interface UseCrudOptions<T> {
  list: () => Promise<ApiResult<T[]>>
  remove: (id: string) => Promise<ApiResult<unknown>>
  deleteConfirm?: string
  deletedMessage?: string
}

export function useCrud<T>({ list, remove, deleteConfirm, deletedMessage }: UseCrudOptions<T>) {
  const { data, loading, error, reload } = useApi<T[]>(list)
  const { toast, notify } = useToast()
  const confirm = useConfirm()

  const removeItem = async (id: string) => {
    const ok = await confirm({
      title: 'Eliminar',
      message: deleteConfirm ?? '¿Desea eliminar este elemento?',
      confirmLabel: 'Eliminar',
      danger: true,
    })
    if (!ok) return
    try {
      const result = await remove(id)
      notify(summarizeMessages(result?.messages) ?? deletedMessage ?? 'Elemento eliminado')
      reload()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Error al eliminar', 'error')
    }
  }

  return { data, loading, error, reload, removeItem, toast, notify }
}