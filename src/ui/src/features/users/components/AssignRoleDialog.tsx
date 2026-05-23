import { useState } from "react";
import { useAssignRole } from "@/api/users";
import { useRoles } from "@/api/roles";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
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
import type { UserResponse } from "@/api/types";
import { UserCog } from "lucide-react";

interface AssignRoleDialogProps {
  user: UserResponse;
}

export function AssignRoleDialog({ user }: AssignRoleDialogProps) {
  const [open, setOpen] = useState(false);
  const [roleId, setRoleId] = useState(user.roleId ?? "");
  const [error, setError] = useState("");

  const assign = useAssignRole();
  const { data: roles } = useRoles();
  const addToast = useToastStore((state) => state.addToast);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    assign.mutate(
      { userId: user.id, roleId },
      {
        onSuccess: () => setOpen(false),
        onError: (err) => {
          const message = extractErrorDetail(err);
          setError(message);
          addToast(message, "error");
        },
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon" title="Assign Role">
          <UserCog className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Assign Role</DialogTitle>
            <DialogDescription>Change the role for {user.email}.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <select
              value={roleId}
              onChange={(e) => setRoleId(e.target.value)}
              required
              className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
            >
              <option value="">Select a role...</option>
              {roles?.map((role) => (
                <option key={role.id} value={role.id}>
                  {role.name}
                </option>
              ))}
            </select>
            {error && <p className="text-sm text-red-500">{error}</p>}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={assign.isPending}>
              {assign.isPending ? "Saving..." : "Assign Role"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
