import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateGeofenceInput, Geofence } from './types';

/** POST /geofences — alta de una geocerca. */
export function create(input: CreateGeofenceInput): Promise<ApiResult<Geofence>> {
  return apiRequest<ApiResult<Geofence>>('/geofences', {
    method: 'POST',
    body: input,
  });
}