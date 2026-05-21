import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMyTenants } from "@/api/tenants";
import { TenantCard } from "@/features/tenants/components/TenantCard";
import { CreateTenantDialog } from "@/features/tenants/components/CreateTenantDialog";

export function TenantsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: tenants, isLoading, error } = useMyTenants();

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">My Tenants</h1>
        <CreateTenantDialog />
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <p className="text-sm text-red-500">{t("todos.error")}</p>}

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
