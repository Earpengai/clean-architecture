import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, setAuthTokens } from "./client";
import type { TenantResponse, CreateTenantPayload, CreateTenantResponse } from "./types";

const TENANTS_KEY = ["tenants"] as const;

export function useMyTenants() {
  return useQuery({
    queryKey: TENANTS_KEY,
    queryFn: () => apiGet<TenantResponse[]>("/tenants/mine"),
  });
}

export function useTenantById(id: string | null | undefined) {
  return useQuery({
    queryKey: [...TENANTS_KEY, "detail", id],
    queryFn: () => apiGet<TenantResponse>(`/tenants/${encodeURIComponent(id!)}`),
    enabled: Boolean(id),
  });
}

export function useCreateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTenantPayload) => apiPost<CreateTenantResponse>("/tenants", payload),
    onSuccess: (data) => {
      setAuthTokens(data.accessToken, data.refreshToken);
      queryClient.invalidateQueries({ queryKey: TENANTS_KEY });
    },
  });
}
