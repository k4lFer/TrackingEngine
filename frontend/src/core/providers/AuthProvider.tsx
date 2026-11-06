import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { ApiResult } from '../api/types';
import * as authApi from '../api/auth';
import type { LoginInput, LoginResponse, SignUpInput, VerifyEmailInput } from '../api/auth';
import { configureClient } from '../api/http/client';
import { clearSession, getDeviceId, getSession, setSession } from '../storage/session';

interface AuthContextValue {
  /** Sesión completa (token + refresh + usuario) o null si no hay sesión. */
  session: LoginResponse | null;
  isAuthenticated: boolean;
  userId: string | null;
  email: string | null;
  username: string | null;
  deviceId: string;
  /** Inicia sesión y persiste la sesión si el backend responde OK. */
  login: (input: LoginInput) => Promise<ApiResult<LoginResponse> | Error>;
  signUp: (input: SignUpInput) => Promise<ApiResult<null> | Error>;
  verifyEmail: (input: VerifyEmailInput) => Promise<ApiResult<null> | Error>;
  resendVerification: (email: string) => Promise<ApiResult<null> | Error>;
  /** Cierra la sesión en el backend y limpia la sesión local. */
  logout: () => Promise<void>;
  clearSession: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<LoginResponse | null>(() => getSession());
  const deviceId = getDeviceId();

  useEffect(() => {
    configureClient({
      getSessionTokens: () => {
        const current = getSession();
        return {
          accessToken: current?.token ?? null,
          deviceId: getDeviceId(),
        };
      },
      onRefreshed: (next) => {
        setSession(next);
        setSessionState(next);
      },
      onUnauthorized: () => {
        setSessionState((prev) => {
          if (prev) {
            clearSession();
          }
          return null;
        });
      },
    });
  }, []);

  const persistSession = useCallback((next: LoginResponse) => {
    setSession(next);
    setSessionState(next);
  }, []);

  const asError = useCallback((err: unknown) => {
    return err instanceof Error ? err : new Error('Error inesperado.');
  }, []);

  const login = useCallback(
    async (input: LoginInput) => {
      try {
        const result = await authApi.login({ ...input, deviceId });
        if (result.data) persistSession(result.data);
        return result;
      } catch (err) {
        return asError(err);
      }
    },
    [deviceId, persistSession, asError],
  );

  const signUp = useCallback(
    async (input: SignUpInput) => {
      try {
        return await authApi.signUp(input);
      } catch (err) {
        return asError(err);
      }
    },
    [asError],
  );

  const verifyEmail = useCallback(
    async (input: VerifyEmailInput) => {
      try {
        return await authApi.verifyEmail(input);
      } catch (err) {
        return asError(err);
      }
    },
    [asError],
  );

  const resendVerification = useCallback(
    async (email: string) => {
      try {
        return await authApi.resendVerification({ email });
      } catch (err) {
        return asError(err);
      }
    },
    [asError],
  );

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      clearSession();
      setSessionState(null);
    }
  }, []);

  const clearSessionValue = useCallback(() => {
    clearSession();
    setSessionState(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: session !== null,
      userId: session?.userId ?? null,
      email: session?.email ?? null,
      username: session?.username ?? null,
      deviceId,
      login,
      signUp,
      verifyEmail,
      resendVerification,
      logout,
      clearSession: clearSessionValue,
    }),
    [session, deviceId, login, signUp, verifyEmail, resendVerification, logout, clearSessionValue],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export default AuthContext;