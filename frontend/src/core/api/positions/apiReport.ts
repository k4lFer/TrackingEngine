import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';
import type { PositionReportInput, PositionResponse } from './types';

/** POST /positions/report — reporte de posición desde un dispositivo. */
export function report(input: PositionReportInput): Promise<ApiResult<PositionResponse>> {
  return apiRequest<ApiResult<PositionResponse>>('/positions/report', {
    method: 'POST',
    body: input,
  });
}