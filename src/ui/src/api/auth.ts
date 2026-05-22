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
    mutationFn: (payload: LoginPayload) => apiPost<LoginResponse>("/auth/login", payload),
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
    mutationFn: (payload: LoginTwoFactorPayload) => apiPost<LoginResponse>("/auth/login-2fa", payload),
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
    mutationFn: (payload: RegisterPayload) => apiPost<string>("/auth/register", payload),
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
    mutationFn: (payload: ChangePasswordPayload) => apiPost<void>("/profile/change-password", payload),
  });
}

export function useChangeEmail() {
  return useMutation({
    mutationFn: (payload: ChangeEmailPayload) => apiPost<void>("/profile/change-email", payload),
  });
}

export function useUpdateProfile() {
  return useMutation({
    mutationFn: (payload: UpdateProfilePayload) => apiPut<void>("/profile", payload),
  });
}

export function useRequestPasswordReset() {
  return useMutation({
    mutationFn: (payload: { email: string }) => apiPost<void>("/auth/request-password-reset", payload),
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: (payload: ResetPasswordPayload) => apiPost<void>("/auth/reset-password", payload),
  });
}

export function useVerifyEmail() {
  return useMutation({
    mutationFn: (payload: VerifyEmailPayload) => apiPost<void>("/auth/verify-email", payload),
  });
}

export function useRequestEmailVerification() {
  return useMutation({
    mutationFn: () => apiPost<void>("/profile/request-verification"),
  });
}

export function useEnableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<EnableTwoFactorResponse>("/profile/enable-2fa"),
  });
}

export function useConfirmTwoFactor() {
  return useMutation({
    mutationFn: (payload: ConfirmTwoFactorPayload) => apiPost<void>("/profile/confirm-2fa", payload),
  });
}

export function useDisableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<void>("/profile/disable-2fa"),
  });
}

export function useGetProfile() {
  return useQuery({
    queryKey: ["user-profile"],
    queryFn: () => apiGet<UserProfileResponse>("/profile"),
  });
}

export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}
