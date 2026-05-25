import { useState } from "react";
import {
  useAdminSubscriptionPlans,
  useCreateSubscriptionPlan,
  useUpdateSubscriptionPlan,
  useDeactivateSubscriptionPlan,
} from "@/api/admin";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Plus, Edit, Archive } from "lucide-react";

interface PlanFormState {
  name: string;
  description: string;
  priceMonthly: number;
  priceYearly: number;
  trialDays: number;
  sortOrder: number;
  isActive: boolean;
}

const defaultForm: PlanFormState = {
  name: "",
  description: "",
  priceMonthly: 0,
  priceYearly: 0,
  trialDays: 0,
  sortOrder: 0,
  isActive: true,
};

function PlanFormDialog({
  open,
  onOpenChange,
  plan,
  onSave,
  isPending,
  error,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  plan?: PlanFormState & { id?: string };
  onSave: (data: PlanFormState) => void;
  isPending: boolean;
  error: string;
}) {
  const [form, setForm] = useState<PlanFormState>(plan ?? defaultForm);
  const isEdit = Boolean(plan?.id);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            onSave(form);
          }}
        >
          <DialogHeader>
            <DialogTitle>{isEdit ? "Edit Plan" : "Create Plan"}</DialogTitle>
            <DialogDescription>
              {isEdit
                ? "Update subscription plan details."
                : "Create a new subscription plan."}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-3 py-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label>Name *</Label>
                <Input
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  required
                  maxLength={100}
                />
              </div>
              <div className="space-y-1">
                <Label>Sort Order</Label>
                <Input
                  type="number"
                  min={0}
                  value={form.sortOrder}
                  onChange={(e) => setForm((f) => ({ ...f, sortOrder: Number(e.target.value) }))}
                />
              </div>
            </div>
            <div className="space-y-1">
              <Label>Description</Label>
              <Input
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                maxLength={500}
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label>Price Monthly ($)</Label>
                <Input
                  type="number"
                  min={0}
                  step={0.01}
                  value={form.priceMonthly}
                  onChange={(e) => setForm((f) => ({ ...f, priceMonthly: Number(e.target.value) }))}
                />
              </div>
              <div className="space-y-1">
                <Label>Price Yearly ($)</Label>
                <Input
                  type="number"
                  min={0}
                  step={0.01}
                  value={form.priceYearly}
                  onChange={(e) => setForm((f) => ({ ...f, priceYearly: Number(e.target.value) }))}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label>Trial Days</Label>
                <Input
                  type="number"
                  min={0}
                  value={form.trialDays}
                  onChange={(e) => setForm((f) => ({ ...f, trialDays: Number(e.target.value) }))}
                />
              </div>
              <div className="space-y-1 flex items-end pb-2">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                    className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                  />
                  <span className="text-sm">Active</span>
                </label>
              </div>
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Saving..." : isEdit ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function AdminPlansPage() {
  const { data: plans, isLoading, error } = useAdminSubscriptionPlans();
  const createPlan = useCreateSubscriptionPlan();
  const updatePlan = useUpdateSubscriptionPlan();
  const deactivatePlan = useDeactivateSubscriptionPlan();
  const addToast = useToastStore((state) => state.addToast);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<(PlanFormState & { id: string }) | undefined>();
  const [formError, setFormError] = useState("");

  const handleCreate = () => {
    setEditingPlan(undefined);
    setFormError("");
    setDialogOpen(true);
  };

  const handleEdit = (plan: PlanFormState & { id: string }) => {
    setEditingPlan(plan);
    setFormError("");
    setDialogOpen(true);
  };

  const handleSave = (data: PlanFormState) => {
    setFormError("");
    if (editingPlan?.id) {
      updatePlan.mutate(
        { ...data, id: editingPlan.id },
        {
          onSuccess: () => setDialogOpen(false),
          onError: (err) => {
            const message = extractErrorDetail(err);
            setFormError(message);
            addToast(message, "error");
          },
        },
      );
    } else {
      createPlan.mutate(data, {
        onSuccess: () => setDialogOpen(false),
        onError: (err) => {
          const message = extractErrorDetail(err);
          setFormError(message);
          addToast(message, "error");
        },
      });
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Admin — Plans</h1>
        <Button onClick={handleCreate}>
          <Plus className="h-4 w-4 mr-1" />
          Create Plan
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-3">
          {[...Array(3)].map((_, i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
      )}
      {error && <ErrorDisplay error={error} className="mb-4" />}

      {plans && plans.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No plans found.</p>
        </div>
      )}

      {plans && plans.length > 0 && (
        <div className="grid gap-3">
          {plans.map((plan) => (
            <div
              key={plan.id}
              className="flex items-center justify-between p-4 rounded-lg border border-gray-200 bg-white"
            >
              <div className="flex items-center gap-4">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-semibold text-gray-900">{plan.name}</span>
                    {plan.isActive ? (
                      <Badge className="bg-green-100 text-green-700">Active</Badge>
                    ) : (
                      <Badge className="bg-gray-100 text-gray-500">Inactive</Badge>
                    )}
                  </div>
                  {plan.description && (
                    <p className="text-xs text-gray-500 mb-1">{plan.description}</p>
                  )}
                  <div className="flex items-center gap-4 text-xs text-gray-500">
                    <span>Monthly: ${plan.priceMonthly.toFixed(2)}</span>
                    <span>Yearly: ${plan.priceYearly.toFixed(2)}</span>
                    <span>Trial: {plan.trialDays}d</span>
                    <span>Subscribers: {plan.subscriptionCount}</span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-1">
                <Button
                  variant="ghost"
                  size="icon"
                  title="Edit"
                  onClick={() =>
                    handleEdit({
                      id: plan.id,
                      name: plan.name,
                      description: plan.description ?? "",
                      priceMonthly: plan.priceMonthly,
                      priceYearly: plan.priceYearly,
                      trialDays: plan.trialDays,
                      sortOrder: plan.sortOrder,
                      isActive: plan.isActive,
                    })
                  }
                >
                  <Edit className="h-4 w-4" />
                </Button>
                {plan.isActive && (
                  <Button
                    variant="ghost"
                    size="icon"
                    title="Deactivate"
                    onClick={() =>
                      deactivatePlan.mutate(plan.id, {
                        onError: (err) => addToast(extractErrorDetail(err), "error"),
                      })
                    }
                    disabled={deactivatePlan.isPending}
                  >
                    <Archive className="h-4 w-4 text-gray-400" />
                  </Button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <PlanFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        plan={editingPlan}
        onSave={handleSave}
        isPending={createPlan.isPending || updatePlan.isPending}
        error={formError}
      />
    </div>
  );
}
