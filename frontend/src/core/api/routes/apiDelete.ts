import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** DELETE /routes/{id} — baja de una ruta. */
export function remove(id: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/routes/${id}`, {
    method: 'DELETE',
  });
}