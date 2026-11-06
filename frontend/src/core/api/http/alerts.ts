import { toast } from 'sonner';
import type { ApiMessage, ApiResult } from '../types';
import { ApiHttpError } from './client';

/** Alerta el primer mensaje de una ApiResult (éxito: toast.success). */
export function notifyResult<T>(
  result: ApiResult<T>,
  opts?: { success?: string; errorTitle?: string },
): T | null {
  const message = result.messages?.[0]?.message;

  if (result.data !== null && result.data !== undefined) {
    toast.success(opts?.success ?? message ?? 'Operación completada.');
    return result.data;
  }

  // Respuesta sin data: si no hay mensaje de error, asumimos éxito.
  if (!message) {
    toast.success(opts?.success ?? 'Operación completada.');
    return null;
  }

  toast.error(message, { description: opts?.errorTitle });
  return null;
}

/** Alerta un ApiHttpError como toast de error. */
export function notifyError(error: unknown, opts?: { fallback?: string }): void {
  if (error instanceof ApiHttpError) {
    toast.error(error.messages[0]?.message ?? error.message);
    return;
  }
  if (error instanceof Error) {
    toast.error(error.message || (opts?.fallback ?? 'Error inesperado.'));
    return;
  }
  toast.error(opts?.fallback ?? 'Error inesperado.');
}

/** Devuelve true si la ApiResult se considera exitosa (tiene data o no mensajes de error). */
export function isSuccess<T>(result: ApiResult<T>): boolean {
  return result.data !== null && result.data !== undefined;
}

export type { ApiMessage };

export function summarizeMessages(messages: ApiMessage[] | null | undefined): string | null {
  return messages?.[0]?.message ?? null;
}