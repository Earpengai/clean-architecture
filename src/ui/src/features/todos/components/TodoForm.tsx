import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useCreateTodo } from "@/api/todos";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Plus, X } from "lucide-react";

interface TodoFormProps {
  userId: string;
}

export function TodoForm({ userId }: TodoFormProps) {
  const { t } = useTranslation();
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [labels, setLabels] = useState<string[]>([]);
  const [labelInput, setLabelInput] = useState("");
  const [priority, setPriority] = useState(0);
  const createTodo = useCreateTodo();

  const addLabel = () => {
    const trimmed = labelInput.trim();
    if (trimmed && !labels.includes(trimmed)) {
      setLabels([...labels, trimmed]);
      setLabelInput("");
    }
  };

  const removeLabel = (label: string) => {
    setLabels(labels.filter((l) => l !== label));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!description.trim()) return;
    createTodo.mutate(
      {
        userId,
        description: description.trim(),
        dueDate: dueDate ? new Date(dueDate).toISOString() : null,
        labels: labels.length > 0 ? labels : undefined,
        priority,
      },
      {
        onSuccess: () => {
          setDescription("");
          setDueDate("");
          setLabels([]);
          setPriority(0);
        },
      },
    );
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="flex gap-2">
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
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1">
          <Label htmlFor="dueDate">Due Date</Label>
          <Input
            id="dueDate"
            type="datetime-local"
            value={dueDate}
            onChange={(e) => setDueDate(e.target.value)}
          />
        </div>
        <div className="space-y-1">
          <Label htmlFor="priority">Priority</Label>
          <select
            id="priority"
            value={priority}
            onChange={(e) => setPriority(Number(e.target.value))}
            className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
          >
            <option value={0}>Low</option>
            <option value={1}>Medium</option>
            <option value={2}>High</option>
          </select>
        </div>
      </div>

      <div className="space-y-1">
        <Label>Labels</Label>
        <div className="flex gap-2">
          <Input
            placeholder="Add label..."
            value={labelInput}
            onChange={(e) => setLabelInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                addLabel();
              }
            }}
          />
          <Button type="button" variant="outline" onClick={addLabel}>
            Add
          </Button>
        </div>
        {labels.length > 0 && (
          <div className="mt-2 flex flex-wrap gap-1">
            {labels.map((label) => (
              <span
                key={label}
                className="inline-flex items-center gap-1 rounded bg-indigo-100 px-2 py-0.5 text-xs font-medium text-indigo-700"
              >
                {label}
                <button type="button" onClick={() => removeLabel(label)} className="hover:text-indigo-900">
                  <X className="h-3 w-3" />
                </button>
              </span>
            ))}
          </div>
        )}
      </div>
    </form>
  );
}
