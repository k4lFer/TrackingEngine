import { apiRequest } from '../http/client';
import type { ApiPagedResult, ApiResult, SessionStatusValue } from '../types';

/** Refleja ActiveSessionDto del backend. */
export interface ActiveSession {
  id: string;
  deviceId: string;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
  lastActivityAt: string;
  expiresAt: string | null;
  revokedAt: string | null;
  isCurrentDevice: boolean;
  readonly isRevoked: boolean;
}

/**
 * GET /api/auth/sessions — lista sesiones según estado.
 * Status: 1 activas (defecto), 2 revocadas, 3 todas.
 */
export function getSessions(opts?: {
  status?: SessionStatusValue;
  page?: number;
  pageSize?: number;
}): Promise<ApiPagedResult<ActiveSession>> {
  return apiRequest<ApiPagedResult<ActiveSession>>('/api/auth/sessions', {
    method: 'GET',
    query: {
      Status: opts?.status ?? 1,
      NumberPage: opts?.page ?? 1,
      PageSize: opts?.pageSize ?? 10,
    },
  });
}

/** DELETE /api/auth/sessions — revoca todas excepto la actual (X-Device-Id). */
export function revokeAllSessionsExceptCurrent(): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>('/api/auth/sessions', {
    method: 'DELETE',
  });
}

/** DELETE /api/auth/sessions/{sessionId} — revoca una sesión específica. */
export function revokeSession(sessionId: string): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>(`/api/auth/sessions/${sessionId}`, {
    method: 'DELETE',
  });
}