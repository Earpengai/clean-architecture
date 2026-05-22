import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, apiDelete, setAuthTokens } from "./client";
import type { UserResponse, InvitationResponse, MyInvitationResponse, AcceptInvitationResponse } from "./types";
import { TENANTS_KEY } from "./tenants";

const USERS_KEY = ["users"] as const;
const INVITATIONS_KEY = ["invitations"] as const;
const MY_INVITATIONS_KEY = ["invitations", "my"] as const;

export function useUsers() {
  return useQuery({
    queryKey: USERS_KEY,
    queryFn: () => apiGet<UserResponse[]>("/users"),
  });
}

export function useUserById(id: string | null | undefined) {
  return useQuery({
    queryKey: [...USERS_KEY, "detail", id],
    queryFn: () => apiGet<UserResponse>(`/users/${encodeURIComponent(id!)}`),
    enabled: Boolean(id),
  });
}

export function useInviteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { email: string; roleId: string }) => apiPost<string>("/tenants/invitations", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: INVITATIONS_KEY });
    },
  });
}

export function useAssignRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: string }) =>
      apiPost<void>(`/tenants/users/${encodeURIComponent(userId)}/roles`, { roleId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USERS_KEY });
    },
  });
}

export function useRemoveUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => apiDelete<void>(`/tenants/users/${encodeURIComponent(userId)}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USERS_KEY });
    },
  });
}

export function useInvitations() {
  return useQuery({
    queryKey: INVITATIONS_KEY,
    queryFn: () => apiGet<InvitationResponse[]>("/tenants/invitations"),
  });
}

export function useAcceptInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { token: string; firstName: string; lastName: string; password: string }) =>
      apiPost<AcceptInvitationResponse>(`/invitations/${encodeURIComponent(payload.token)}/accept`, payload),
    onSuccess: (data) => {
      setAuthTokens(data.accessToken, data.refreshToken);
      queryClient.clear();
    },
  });
}

export function useMyInvitations() {
  return useQuery({
    queryKey: MY_INVITATIONS_KEY,
    queryFn: () => apiGet<MyInvitationResponse[]>("/invitations/my"),
  });
}

export function useAcceptInvitationAuthenticated() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (token: string) =>
      apiPost<AcceptInvitationResponse>(`/invitations/${encodeURIComponent(token)}/accept-authenticated`),
    onSuccess: (data) => {
      setAuthTokens(data.accessToken, data.refreshToken);
      queryClient.invalidateQueries({ queryKey: MY_INVITATIONS_KEY });
      queryClient.invalidateQueries({ queryKey: TENANTS_KEY });
    },
  });
}
