import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { TripSummary } from './types';

/** GET /trips/vehicle/{vehicleId} — viajes de una unidad (array directo). */
export function byVehicle(vehicleId: string): Promise<ApiResult<TripSummary[]>> {
  return apiRequest<ApiResult<TripSummary[]>>(`/trips/vehicle/${vehicleId}`);
}