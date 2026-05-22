import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost } from "./client";
import type { InitiatePaymentResponse, CheckPaymentResponse, PaymentResponse } from "./types";

const BILLING_KEY = ["tenant", "subscription", "payment"] as const;

export function useInitiatePayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { plan: number; billingPeriod: number }) =>
      apiPost<InitiatePaymentResponse>("/tenant/subscription/payment", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: BILLING_KEY });
    },
  });
}

export function useCheckPayment(md5: string | null) {
  return useQuery({
    queryKey: [...BILLING_KEY, "check", md5],
    queryFn: () => apiGet<CheckPaymentResponse>(`/tenant/subscription/payment/${encodeURIComponent(md5!)}`),
    enabled: md5 !== null,
    refetchInterval: (query) => {
      const data = query.state.data;
      if (data?.isCompleted) return false;
      return 5000;
    },
  });
}

export function usePaymentHistory() {
  return useQuery({
    queryKey: [...BILLING_KEY, "history"],
    queryFn: () => apiGet<PaymentResponse[]>("/tenant/subscription/payment/history"),
  });
}
