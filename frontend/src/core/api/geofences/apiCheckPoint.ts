import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Geofence } from './types';

/** GET /geofences/check-point — geocerca que contiene el punto (si existe). */
export function checkPoint(lat: number, lon: number): Promise<ApiResult<Geofence>> {
  return apiRequest<ApiResult<Geofence>>('/geofences/check-point', {
    query: { lat, lon },
  });
}