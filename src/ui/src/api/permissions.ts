import { useQuery } from "@tanstack/react-query";
import { apiGet, getAuthToken } from "./client";
import { useTenantStore } from "@/stores/tenantStore";
import type { MyPermissionsResponse } from "./types";

const PERMISSIONS_KEY = ["my-permissions"] as const;

export function useMyPermissions() {
  const activeTenantId = useTenantStore((state) => state.activeTenantId);

  return useQuery({
    queryKey: PERMISSIONS_KEY,
    queryFn: () => apiGet<MyPermissionsResponse>("/tenants/my-permissions"),
    enabled: getAuthToken() !== null && activeTenantId !== null,
    staleTime: 5 * 60 * 1000,
  });
}
