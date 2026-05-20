import { useState } from "react";
import { useDeleteRole } from "@/api/roles";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import type { RoleResponse } from "@/api/types";
import { Trash2 } from "lucide-react";

interface DeleteRoleDialogProps {
  role: RoleResponse;
}

export function DeleteRoleDialog({ role }: DeleteRoleDialogProps) {
  const [open, setOpen] = useState(false);
  const [error, setError] = useState("");
  const deleteRole = useDeleteRole();

  const handleDelete = () => {
    setError("");
    deleteRole.mutate(role.id, {
      onSuccess: () => setOpen(false),
      onError: (err) => setError(err.message),
    });
  };

  if (role.isSystem) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon" className="text-red-600 hover:bg-red-50">
          <Trash2 className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Role</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete the role <strong>{role.name}</strong>? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        {error && <p className="text-sm text-red-500">{error}</p>}
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={deleteRole.isPending}>
            {deleteRole.isPending ? "Deleting..." : "Delete"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
