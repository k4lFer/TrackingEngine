import { apiRequest } from '../http/client';
import type { ApiResult } from '../types';

/** Entrada de POST /api/auth/verify-email (refleja VerifyEmailDto del backend). */
export interface VerifyEmailInput {
  email?: string;
  token?: string;
  code?: string;
}

/** POST /api/auth/verify-email — confirma la cuenta con token del enlace o código OTP. */
export function verifyEmail(input: VerifyEmailInput): Promise<ApiResult<null>> {
  return apiRequest<ApiResult<null>>('/api/auth/verify-email', {
    method: 'POST',
    body: input,
    auth: 'none',
  });
}