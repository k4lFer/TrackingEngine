import { apiList } from '../http/client';
import type { ApiResult } from '../types';
import type { Geofence } from './types';

/** GET /geofences — listado paginado. */
export function list(): Promise<ApiResult<Geofence[]>> {
  return apiList<Geofence>('/geofences');
}