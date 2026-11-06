import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Geofence } from './types';

/** GET /geofences/{id} — detalle de una geocerca. */
export function getById(id: string): Promise<ApiResult<Geofence>> {
  return apiRequest<ApiResult<Geofence>>(`/geofences/${id}`);
}