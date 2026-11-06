import { apiList } from '../http/client';
import type { ApiResult } from '../types';
import type { RouteItem } from './types';

/** GET /routes — listado paginado. */
export function list(): Promise<ApiResult<RouteItem[]>> {
  return apiList<RouteItem>('/routes');
}