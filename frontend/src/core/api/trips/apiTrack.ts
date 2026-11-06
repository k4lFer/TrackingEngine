import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { PositionHistory } from '../positions';

/** GET /trips/{id}/track — puntos del recorrido del viaje (array directo). */
export function track(id: string): Promise<ApiResult<PositionHistory[]>> {
  return apiRequest<ApiResult<PositionHistory[]>>(`/trips/${id}/track`);
}