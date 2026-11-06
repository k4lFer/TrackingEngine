import { apiList } from '../http/client';
import type { ApiResult } from '../types';
import type { RoadItem } from './types';

/** GET /roads — listado paginado. */
export function list(): Promise<ApiResult<RoadItem[]>> {
  return apiList<RoadItem>('/roads');
}