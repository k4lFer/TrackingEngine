import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { RoadItem } from './types';

/** GET /roads/{id} — detalle de una vía. */
export function getById(id: string): Promise<ApiResult<RoadItem>> {
  return apiRequest<ApiResult<RoadItem>>(`/roads/${id}`);
}