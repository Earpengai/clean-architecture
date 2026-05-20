import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiPost, setAuthToken, getAuthToken } from "./client";

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

export function isAuthenticated() {
  return getAuthToken() !== null;
}
