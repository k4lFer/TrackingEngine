import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** DELETE /vehicles/{id} — baja de una unidad. */
export function remove(id: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/vehicles/${id}`, {
    method: 'DELETE',
  });
}