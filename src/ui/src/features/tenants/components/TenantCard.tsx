import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { TenantResponse } from "@/api/types";

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

interface TenantCardProps {
  tenant: TenantResponse;
  onClick?: () => void;
}

export function TenantCard({ tenant, onClick }: TenantCardProps) {
  const status = statusLabel(tenant.subscriptionStatus);
  const isDisabled = !tenant.isActive;

  return (
    <Card
      className={`cursor-pointer transition-colors ${
        isDisabled
          ? "opacity-60 border-red-200 hover:border-red-300"
          : "hover:border-indigo-300"
      }`}
      onClick={onClick}
    >
      <CardContent className="p-5">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h3 className="font-semibold text-gray-900 truncate">
              {tenant.name}
              {isDisabled && (
                <span className="ml-2 text-xs text-red-500 font-normal">(Disabled)</span>
              )}
            </h3>
            <p className="text-xs text-gray-500 mt-0.5">{tenant.identifier}</p>
          </div>
          <div className="flex flex-col gap-1 items-end shrink-0">
            {tenant.isDemoData && (
              <Badge className="bg-amber-100 text-amber-700">Demo</Badge>
            )}
            {isDisabled && (
              <Badge className="bg-red-100 text-red-700">Disabled</Badge>
            )}
            <Badge className={status.color}>{status.label}</Badge>
          </div>
        </div>
        <div className="mt-3 flex items-center gap-4 text-xs text-gray-500">
          <span>Plan: {tenant.subscriptionPlanName ?? "None"}</span>
          <span>Role: {tenant.role}</span>
        </div>
        {tenant.subscriptionExpiresAt && (
          <div className="mt-2 text-xs text-gray-400">
            {tenant.subscriptionStatus === 1 ? "Trial ends" : "Expires"}:{" "}
            {new Date(tenant.subscriptionExpiresAt).toLocaleDateString()}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
