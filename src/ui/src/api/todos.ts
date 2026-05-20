import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost, apiPut, apiDelete } from "./client";
import type { TodoItem, CreateTodoPayload } from "./types";

const TODOS_KEY = ["todos"] as const;

export function useTodos(userId: string | null | undefined) {
  return useQuery({
    queryKey: [...TODOS_KEY, userId],
    queryFn: () => apiGet<TodoItem[]>(`/todos?userId=${encodeURIComponent(userId!)}`),
    enabled: Boolean(userId),
  });
}

export function useTodoById(id: string | null | undefined) {
  return useQuery({
    queryKey: [...TODOS_KEY, "detail", id],
    queryFn: () => apiGet<TodoItem>(`/todos/${encodeURIComponent(id!)}`),
    enabled: Boolean(id),
  });
}

export function useCreateTodo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTodoPayload) => apiPost<TodoItem>("/todos", payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: [...TODOS_KEY, variables.userId] });
    },
  });
}

export function useCompleteTodo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => apiPut<void>(`/todos/${encodeURIComponent(id)}/complete`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TODOS_KEY });
    },
  });
}

export function useDeleteTodo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => apiDelete<void>(`/todos/${encodeURIComponent(id)}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TODOS_KEY });
    },
  });
}

export function useCopyTodo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, userId }: { id: string; userId: string }) =>
      apiPost<string>(`/todos/${encodeURIComponent(id)}/copy`, { userId, todoId: id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: TODOS_KEY });
    },
  });
}
