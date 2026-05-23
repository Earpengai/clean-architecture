import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useTodosTree } from "@/api/todos";
import type { TodoTreeItem } from "@/api/types";
import { TodoItem } from "./TodoItem";
import { ChevronRight, ChevronDown } from "lucide-react";

function TodoTreeNode({ item, depth }: { item: TodoTreeItem; depth: number }) {
  const [expanded, setExpanded] = useState(true);
  const hasChildren = item.children.length > 0;

  return (
    <div>
      <div style={{ marginLeft: `${depth * 20}px` }} className="flex items-center gap-1">
        {hasChildren ? (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setExpanded(!expanded);
            }}
            className="text-gray-400 hover:text-gray-600 shrink-0"
          >
            {expanded ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
          </button>
        ) : (
          <span className="w-4 shrink-0" />
        )}
        <div className="flex-1">
          <TodoItem todo={item} />
        </div>
      </div>
      {hasChildren && expanded && item.children.map((child) => (
        <TodoTreeNode key={child.id} item={child} depth={depth + 1} />
      ))}
    </div>
  );
}

export function TodoTreeView() {
  const { t } = useTranslation();
  const { data, isLoading, error } = useTodosTree();

  if (isLoading) {
    return <p className="text-sm text-gray-400">{t("todos.loading")}</p>;
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  const roots = data!.roots;

  if (roots.length === 0) {
    return <p className="text-sm text-gray-400">{t("todos.noTodos")}</p>;
  }

  return (
    <div className="space-y-1">
      {roots.map((root) => (
        <TodoTreeNode key={root.id} item={root} depth={0} />
      ))}
    </div>
  );
}
