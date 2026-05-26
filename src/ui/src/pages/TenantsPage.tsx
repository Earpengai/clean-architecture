import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMyTenants, useAvailablePlans } from "@/api/tenants";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { TenantCard } from "@/features/tenants/components/TenantCard";
import { CreateTenantDialog } from "@/features/tenants/components/CreateTenantDialog";
import { Layers } from "lucide-react";

export function TenantsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: tenants, isLoading, error } = useMyTenants();
  const { data: plans } = useAvailablePlans();

  const tenantCount = tenants?.length ?? 0;

  const totalRemaining = plans
    ? plans.reduce((sum, p) => sum + (p.remainingQuota === -1 ? Number.MAX_SAFE_INTEGER : p.remainingQuota), 0)
    : null;

  const isUnlimited = totalRemaining !== null && totalRemaining >= Number.MAX_SAFE_INTEGER;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">My Tenants</h1>
          {totalRemaining !== null && (
            <span className="text-xs text-gray-500 flex items-center gap-1">
              <Layers className="h-3 w-3" />
              {isUnlimited
                ? `${tenantCount} created · Unlimited`
                : `${tenantCount} created · ${totalRemaining} remaining`}
            </span>
          )}
        </div>
        <CreateTenantDialog />
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <ErrorDisplay error={error} className="mb-4" />}

      {tenants && tenants.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No tenants yet. Create your first workspace.</p>
          <div className="mt-4">
            <CreateTenantDialog />
          </div>
        </div>
      )}

      {tenants && tenants.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {tenants.map((tenant) => (
            <TenantCard
              key={tenant.id}
              tenant={tenant}
              onClick={() => navigate(`/tenant/tenants/${tenant.id}`)}
            />
          ))}
        </div>
      )}
    </div>
  );
}
