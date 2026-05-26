import { create } from "zustand";
import { getAuthToken, setAuthTokens, onAuthTokenChange } from "@/api/client";
import { useTenantStore } from "@/stores/tenantStore";
import { decodeJwt } from "@/lib/jwt";

interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  userId: string | null;
  isSystemAdmin: boolean;
  isEmailVerified: boolean;
  login: (accessToken: string, refreshToken: string) => void;
  logout: () => void;
}

function deriveFromToken(token: string | null) {
  if (token === null) {
    return { userId: null, isSystemAdmin: false, isEmailVerified: false } as const;
  }
  const payload = decodeJwt(token);
  return {
    userId: (payload?.sub as string | undefined) ?? null,
    isSystemAdmin: payload?.["is_system_admin"] === "TRUE",
    isEmailVerified: payload?.email_verified === "TRUE",
  };
}

const initialToken = getAuthToken();

export const useAuthStore = create<AuthState>(() => ({
  token: initialToken,
  isAuthenticated: initialToken !== null,
  ...deriveFromToken(initialToken),
  login: (accessToken, refreshToken) => {
    setAuthTokens(accessToken, refreshToken);
  },
  logout: () => {
    useTenantStore.getState().clearActiveTenant();
    setAuthTokens(null, null);
  },
}));

onAuthTokenChange((token) => {
  useAuthStore.setState({
    token,
    isAuthenticated: token !== null,
    ...deriveFromToken(token),
  });
});
