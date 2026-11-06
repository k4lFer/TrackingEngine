import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { Geofence, UpdateGeofenceInput } from './types';

/** PUT /geofences/{id} — actualización de una geocerca. */
export function update(id: string, input: UpdateGeofenceInput): Promise<ApiResult<Geofence>> {
  return apiRequest<ApiResult<Geofence>>(`/geofences/${id}`, {
    method: 'PUT',
    body: input,
  });
}