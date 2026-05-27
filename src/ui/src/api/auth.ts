import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTenantStore } from "@/stores/tenantStore";
import { apiGet, apiPost, apiPut, apiDelete, setAuthTokens, getAuthToken } from "./client";
import type { LoginResponse, EnableTwoFactorResponse, UserProfileResponse, RegisterResponse, ConfirmTwoFactorResponse, UserSessionResponse } from "./types";

interface LoginPayload {
  email: string;
  password: string;
}

interface RegisterPayload {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  companyName: string;
  industry: string;
  country: string;
  acceptedTerms: boolean;
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
  code: string;
  rememberDevice: boolean;
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
    mutationFn: (payload: RegisterPayload) => apiPost<RegisterResponse>("/auth/register", payload),
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

interface ResendVerificationPayload {
  userId: string;
}

export function useResendVerification() {
  return useMutation({
    mutationFn: (payload: ResendVerificationPayload) => apiPost<void>("/auth/resend-verification", payload),
  });
}

export function useEnableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<EnableTwoFactorResponse>("/profile/enable-2fa"),
  });
}

export function useConfirmTwoFactor() {
  return useMutation({
    mutationFn: (payload: ConfirmTwoFactorPayload) => apiPost<ConfirmTwoFactorResponse>("/profile/confirm-2fa", payload),
  });
}

export function useDisableTwoFactor() {
  return useMutation({
    mutationFn: () => apiPost<void>("/profile/disable-2fa"),
  });
}

interface LoginRecoveryPayload {
  userId: string;
  recoveryCode: string;
}

export function useLoginRecovery() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: LoginRecoveryPayload) => apiPost<LoginResponse>("/auth/login-recovery", payload),
    onSuccess: (data) => {
      if (data.accessToken && data.refreshToken) {
        setAuthTokens(data.accessToken, data.refreshToken);
        queryClient.clear();
      }
    },
  });
}

export function useGetProfile() {
  return useQuery({
    queryKey: ["user-profile"],
    queryFn: () => apiGet<UserProfileResponse>("/profile"),
  });
}

export function useUserSessions() {
  return useQuery({
    queryKey: ["user-sessions"],
    queryFn: () => apiGet<UserSessionResponse[]>("/profile/sessions"),
  });
}

export function useRevokeSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (sessionId: string) => apiDelete<void>(`/profile/sessions/${sessionId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-sessions"] });
    },
  });
}

export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}
