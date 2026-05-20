import type { TodoItem as TodoItemType } from "@/api/types";
import { cn } from "@/lib/cn";

interface TodoItemProps {
  todo: TodoItemType;
}

export function TodoItem({ todo }: TodoItemProps) {
  const completed = todo.isCompleted;

  return (
    <div className={cn("rounded-md border p-4", completed ? "border-gray-200 bg-gray-50" : "border-gray-200 bg-white")}>
      <div className="flex items-start gap-3">
        <div
          className={cn("mt-1 h-4 w-4 rounded-full border-2", completed ? "border-green-500 bg-green-500" : "border-gray-300")}
        />
        <div className="flex-1 min-w-0">
          <p className={cn("text-sm", completed && "line-through text-gray-400")}>{todo.description}</p>
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
            <p className="mt-1 text-xs text-gray-400">
              Due: {new Date(todo.dueDate).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
