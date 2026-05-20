import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAdminTenants, useEnableTenant, useDisableTenant, useUpdateTenantSubscription } from "@/api/admin";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
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
import { Building2, Power, PowerOff, CreditCard } from "lucide-react";

function planLabel(plan: number) {
  switch (plan) {
    case 0: return "Free";
    case 1: return "Pro";
    case 2: return "Enterprise";
    default: return "Unknown";
  }
}

function statusLabel(status: number) {
  switch (status) {
    case 0: return { label: "Active", color: "bg-green-100 text-green-700" };
    case 1: return { label: "Trialing", color: "bg-blue-100 text-blue-700" };
    case 2: return { label: "Past Due", color: "bg-yellow-100 text-yellow-700" };
    case 3: return { label: "Canceled", color: "bg-red-100 text-red-700" };
    case 4: return { label: "Expired", color: "bg-gray-100 text-gray-700" };
    default: return { label: "Unknown", color: "bg-gray-100 text-gray-700" };
  }
}

function SubscriptionDialog({ tenant }: { tenant: import("@/api/types").TenantAdminResponse }) {
  const [open, setOpen] = useState(false);
  const [plan, setPlan] = useState(tenant.subscriptionPlan);
  const [status, setStatus] = useState(tenant.subscriptionStatus);
  const [seats, setSeats] = useState(tenant.seatCount);
  const [error, setError] = useState("");

  const update = useUpdateTenantSubscription();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    update.mutate(
      { id: tenant.id, subscriptionPlan: plan, subscriptionStatus: status, seatCount: seats },
      {
        onSuccess: () => setOpen(false),
        onError: (err) => setError(err.message),
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon" title="Edit Subscription">
          <CreditCard className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Update Subscription</DialogTitle>
            <DialogDescription>Change plan, status, and seats for {tenant.name}.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label>Plan</Label>
                <select value={plan} onChange={(e) => setPlan(Number(e.target.value))} className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm">
                  <option value={0}>Free</option>
                  <option value={1}>Pro</option>
                  <option value={2}>Enterprise</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label>Status</Label>
                <select value={status} onChange={(e) => setStatus(Number(e.target.value))} className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm">
                  <option value={0}>Active</option>
                  <option value={1}>Trialing</option>
                  <option value={2}>Past Due</option>
                  <option value={3}>Canceled</option>
                  <option value={4}>Expired</option>
                </select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>Seats</Label>
              <Input type="number" min={1} value={seats} onChange={(e) => setSeats(Number(e.target.value))} />
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={update.isPending}>
              {update.isPending ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function AdminTenantsPage() {
  const { t } = useTranslation();
  const { data: tenants, isLoading, error } = useAdminTenants();
  const enable = useEnableTenant();
  const disable = useDisableTenant();

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Admin — Tenants</h1>
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <p className="text-sm text-red-500">{t("todos.error")}</p>}

      {tenants && tenants.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No tenants found.</p>
        </div>
      )}

      {tenants && tenants.length > 0 && (
        <div className="space-y-3">
          {tenants.map((tenant) => {
            const status = statusLabel(tenant.subscriptionStatus);
            return (
              <Card key={tenant.id}>
                <CardContent className="p-4">
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-indigo-100">
                        <Building2 className="h-4 w-4 text-indigo-600" />
                      </div>
                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-medium text-gray-900 truncate">{tenant.name}</p>
                          <Badge className={tenant.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}>
                            {tenant.isActive ? "Active" : "Disabled"}
                          </Badge>
                        </div>
                        <p className="text-xs text-gray-500">{tenant.identifier}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-xs text-gray-500">
                      <span>Plan: {planLabel(tenant.subscriptionPlan)}</span>
                      <Badge className={status.color}>{status.label}</Badge>
                      <span>Seats: {tenant.seatCount}</span>
                      <div className="flex items-center gap-1 ml-2">
                        {tenant.isActive ? (
                          <Button
                            variant="ghost"
                            size="icon"
                            className="text-red-600 hover:bg-red-50"
                            onClick={() => disable.mutate(tenant.id)}
                            disabled={disable.isPending}
                            title="Disable"
                          >
                            <PowerOff className="h-4 w-4" />
                          </Button>
                        ) : (
                          <Button
                            variant="ghost"
                            size="icon"
                            className="text-green-600 hover:bg-green-50"
                            onClick={() => enable.mutate(tenant.id)}
                            disabled={enable.isPending}
                            title="Enable"
                          >
                            <Power className="h-4 w-4" />
                          </Button>
                        )}
                        <SubscriptionDialog tenant={tenant} />
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
