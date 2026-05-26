import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAdminTenants, useEnableTenant, useDisableTenant, useUpdateTenantSubscription, useAdminSubscriptionPlans } from "@/api/admin";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
import { ErrorDisplay } from "@/components/ErrorDisplay";
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

function statusLabel(status: number | null) {
  if (status === null) return { label: "Unknown", color: "bg-gray-100 text-gray-700" };
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
  const [planId, setPlanId] = useState("");
  const [status, setStatus] = useState(tenant.subscriptionStatus ?? 0);
  const [maxUsersOverride, setMaxUsersOverride] = useState(tenant.maxUsersOverride);
  const [error, setError] = useState("");

  const { data: plans } = useAdminSubscriptionPlans();
  const update = useUpdateTenantSubscription();
  const addToast = useToastStore((state) => state.addToast);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!planId) {
      setError("Please select a plan.");
      return;
    }
    setError("");
    update.mutate(
      { id: tenant.id, subscriptionPlanId: planId, subscriptionStatus: status, maxUsersOverride },
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
    <Dialog open={open} onOpenChange={(isOpen) => { setOpen(isOpen); if (!isOpen) setPlanId(""); }}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon" title="Edit Subscription">
          <CreditCard className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Update Subscription</DialogTitle>
            <DialogDescription>Change plan, status, and max users for {tenant.name}.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label>Plan</Label>
                <select value={planId} onChange={(e) => setPlanId(e.target.value)} className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm">
                  <option value="">Select plan...</option>
                  {plans?.map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
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
              <Label>Max Users Override</Label>
              <Input type="number" min={1} value={maxUsersOverride ?? ""} placeholder="Plan default" onChange={(e) => setMaxUsersOverride(e.target.value ? Number(e.target.value) : null)} />
              <p className="text-xs text-gray-400">Leave empty to use the plan's default limit.</p>
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
  const addToast = useToastStore((state) => state.addToast);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Admin — Tenants</h1>
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <ErrorDisplay error={error} className="mb-4" />}

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
                          {tenant.isDemoData && (
                            <Badge className="bg-amber-100 text-amber-700">Demo</Badge>
                          )}
                          <Badge className={tenant.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}>
                            {tenant.isActive ? "Active" : "Disabled"}
                          </Badge>
                        </div>
                        <p className="text-xs text-gray-500">{tenant.identifier}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-xs text-gray-500">
                      <span>Plan: {tenant.subscriptionPlanName ?? "None"}</span>
                      <Badge className={status.color}>{status.label}</Badge>
                      <span>Max Users: {tenant.maxUsersOverride ?? "Default"}</span>
                      <div className="flex items-center gap-1 ml-2">
                        {tenant.isActive ? (
                          <Button
                            variant="ghost"
                            size="icon"
                            className="text-red-600 hover:bg-red-50"
                            onClick={() => disable.mutate(tenant.id, {
                              onError: (err) => addToast(extractErrorDetail(err), "error"),
                            })}
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
                            onClick={() => enable.mutate(tenant.id, {
                              onError: (err) => addToast(extractErrorDetail(err), "error"),
                            })}
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
