import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { PositionHistory } from './types';

/** GET /positions/history — historial posicional de una unidad. */
export function history(
  vehicleId: string,
  from?: string,
  to?: string,
): Promise<ApiResult<PositionHistory[]>> {
  return apiRequest<ApiResult<PositionHistory[]>>('/positions/history', {
    query: { vehicleId, from, to },
  });
}