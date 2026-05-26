import { useState } from "react";
import { useCreateTenant, useAvailablePlans } from "@/api/tenants";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
import type { AvailablePlanResponse } from "@/api/types";
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
import { Badge } from "@/components/ui/badge";
import { Plus, Check, Zap, Users, FileText, HardDrive, Layers } from "lucide-react";

const FEATURE_LABELS: Record<string, string> = {
  "feature:api_access": "API Access",
  "feature:reporting": "Reporting",
  "feature:audit_log": "Audit Log",
  "feature:custom_domain": "Custom Domain",
  "feature:priority_support": "Priority Support",
  "feature:sso": "SSO",
  "feature:white_label": "White Label",
};

const LIMIT_LABELS: Record<string, { label: string; icon: React.ComponentType<{ className?: string }> }> = {
  "limit:max_users": { label: "Users", icon: Users },
  "limit:max_todos": { label: "Todos", icon: FileText },
  "limit:storage_mb": { label: "Storage (MB)", icon: HardDrive },
  "limit:max_tenants_per_user": { label: "Tenants per user", icon: Layers },
};

function UnlimitedBadge() {
  return <Badge className="bg-green-100 text-green-700 text-xs">Unlimited</Badge>;
}

function PlanCard({
  plan,
  selected,
  onSelect,
}: {
  plan: AvailablePlanResponse;
  selected: boolean;
  onSelect: () => void;
}) {
  const isExhausted = plan.remainingQuota === 0;

  return (
    <button
      type="button"
      onClick={isExhausted ? undefined : onSelect}
      disabled={isExhausted}
      className={`w-full text-left rounded-lg border-2 p-4 transition-colors ${
        isExhausted
          ? "border-gray-200 opacity-50 cursor-not-allowed"
          : selected
            ? "border-indigo-500 bg-indigo-50"
            : "border-gray-200 hover:border-indigo-300"
      }`}
    >
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-2">
          <Zap className="h-4 w-4 text-indigo-500" />
          <span className="font-semibold text-gray-900">{plan.name}</span>
          {selected && <Check className="h-4 w-4 text-indigo-600" />}
        </div>
        <Badge className={selected ? "bg-indigo-100 text-indigo-700" : "bg-gray-100 text-gray-700"}>
          {plan.trialDays > 0 ? `$${plan.priceMonthly}/mo · ${plan.trialDays}d trial` : "Free"}
        </Badge>
      </div>

      {plan.description && (
        <p className="text-xs text-gray-500 mb-2">{plan.description}</p>
      )}

      <div className="space-y-1.5 mb-2">
        {Object.entries(plan.limits).map(([key, value]) => {
          const info = LIMIT_LABELS[key];
          if (!info) return null;
          const Icon = info.icon;
          return (
            <div key={key} className="flex items-center gap-2 text-xs text-gray-600">
              <Icon className="h-3 w-3 text-gray-400" />
              <span>{info.label}:</span>
              {value === -1 ? <UnlimitedBadge /> : <span className="font-medium">{value}</span>}
            </div>
          );
        })}
      </div>

      <div className="flex flex-wrap gap-1 mt-3">
        {plan.features.map((f) => (
          <Badge key={f} className="bg-indigo-100 text-indigo-700 text-xs">
            {FEATURE_LABELS[f] ?? f}
          </Badge>
        ))}
        {plan.features.length === 0 && (
          <span className="text-xs text-gray-400">No additional features</span>
        )}
      </div>

      <div className="mt-2 text-xs text-gray-500">
        {isExhausted
          ? "Quota exhausted"
          : plan.remainingQuota === -1
            ? "No creation limit"
            : `${plan.remainingQuota} remaining`}
      </div>
    </button>
  );
}

export function CreateTenantDialog() {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [identifier, setIdentifier] = useState("");
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
  const [error, setError] = useState("");
  const create = useCreateTenant();
  const addToast = useToastStore((state) => state.addToast);
  const { data: plans, isLoading: plansLoading } = useAvailablePlans();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    create.mutate(
      {
        name: name.trim(),
        identifier: identifier.trim(),
        subscriptionPlanId: selectedPlanId ?? undefined,
      },
      {
        onSuccess: () => {
          setOpen(false);
          setName("");
          setIdentifier("");
          setSelectedPlanId(null);
        },
        onError: (err) => {
          const message = extractErrorDetail(err);
          setError(message);
          addToast(message, "error");
        },
      },
    );
  };

  const selectedPlan = plans?.find((p) => p.planId === selectedPlanId);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-1" />
          Create Tenant
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-lg">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Create Tenant</DialogTitle>
            <DialogDescription>Create a new organization workspace.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4 max-h-[60vh] overflow-y-auto">
            <div className="space-y-2">
              <Label htmlFor="tenantName">Name</Label>
              <Input id="tenantName" value={name} onChange={(e) => setName(e.target.value)} placeholder="Acme Inc." required />
            </div>
            <div className="space-y-2">
              <Label htmlFor="tenantIdentifier">Identifier</Label>
              <Input id="tenantIdentifier" value={identifier} onChange={(e) => setIdentifier(e.target.value)} placeholder="acme" required />
              <p className="text-xs text-gray-500">A unique URL-safe identifier for this tenant.</p>
            </div>

            <div className="space-y-2">
              <Label>Select Plan</Label>
              {plansLoading && <p className="text-sm text-gray-400">Loading plans...</p>}
              {plans && plans.length === 0 && <p className="text-sm text-gray-400">No plans available.</p>}
              <div className="grid gap-3">
                {plans?.map((plan) => (
                  <PlanCard
                    key={plan.planId}
                    plan={plan}
                    selected={selectedPlanId === plan.planId}
                    onSelect={() => setSelectedPlanId(plan.planId)}
                  />
                ))}
              </div>
            </div>

            {selectedPlan && (
              <p className="text-xs text-gray-500">
                {selectedPlan.trialDays > 0
                  ? `${selectedPlan.trialDays}-day free trial. You will not be charged until the trial ends.`
                  : "Free plan — no charges, no trial expiry."}
              </p>
            )}

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