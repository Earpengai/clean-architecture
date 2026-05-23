import { useNavigate } from "react-router-dom";
import type { TodoCardData } from "@/api/types";
import { useCompleteTodo, useDeleteTodo, useCopyTodo } from "@/api/todos";
import { useAuthStore } from "@/stores/authStore";
import { cn } from "@/lib/cn";
import { Check, Trash2, Copy, Circle } from "lucide-react";

const PRIORITY_LABELS: Record<number, string> = {
  4: "Top",
  3: "High",
  2: "Medium",
  1: "Low",
  0: "Normal",
};

const PRIORITY_BADGE: Record<number, string> = {
  4: "bg-red-100 text-red-700",
  3: "bg-orange-100 text-orange-700",
  2: "bg-yellow-100 text-yellow-700",
  1: "bg-blue-100 text-blue-700",
  0: "bg-gray-100 text-gray-600",
};

interface TodoItemProps {
  todo: TodoCardData;
  prefix?: React.ReactNode;
}

export function TodoItem({ todo, prefix }: TodoItemProps) {
  const navigate = useNavigate();
  const userId = useAuthStore((state) => state.userId);
  const completeTodo = useCompleteTodo();
  const deleteTodo = useDeleteTodo();
  const copyTodo = useCopyTodo();

  const completed = todo.isCompleted;

  const handleToggle = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!completed) {
      completeTodo.mutate(todo.id);
    }
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    deleteTodo.mutate(todo.id);
  };

  const handleCopy = (e: React.MouseEvent) => {
    e.stopPropagation();
    copyTodo.mutate({ id: todo.id, userId: userId! });
  };

  return (
    <div
      className={cn(
        "group rounded-md border p-4 cursor-pointer transition-colors hover:border-indigo-300",
        completed ? "border-gray-200 bg-gray-50" : "border-gray-200 bg-white",
      )}
      onClick={() => navigate(`/app/todos/${todo.id}`)}
    >
      <div className="flex items-start gap-3">
        {prefix}
        <button
          type="button"
          onClick={handleToggle}
          className={cn(
            "mt-0.5 h-5 w-5 rounded-full border-2 flex items-center justify-center transition-colors",
            completed
              ? "border-green-500 bg-green-500 text-white"
              : "border-gray-300 hover:border-indigo-400",
          )}
          disabled={completeTodo.isPending || completed}
        >
          {completed ? <Check className="h-3 w-3" /> : <Circle className="h-3 w-3 text-transparent" />}
        </button>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <p className={cn("text-sm", completed && "line-through text-gray-400")}>{todo.description}</p>
            <span className={cn("rounded px-1.5 py-0.5 text-xs font-medium", PRIORITY_BADGE[todo.priority] ?? "bg-gray-100 text-gray-600")}>
              {PRIORITY_LABELS[todo.priority] ?? `P${todo.priority}`}
            </span>
          </div>
          {todo.labels.length > 0 && (
            <div className="mt-2 flex flex-wrap gap-1">
              {todo.labels.map((label) => (
                <span key={label} className="rounded bg-indigo-100 px-2 py-0.5 text-xs font-medium text-indigo-700">
                  {label}
                </span>
              ))}
            </div>
          )}
          {todo.dueDate && (
            <p className="mt-1 text-xs text-gray-400">Due: {new Date(todo.dueDate).toLocaleDateString()}</p>
          )}
        </div>
        <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            type="button"
            onClick={handleCopy}
            className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-indigo-600"
            title="Copy"
            disabled={copyTodo.isPending}
          >
            <Copy className="h-4 w-4" />
          </button>
          <button
            type="button"
            onClick={handleDelete}
            className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600"
            title="Delete"
            disabled={deleteTodo.isPending}
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>
  );
}
