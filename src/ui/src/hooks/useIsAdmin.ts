import { useAuthStore } from "@/stores/authStore";

export function useIsAdmin(): boolean {
  return useAuthStore((state) => state.isSystemAdmin);
}
