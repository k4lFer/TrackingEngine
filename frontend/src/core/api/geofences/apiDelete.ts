import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** DELETE /geofences/{id} — baja de una geocerca. */
export function remove(id: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/geofences/${id}`, {
    method: 'DELETE',
  });
}