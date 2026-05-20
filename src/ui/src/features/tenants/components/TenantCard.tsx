import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { TenantResponse } from "@/api/types";

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

interface TenantCardProps {
  tenant: TenantResponse;
  onClick?: () => void;
}

export function TenantCard({ tenant, onClick }: TenantCardProps) {
  const status = statusLabel(tenant.subscriptionStatus);

  return (
    <Card className="cursor-pointer hover:border-indigo-300 transition-colors" onClick={onClick}>
      <CardContent className="p-5">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h3 className="font-semibold text-gray-900 truncate">{tenant.name}</h3>
            <p className="text-xs text-gray-500 mt-0.5">{tenant.identifier}</p>
          </div>
          <Badge className={status.color}>{status.label}</Badge>
        </div>
        <div className="mt-3 flex items-center gap-4 text-xs text-gray-500">
          <span>Plan: {planLabel(tenant.subscriptionPlan)}</span>
          <span>Seats: {tenant.seatCount}</span>
          <span>Role: {tenant.role}</span>
        </div>
      </CardContent>
    </Card>
  );
}
