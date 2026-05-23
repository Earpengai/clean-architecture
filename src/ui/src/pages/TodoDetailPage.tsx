import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useTodoById, useCompleteTodo, useDeleteTodo } from "@/api/todos";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/cn";
import { ArrowLeft, Check, Trash2 } from "lucide-react";

export function TodoDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const { data: todo, isLoading, error } = useTodoById(id);
  const completeTodo = useCompleteTodo();
  const deleteTodo = useDeleteTodo();
  const addToast = useToastStore((state) => state.addToast);

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <p className="text-sm text-gray-400">{t("todos.loading")}</p>
      </div>
    );
  }

  if (error || !todo) {
    return (
      <div className="text-center py-12">
        <ErrorDisplay error={error} />
        <Button variant="outline" className="mt-4" onClick={() => navigate("/app/todos")}>
          <ArrowLeft className="h-4 w-4 mr-1" />
          Back to Todos
        </Button>
      </div>
    );
  }

  const completed = todo.isCompleted;

  const handleDelete = () => {
    deleteTodo.mutate(todo.id, {
      onSuccess: () => navigate("/app/todos"),
      onError: (err) => addToast(extractErrorDetail(err), "error"),
    });
  };

  const handleComplete = () => {
    if (!completed) {
      completeTodo.mutate(todo.id, {
        onError: (err) => addToast(extractErrorDetail(err), "error"),
      });
    }
  };

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate("/app/todos")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-2xl font-bold text-gray-900">Todo Detail</h1>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-start justify-between gap-4">
            <div>
              <div className="flex items-center gap-2 mb-2">
                <div
                  className={cn(
                    "h-3 w-3 rounded-full",
                    completed ? "bg-green-500" : "bg-gray-300",
                  )}
                />
                <span className={cn("text-xs font-medium", completed ? "text-green-600" : "text-gray-500")}>
                  {completed ? "Completed" : "Pending"}
                </span>
              </div>
              <CardTitle className={cn(completed && "line-through text-gray-400")}>{todo.description}</CardTitle>
            </div>
            <div className="flex gap-2">
              {!completed && (
                <Button variant="outline" size="sm" onClick={handleComplete} disabled={completeTodo.isPending}>
                  <Check className="h-4 w-4 mr-1" />
                  Complete
                </Button>
              )}
              <Button variant="outline" size="sm" className="text-red-600 hover:bg-red-50" onClick={handleDelete} disabled={deleteTodo.isPending}>
                <Trash2 className="h-4 w-4 mr-1" />
                Delete
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {todo.labels.length > 0 && (
            <div>
              <p className="text-xs font-medium text-gray-500 mb-1">Labels</p>
              <div className="flex flex-wrap gap-1">
                {todo.labels.map((label) => (
                  <span key={label} className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-medium text-indigo-700">
                    {label}
                  </span>
                ))}
              </div>
            </div>
          )}
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <p className="text-xs font-medium text-gray-500">Created</p>
              <p className="text-gray-700">{new Date(todo.createdAt).toLocaleString()}</p>
            </div>
            {todo.dueDate && (
              <div>
                <p className="text-xs font-medium text-gray-500">Due Date</p>
                <p className="text-gray-700">{new Date(todo.dueDate).toLocaleString()}</p>
              </div>
            )}
            {todo.completedAt && (
              <div>
                <p className="text-xs font-medium text-gray-500">Completed</p>
                <p className="text-gray-700">{new Date(todo.completedAt).toLocaleString()}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
