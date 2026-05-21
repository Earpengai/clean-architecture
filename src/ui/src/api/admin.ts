import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, apiPut } from "./client";
import type { TenantAdminResponse } from "./types";

const ADMIN_KEY = ["admin"] as const;
const ADMIN_USERS_KEY = ["admin", "users"] as const;

export function useAdminTenants() {
  return useQuery({
    queryKey: ADMIN_KEY,
    queryFn: () => apiGet<TenantAdminResponse[]>("/admin/tenants"),
  });
}

export function useEnableTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => apiPost<void>(`/admin/tenants/${encodeURIComponent(id)}/enable`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_KEY });
    },
  });
}

export function useDisableTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => apiPost<void>(`/admin/tenants/${encodeURIComponent(id)}/disable`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_KEY });
    },
  });
}

export function useUpdateTenantSubscription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { id: string; subscriptionPlan: number; subscriptionStatus: number; seatCount: number }) =>
      apiPut<void>(`/admin/tenants/${encodeURIComponent(payload.id)}/subscription`, {
        subscriptionPlan: payload.subscriptionPlan,
        subscriptionStatus: payload.subscriptionStatus,
        seatCount: payload.seatCount,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_KEY });
    },
  });
}

export function useLockUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => apiPost<void>(`/admin/users/${encodeURIComponent(userId)}/lock`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_USERS_KEY });
    },
  });
}

export function useUnlockUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => apiPost<void>(`/admin/users/${encodeURIComponent(userId)}/unlock`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_USERS_KEY });
    },
  });
}
