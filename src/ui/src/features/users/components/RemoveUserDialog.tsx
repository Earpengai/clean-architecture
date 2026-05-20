import { useState } from "react";
import { useRemoveUser } from "@/api/users";
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
import { UserMinus } from "lucide-react";

interface RemoveUserDialogProps {
  user: UserResponse;
}

export function RemoveUserDialog({ user }: RemoveUserDialogProps) {
  const [open, setOpen] = useState(false);
  const [error, setError] = useState("");
  const remove = useRemoveUser();

  const handleRemove = () => {
    setError("");
    remove.mutate(user.id, {
      onSuccess: () => setOpen(false),
      onError: (err) => setError(err.message),
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon" className="text-red-600 hover:bg-red-50" title="Remove from Tenant">
          <UserMinus className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Remove User</DialogTitle>
          <DialogDescription>
            Are you sure you want to remove <strong>{user.email}</strong> from this tenant? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        {error && <p className="text-sm text-red-500">{error}</p>}
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button variant="destructive" onClick={handleRemove} disabled={remove.isPending}>
            {remove.isPending ? "Removing..." : "Remove"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
