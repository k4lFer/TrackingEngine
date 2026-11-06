import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** Entrada de POST /api/auth/resend-verification (refleja ResendVerificationDto del backend). */
export interface ResendVerificationInput {
  email: string;
}

/** POST /api/auth/resend-verification — reenvía el email de verificación. */
export function resendVerification(input: ResendVerificationInput): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>('/api/auth/resend-verification', {
    method: 'POST',
    body: input,
    auth: 'none',
  });
}