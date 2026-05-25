import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPut } from "./client";
import type { PlanFeatureResponse, PlanLimitResponse, TenantFeaturesResponse } from "./types";

const SUBSCRIPTION_FEATURES_KEY = ["admin", "subscription", "features"] as const;
const SUBSCRIPTION_LIMITS_KEY = ["admin", "subscription", "limits"] as const;
const TENANT_SUBSCRIPTION_KEY = ["tenant", "subscription"] as const;

export function usePlanFeatures() {
  return useQuery({
    queryKey: SUBSCRIPTION_FEATURES_KEY,
    queryFn: () => apiGet<PlanFeatureResponse[]>("/admin/subscription/features"),
  });
}

export function useUpdatePlanFeature() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { planId: string; feature: string; isEnabled: boolean }) =>
      apiPut<void>(
        `/admin/subscription/plans/${encodeURIComponent(payload.planId)}/features/${encodeURIComponent(payload.feature)}`,
        { isEnabled: payload.isEnabled },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SUBSCRIPTION_FEATURES_KEY });
    },
  });
}

export function usePlanLimits() {
  return useQuery({
    queryKey: SUBSCRIPTION_LIMITS_KEY,
    queryFn: () => apiGet<PlanLimitResponse[]>("/admin/subscription/limits"),
  });
}

export function useUpdatePlanLimit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { planId: string; limit: string; value: number }) =>
      apiPut<void>(
        `/admin/subscription/plans/${encodeURIComponent(payload.planId)}/limits/${encodeURIComponent(payload.limit)}`,
        { value: payload.value },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SUBSCRIPTION_LIMITS_KEY });
    },
  });
}

export function useTenantSubscription() {
  return useQuery({
    queryKey: TENANT_SUBSCRIPTION_KEY,
    queryFn: () => apiGet<TenantFeaturesResponse>("/tenant/subscription"),
  });
}
