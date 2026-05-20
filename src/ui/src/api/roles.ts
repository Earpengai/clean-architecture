import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, apiPut, apiDelete } from "./client";
import type { RoleResponse, RoleFormPayload } from "./types";

const ROLES_KEY = ["roles"] as const;

export function useRoles() {
  return useQuery({
    queryKey: ROLES_KEY,
    queryFn: () => apiGet<RoleResponse[]>("/tenants/roles"),
  });
}

export function useCreateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: RoleFormPayload) => apiPost<string>("/tenants/roles", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ROLES_KEY });
    },
  });
}

export function useUpdateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: RoleFormPayload }) =>
      apiPut<void>(`/tenants/roles/${encodeURIComponent(id)}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ROLES_KEY });
    },
  });
}

export function useDeleteRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => apiDelete<void>(`/tenants/roles/${encodeURIComponent(id)}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ROLES_KEY });
    },
  });
}
