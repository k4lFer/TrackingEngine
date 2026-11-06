import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateRouteInput, RouteItem } from './types';

/** POST /routes — alta de una ruta. */
export function create(input: CreateRouteInput): Promise<ApiResult<RouteItem>> {
  return apiRequest<ApiResult<RouteItem>>('/routes', {
    method: 'POST',
    body: input,
  });
}