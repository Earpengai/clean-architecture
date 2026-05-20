import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiPost, apiPut, setAuthToken, getAuthToken } from "./client";

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
  token: string;
  newPassword: string;
}

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: LoginPayload) => apiPost<string>("/users/login", payload),
    onSuccess: (token) => {
      setAuthToken(token);
      queryClient.clear();
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
    setAuthToken(null);
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
    mutationFn: (payload: { token: string }) => apiPost<void>("/users/verify-email", payload),
  });
}

export function useRequestEmailVerification() {
  return useMutation({
    mutationFn: () => apiPost<void>("/users/request-verification"),
  });
}

export function isAuthenticated(): boolean {
  return getAuthToken() !== null;
}
