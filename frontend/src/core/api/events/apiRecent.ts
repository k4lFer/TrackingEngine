import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { RecentEvents } from './types';

/** GET /events?count={count} — eventos recientes de la flota. */
export function recent(count = 50): Promise<ApiResult<RecentEvents>> {
  return apiRequest<ApiResult<RecentEvents>>('/events', {
    query: { count },
  });
}