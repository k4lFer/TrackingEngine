import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** DELETE /roads/{id} — baja de una vía. */
export function remove(id: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/roads/${id}`, {
    method: 'DELETE',
  });
}