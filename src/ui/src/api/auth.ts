import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTenantStore } from "@/stores/tenantStore";
import { apiGet, apiPost, apiPut, setAuthTokens, getAuthToken } from "./client";
import type { LoginResponse, EnableTwoFactorResponse, UserProfileResponse } from "./types";

interface LoginPayload {
  email: string;
  password: string;
}

interface RegisterPayload {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
}

interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

interface ChangeEmailPayload {
  newEmail: string;
}

interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
}

interface ResetPasswordPayload {
  userId: string;
  token: string;
  newPassword: string;
}

interface VerifyEmailPayload {
  userId: string;
  token: string;
}

interface LoginTwoFactorPayload {
  userId: string;
  twoFactorToken: string;
  code: string;
}

interface ConfirmTwoFactorPayload {
  code: string;
}

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: LoginPayload) => apiPost<LoginResponse>("/users/login", payload),
    onSuccess: (data) => {
      if (!data.requiresTwoFactor && data.accessToken && data.refreshToken) {
        setAuthTokens(data.accessToken, data.refreshToken);
        queryClient.clear();
      }
    },
  });
}

export function useLoginTwoFactor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: LoginTwoFactorPayload) => apiPost<LoginResponse>("/users/login-2fa", payload),
    onSuccess: (data) => {
      if (data.accessToken && data.refreshToken) {
        setAuthTokens(data.accessToken, data.refreshToken);
        queryClient.clear();
      }
    },
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (payload: RegisterPayload) => apiPost<string>("/users/register", payload),
  });
}

export function useLogout() {
  const queryClient = useQueryClient();

  return () => {
    useTenantStore.getState().clearActiveTenant();
    setAuthTokens(null, null);
    queryClient.clear();
  };
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (payload: ChangePasswordPayload) => apiPost<void>("/users/change-password", payload),
  });
}

export function useChangeEmail() {
  return useMutation({
    mutationFn: (payload: ChangeEmailPayload) => apiPost<void>("/users/change-email", payload),
  });
}

export function useUpdateProfile() {
  return useMutation({
    mutationFn: (payload: UpdateProfilePayload) => apiPut<void>("/users/profile", payload),
  });
}

export function useRequestPasswordReset() {
  return useMutation({
    mutationFn: (payload: { email: string }) => apiPost<void>("/users/request-password-reset", payload),
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: (payload: ResetPasswordPayload) => apiPost<void>("/users/reset-password", payload),
  });
}

export function useVerifyEmail() {
  return useMutation({
    mutationFn: (payload: VerifyEmailPayload) => apiPost<void>("/users/verify-email", payload),
  });
}

export function useRequestEmailVerification() {
  return useMutation({
    mutationFn: () => apiPost<void>("/users/request-verification"),
  });
}

export function useEnableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<EnableTwoFactorResponse>("/users/enable-2fa"),
  });
}

export function useConfirmTwoFactor() {
  return useMutation({
    mutationFn: (payload: ConfirmTwoFactorPayload) => apiPost<void>("/users/confirm-2fa", payload),
  });
}

export function useDisableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<void>("/users/disable-2fa"),
  });
}

export function useGetProfile() {
  return useQuery({
    queryKey: ["user-profile"],
    queryFn: () => apiGet<UserProfileResponse>("/users/profile"),
  });
}

export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}
