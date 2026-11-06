import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { RouteItem } from './types';

/** GET /routes/{id} — detalle de una ruta (incluye alternativas). */
export function getById(id: string): Promise<ApiResult<RouteItem>> {
  return apiRequest<ApiResult<RouteItem>>(`/routes/${id}`);
}