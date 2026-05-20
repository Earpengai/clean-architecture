import { useTranslation } from "react-i18next";
import { useTodos } from "@/api/todos";
import { TodoItem } from "./TodoItem";

interface TodoListProps {
  userId: string;
}

export function TodoList({ userId }: TodoListProps) {
  const { t } = useTranslation();
  const { data: todos, isLoading, error } = useTodos(userId);

  if (isLoading) {
    return <p className="text-sm text-gray-400">{t("todos.loading")}</p>;
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  if (!todos || todos.length === 0) {
    return <p className="text-sm text-gray-400">{t("todos.noTodos")}</p>;
  }

  return (
    <div className="space-y-3">
      {todos.map((todo) => (
        <TodoItem key={todo.id} todo={todo} />
      ))}
    </div>
  );
}
