import { useAuth } from "@/hooks/useAuth";

export function useIsAdmin(): boolean {
  const { token } = useAuth();

  if (token === null) {
    return false;
  }

  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      return false;
    }

    const payload = JSON.parse(atob(parts[1]!)) as Record<string, unknown>;
    return payload["is_system_admin"] === "TRUE";
  } catch {
    return false;
  }
}
