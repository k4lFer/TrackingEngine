import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { TrackingEvent } from './types';

/** GET /events/vehicle/{vehicleId} — eventos de una unidad en un rango. */
export function byVehicle(
  vehicleId: string,
  from?: string,
  to?: string,
): Promise<ApiResult<TrackingEvent[]>> {
  return apiRequest<ApiResult<TrackingEvent[]>>(`/events/vehicle/${vehicleId}`, {
    query: { from, to },
  });
}