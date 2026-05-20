import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useCreateTodo } from "@/api/todos";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";

interface TodoFormProps {
  userId: string;
}

export function TodoForm({ userId }: TodoFormProps) {
  const { t } = useTranslation();
  const [description, setDescription] = useState("");
  const createTodo = useCreateTodo();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!description.trim()) return;
    createTodo.mutate(
      { userId, description: description.trim() },
      { onSuccess: () => setDescription("") },
    );
  };

  return (
    <form onSubmit={handleSubmit} className="flex gap-2">
      <Input
        placeholder={t("todos.description")}
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        className="flex-1"
      />
      <Button type="submit" disabled={createTodo.isPending || !description.trim()}>
        <Plus className="h-4 w-4 mr-1" />
        {t("todos.addTodo")}
      </Button>
    </form>
  );
}
