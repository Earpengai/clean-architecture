import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useTodosPaginated } from "@/api/todos";
import { TodoItem } from "./TodoItem";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Search, ChevronLeft, ChevronRight } from "lucide-react";

export function TodoPaginatedView() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");

  const { data, isLoading, error } = useTodosPaginated({ page, pageSize, search });

  const handleSearch = () => {
    setSearch(searchInput.trim());
    setPage(1);
  };

  if (isLoading) {
    return <p className="text-sm text-gray-400">{t("todos.loading")}</p>;
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  const paginatedList = data!;

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        <Input
          placeholder="Search todos..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter") handleSearch();
          }}
          className="flex-1"
        />
        <Button variant="outline" onClick={handleSearch}>
          <Search className="h-4 w-4" />
        </Button>
      </div>

      {paginatedList.items.length === 0 ? (
        <p className="text-sm text-gray-400">{t("todos.noTodos")}</p>
      ) : (
        <div className="space-y-3">
          {paginatedList.items.map((todo) => (
            <TodoItem key={todo.id} todo={todo} />
          ))}
        </div>
      )}

      <div className="flex items-center justify-between pt-2 text-sm text-gray-500">
        <div className="flex items-center gap-2">
          <span>
            Page {paginatedList.page} of {paginatedList.totalPages || 1} ({paginatedList.totalCount} items)
          </span>
          <select
            value={pageSize}
            onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}
            className="h-8 rounded border border-gray-300 bg-white px-2 text-xs"
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
        </div>
        <div className="flex gap-1">
          <Button
            variant="outline"
            size="sm"
            disabled={!paginatedList.hasPreviousPage}
            onClick={() => setPage((p) => p - 1)}
          >
            <ChevronLeft className="h-4 w-4 mr-1" />
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={!paginatedList.hasNextPage}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
            <ChevronRight className="h-4 w-4 ml-1" />
          </Button>
        </div>
      </div>
    </div>
  );
}
