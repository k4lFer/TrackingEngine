import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { RouteItem, UpdateRouteInput } from './types';

/** PUT /routes/{id} — actualización de una ruta. */
export function update(id: string, input: UpdateRouteInput): Promise<ApiResult<RouteItem>> {
  return apiRequest<ApiResult<RouteItem>>(`/routes/${id}`, {
    method: 'PUT',
    body: input,
  });
}