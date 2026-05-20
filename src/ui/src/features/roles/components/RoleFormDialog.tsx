import { useState, useEffect } from "react";
import { useCreateRole, useUpdateRole } from "@/api/roles";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import type { RoleResponse, RoleFormPayload } from "@/api/types";
import { Plus, Pencil } from "lucide-react";

const ALL_PERMISSIONS = [
  "users:read",
  "users:write",
  "users:delete",
  "roles:read",
  "roles:write",
  "roles:delete",
  "tenants:read",
  "tenants:write",
  "tenants:delete",
];

interface RoleFormDialogProps {
  role?: RoleResponse | null;
  trigger?: React.ReactNode;
}

export function RoleFormDialog({ role, trigger }: RoleFormDialogProps) {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [permissions, setPermissions] = useState<string[]>([]);
  const [error, setError] = useState("");

  const isEdit = Boolean(role);
  const create = useCreateRole();
  const update = useUpdateRole();

  useEffect(() => {
    if (role) {
      setName(role.name);
      setDescription(role.description ?? "");
      setPermissions(role.permissions);
    } else {
      setName("");
      setDescription("");
      setPermissions([]);
    }
    setError("");
  }, [role, open]);

  const togglePermission = (perm: string) => {
    setPermissions((prev) =>
      prev.includes(perm) ? prev.filter((p) => p !== perm) : [...prev, perm],
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    const payload: RoleFormPayload = {
      name: name.trim(),
      description: description.trim() || null,
      permissions,
    };

    if (isEdit && role) {
      update.mutate(
        { id: role.id, payload },
        {
          onSuccess: () => setOpen(false),
          onError: (err) => setError(err.message),
        },
      );
    } else {
      create.mutate(payload, {
        onSuccess: () => setOpen(false),
        onError: (err) => setError(err.message),
      });
    }
  };

  const isPending = create.isPending || update.isPending;

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        {trigger ?? (
          <Button size="sm">
            {isEdit ? <Pencil className="h-4 w-4 mr-1" /> : <Plus className="h-4 w-4 mr-1" />}
            {isEdit ? "Edit" : "Create Role"}
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="max-w-lg">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{isEdit ? "Edit Role" : "Create Role"}</DialogTitle>
            <DialogDescription>
              {isEdit ? "Update role details and permissions." : "Define a new role with permissions."}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="roleName">Name</Label>
              <Input
                id="roleName"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Manager"
                required
                disabled={role?.isSystem}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="roleDescription">Description</Label>
              <Input
                id="roleDescription"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Optional description"
              />
            </div>
            <div className="space-y-2">
              <Label>Permissions</Label>
              <div className="grid grid-cols-2 gap-2">
                {ALL_PERMISSIONS.map((perm) => (
                  <label
                    key={perm}
                    className="flex items-center gap-2 rounded-md border border-gray-200 p-2 cursor-pointer hover:bg-gray-50"
                  >
                    <input
                      type="checkbox"
                      checked={permissions.includes(perm)}
                      onChange={() => togglePermission(perm)}
                      className="h-4 w-4 rounded border-gray-300 text-indigo-600"
                    />
                    <span className="text-sm">{perm}</span>
                  </label>
                ))}
              </div>
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Saving..." : isEdit ? "Save Changes" : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
