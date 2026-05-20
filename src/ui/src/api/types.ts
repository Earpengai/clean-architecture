export interface TodoItem {
  id: string;
  userId: string;
  description: string;
  dueDate: string | null;
  labels: string[];
  isCompleted: boolean;
  createdAt: string;
  completedAt: string | null;
}

export interface CreateTodoPayload {
  userId: string;
  description: string;
  dueDate?: string | null;
  labels?: string[];
  priority?: number;
}
