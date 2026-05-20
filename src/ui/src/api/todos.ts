import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiGet, apiPost } from "./client";
import type { TodoItem, CreateTodoPayload } from "./types";

const TODOS_KEY = ["todos"] as const;

export function useTodos(userId: string | null | undefined) {
  return useQuery({
    queryKey: [...TODOS_KEY, userId],
    queryFn: () => apiGet<TodoItem[]>(`/todos?userId=${encodeURIComponent(userId!)}`),
    enabled: Boolean(userId),
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
