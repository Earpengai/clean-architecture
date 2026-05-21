export interface JwtPayload {
  sub: string;
  is_system_admin: string | undefined;
  security_stamp: string;
  [key: string]: unknown;
}

export function decodeJwt(token: string): JwtPayload | null {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      return null;
    }

    return JSON.parse(atob(parts[1]!)) as JwtPayload;
  } catch {
    return null;
  }
}
