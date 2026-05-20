import { useState } from "react";
import { useCreateTenant } from "@/api/tenants";
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
import { Plus } from "lucide-react";

export function CreateTenantDialog() {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [identifier, setIdentifier] = useState("");
  const [error, setError] = useState("");
  const create = useCreateTenant();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    create.mutate(
      { name: name.trim(), identifier: identifier.trim() },
      {
        onSuccess: () => {
          setOpen(false);
          setName("");
          setIdentifier("");
        },
        onError: (err) => setError(err.message),
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-1" />
          Create Tenant
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Create Tenant</DialogTitle>
            <DialogDescription>Create a new organization workspace.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="tenantName">Name</Label>
              <Input id="tenantName" value={name} onChange={(e) => setName(e.target.value)} placeholder="Acme Inc." required />
            </div>
            <div className="space-y-2">
              <Label htmlFor="tenantIdentifier">Identifier</Label>
              <Input id="tenantIdentifier" value={identifier} onChange={(e) => setIdentifier(e.target.value)} placeholder="acme" required />
              <p className="text-xs text-gray-500">A unique URL-safe identifier for this tenant.</p>
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={create.isPending}>
              {create.isPending ? "Creating..." : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
