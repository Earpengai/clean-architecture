import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, apiPut } from "./client";
import type {
  TenantAdminResponse,
  SubscriptionPlanListItem,
  SubscriptionPlanDetail,
  SubscriptionResponse,
  TenantSubscriptionResponse,
  UpdateTenantSubscriptionPayload,
  CreateSubscriptionPlanPayload,
  UpdateSubscriptionStatusPayload,
} from "./types";

const ADMIN_KEY = ["admin"] as const;
const ADMIN_USERS_KEY = ["admin", "users"] as const;
const ADMIN_PLANS_KEY = ["admin", "subscription", "plans"] as const;
const ADMIN_SUBSCRIPTIONS_KEY = ["admin", "subscriptions"] as const;

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
    mutationFn: (payload: UpdateTenantSubscriptionPayload & { id: string }) =>
      apiPut<void>(`/admin/tenants/${encodeURIComponent(payload.id)}/subscription`, {
        subscriptionPlanId: payload.subscriptionPlanId,
        subscriptionStatus: payload.subscriptionStatus,
        maxUsersOverride: payload.maxUsersOverride,
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

export function useAdminSubscriptionPlans() {
  return useQuery({
    queryKey: ADMIN_PLANS_KEY,
    queryFn: () => apiGet<SubscriptionPlanListItem[]>("/admin/subscription/plans"),
  });
}

export function useAdminSubscriptionPlanById(id: string | null | undefined) {
  return useQuery({
    queryKey: [...ADMIN_PLANS_KEY, "detail", id],
    queryFn: () => apiGet<SubscriptionPlanDetail>(`/admin/subscription/plans/${encodeURIComponent(id!)}`),
    enabled: Boolean(id),
  });
}

export function useCreateSubscriptionPlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateSubscriptionPlanPayload) =>
      apiPost<string>("/admin/subscription/plans", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_PLANS_KEY });
    },
  });
}

export function useUpdateSubscriptionPlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateSubscriptionPlanPayload & { id: string }) =>
      apiPut<void>(`/admin/subscription/plans/${encodeURIComponent(payload.id)}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_PLANS_KEY });
    },
  });
}

export function useDeactivateSubscriptionPlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiPost<void>(`/admin/subscription/plans/${encodeURIComponent(id)}/deactivate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_PLANS_KEY });
    },
  });
}

export function useAdminSubscriptions() {
  return useQuery({
    queryKey: ADMIN_SUBSCRIPTIONS_KEY,
    queryFn: () => apiGet<SubscriptionResponse[]>("/admin/subscriptions"),
  });
}

export function useAdminTenantSubscription(tenantId: string | null | undefined) {
  return useQuery({
    queryKey: [...ADMIN_SUBSCRIPTIONS_KEY, "tenant", tenantId],
    queryFn: () =>
      apiGet<TenantSubscriptionResponse>(`/admin/tenants/${encodeURIComponent(tenantId!)}/subscription`),
    enabled: Boolean(tenantId),
  });
}

export function useUpdateSubscriptionStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateSubscriptionStatusPayload & { id: string }) =>
      apiPut<void>(`/admin/subscriptions/${encodeURIComponent(payload.id)}/status`, {
        newStatus: payload.newStatus,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ADMIN_SUBSCRIPTIONS_KEY });
    },
  });
}
