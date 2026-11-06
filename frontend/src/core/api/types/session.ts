/** Códigos de estado de sesión (refleja SessionStatusEnum del backend). */
export const SessionStatus = {
  Active: 1,
  Revoked: 2,
  All: 3,
} as const;

export type SessionStatusValue = (typeof SessionStatus)[keyof typeof SessionStatus];