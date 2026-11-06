import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { CreateRoadInput, RoadItem } from './types';

/** POST /roads — alta de una vía. */
export function create(input: CreateRoadInput): Promise<ApiResult<RoadItem>> {
  return apiRequest<ApiResult<RoadItem>>('/roads', {
    method: 'POST',
    body: input,
  });
}