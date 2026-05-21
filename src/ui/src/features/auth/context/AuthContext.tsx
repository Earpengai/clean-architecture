import { createContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { getAuthToken, setAuthTokens, onAuthTokenChange } from "@/api/client";

interface AuthContextValue {
  token: string | null;
  isAuthenticated: boolean;
  login: (accessToken: string, refreshToken: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => getAuthToken());

  useEffect(() => {
    return onAuthTokenChange(setToken);
  }, []);

  const login = useCallback((accessToken: string, refreshToken: string) => {
    setAuthTokens(accessToken, refreshToken);
  }, []);

  const logout = useCallback(() => {
    setAuthTokens(null, null);
  }, []);

  return (
    <AuthContext.Provider value={{ token, isAuthenticated: token !== null, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
