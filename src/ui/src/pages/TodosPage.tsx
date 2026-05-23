import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuthStore } from "@/stores/authStore";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { TodoForm } from "@/features/todos/components/TodoForm";
import { TodoPaginatedView } from "@/features/todos/components/TodoPaginatedView";
import { TodoKanbanView } from "@/features/todos/components/TodoKanbanView";
import { TodoTreeView } from "@/features/todos/components/TodoTreeView";

type ViewMode = "paginated" | "kanban" | "tree";

export function TodosPage() {
  const { t } = useTranslation();
  const userId = useAuthStore((state) => state.userId);
  const [viewMode, setViewMode] = useState<ViewMode>("paginated");

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">{t("todos.title")}</h1>
      <div className="mt-6 space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>{t("todos.newTodo")}</CardTitle>
          </CardHeader>
          <CardContent>
            <TodoForm userId={userId} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>{t("todos.title")}</CardTitle>
            <select
              value={viewMode}
              onChange={(e) => setViewMode(e.target.value as ViewMode)}
              className="flex h-9 rounded-md border border-gray-300 bg-white px-3 py-1 text-sm text-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="paginated">List</option>
              <option value="kanban">Kanban</option>
              <option value="tree">Tree</option>
            </select>
          </CardHeader>
          <CardContent>
            {viewMode === "paginated" && <TodoPaginatedView />}
            {viewMode === "kanban" && <TodoKanbanView />}
            {viewMode === "tree" && <TodoTreeView />}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
