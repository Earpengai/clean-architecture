import { useTenantStore } from "@/stores/tenantStore";
import type { RefreshTokenResponse } from "./types";

const API_TARGET = import.meta.env.VITE_API_TARGET as string | undefined;
const API_BASE = API_TARGET ? `${API_TARGET}/api` : "/api";
const ACCESS_TOKEN_KEY = "access_token";
const REFRESH_TOKEN_KEY = "refresh_token";

type TokenListener = (token: string | null) => void;

const listeners = new Set<TokenListener>();

let refreshPromise: Promise<RefreshTokenResponse | null> | null = null;

function getStoredAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

function getStoredRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

function storeTokens(accessToken: string | null, refreshToken: string | null) {
  if (accessToken) {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  } else {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
  }
  if (refreshToken) {
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  } else {
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }
}

function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function setAuthTokens(accessToken: string | null, refreshToken: string | null) {
  storeTokens(accessToken, refreshToken);
  listeners.forEach((fn) => fn(accessToken));
}

export function getAuthToken(): string | null {
  return getStoredAccessToken();
}

export function onAuthTokenChange(fn: TokenListener): () => void {
  listeners.add(fn);
  return () => {
    listeners.delete(fn);
  };
}

async function refreshAccessToken(): Promise<RefreshTokenResponse | null> {
  const storedRefreshToken = getStoredRefreshToken();
  if (!storedRefreshToken) {
    return null;
  }

  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    try {
      const response = await fetch(`${API_BASE}/users/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: storedRefreshToken }),
      });

      if (!response.ok) {
        clearTokens();
        listeners.forEach((fn) => fn(null));
        return null;
      }

      const data = (await response.json()) as RefreshTokenResponse;
      storeTokens(data.accessToken, data.refreshToken);
      listeners.forEach((fn) => fn(data.accessToken));
      return data;
    } catch {
      clearTokens();
      listeners.forEach((fn) => fn(null));
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiGet<T>(path: string): Promise<T> {
  return apiRequest<T>(path, { method: "GET" });
}

export async function apiPost<T>(path: string, body?: unknown): Promise<T> {
  return apiRequest<T>(path, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body ? JSON.stringify(body) : undefined,
  });
}

export async function apiPut<T>(path: string, body?: unknown): Promise<T> {
  return apiRequest<T>(path, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: body ? JSON.stringify(body) : undefined,
  });
}

export async function apiDelete<T>(path: string): Promise<T> {
  return apiRequest<T>(path, { method: "DELETE" });
}

async function apiRequest<T>(path: string, init: RequestInit, retry = true): Promise<T> {
  const token = getStoredAccessToken();

  const headers: Record<string, string> = {
    ...(init.headers as Record<string, string> | undefined),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const activeTenantIdentifier = useTenantStore.getState().activeTenantIdentifier;
  if (activeTenantIdentifier) {
    headers["X-Tenant-Id"] = activeTenantIdentifier;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    if (response.status === 401 && retry && getStoredRefreshToken()) {
      const refreshed = await refreshAccessToken();
      if (refreshed) {
        return apiRequest<T>(path, init, false);
      }
      throw new ApiError(response.status, "Session expired. Please sign in again.");
    }

    if (response.status === 401) {
      clearTokens();
      listeners.forEach((fn) => fn(null));
    }

    const errorBody = await response.text();
    let detail = "";
    try {
      const parsed = JSON.parse(errorBody);
      detail = parsed.detail ?? parsed.title ?? errorBody;
    } catch {
      detail = errorBody || response.statusText;
    }

    throw new ApiError(response.status, detail);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export class ApiError extends Error {
  status: number;

  constructor(status: number, detail: string) {
    super(detail);
    this.name = "ApiError";
    this.status = status;
  }
}
