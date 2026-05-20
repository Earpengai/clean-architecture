const API_TARGET = import.meta.env.VITE_API_TARGET as string | undefined;
const API_BASE = API_TARGET ? `${API_TARGET}` : "";
const TOKEN_KEY = "auth_token";

type TokenListener = (token: string | null) => void;

const listeners = new Set<TokenListener>();

function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function storeToken(token: string | null) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_KEY);
  }
}

export function setAuthToken(token: string | null) {
  storeToken(token);
  listeners.forEach((fn) => fn(token));
}

export function getAuthToken(): string | null {
  return getStoredToken();
}

export function onAuthTokenChange(fn: TokenListener): () => void {
  listeners.add(fn);
  return () => {
    listeners.delete(fn);
  };
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

async function apiRequest<T>(path: string, init: RequestInit): Promise<T> {
  const token = getStoredToken();

  const headers: Record<string, string> = {
    ...(init.headers as Record<string, string> | undefined),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    const errorBody = await response.text();
    let detail = "";
    try {
      const parsed = JSON.parse(errorBody);
      detail = parsed.detail ?? parsed.title ?? errorBody;
    } catch {
      detail = errorBody || response.statusText;
    }

    if (response.status === 401) {
      setAuthToken(null);
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
