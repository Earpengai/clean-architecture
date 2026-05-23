import { useTranslation } from "react-i18next";
import { useTodosKanban } from "@/api/todos";
import { TodoItem } from "./TodoItem";
import { cn } from "@/lib/cn";

const PRIORITY_COLORS: Record<number, string> = {
  4: "border-red-300 bg-red-50",     // Top
  3: "border-orange-300 bg-orange-50", // High
  2: "border-yellow-300 bg-yellow-50", // Medium
  1: "border-blue-300 bg-blue-50",     // Low
  0: "border-gray-300 bg-gray-50",    // Normal/Backlog
};

export function TodoKanbanView() {
  const { t } = useTranslation();
  const { data, isLoading, error } = useTodosKanban();

  if (isLoading) {
    return <p className="text-sm text-gray-400">{t("todos.loading")}</p>;
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  const columns = data!.columns;

  if (columns.every((c) => c.items.length === 0)) {
    return <p className="text-sm text-gray-400">{t("todos.noTodos")}</p>;
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
      {columns.map((column) => (
        <div
          key={column.key}
          className={cn("rounded-lg border-2 p-3 min-h-[200px]", PRIORITY_COLORS[column.key] ?? "border-gray-200 bg-white")}
        >
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-gray-700">{column.label}</h3>
            <span className="rounded-full bg-white px-2 py-0.5 text-xs font-medium text-gray-500 border border-gray-200">
              {column.items.length}
            </span>
          </div>
          <div className="space-y-2">
            {column.items.map((todo) => (
              <TodoItem key={todo.id} todo={todo} />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
