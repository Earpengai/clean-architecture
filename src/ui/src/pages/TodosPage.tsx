import { useTranslation } from "react-i18next";
import { useAuthStore } from "@/stores/authStore";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { TodoList } from "@/features/todos/components/TodoList";
import { TodoForm } from "@/features/todos/components/TodoForm";

export function TodosPage() {
  const { t } = useTranslation();
  const userId = useAuthStore((state) => state.userId);

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
          <CardHeader>
            <CardTitle>{t("todos.title")}</CardTitle>
          </CardHeader>
          <CardContent>
            <TodoList userId={userId} />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
