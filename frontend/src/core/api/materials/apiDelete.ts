import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** DELETE /materials/{id} — baja de un material. */
export function remove(id: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/materials/${id}`, {
    method: 'DELETE',
  });
}