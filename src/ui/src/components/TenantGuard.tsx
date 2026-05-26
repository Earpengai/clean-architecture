import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useMyTenants } from "@/api/tenants";
import { useTenantStore } from "@/stores/tenantStore";
import { TenantBlockedPage } from "@/components/TenantBlockedPage";

const ALLOWED_PATHS = ["/tenant/tenants", "/tenant/billing"];

export function TenantGuard() {
  const location = useLocation();
  const { data: tenants, isLoading } = useMyTenants();
  const activeTenantId = useTenantStore((state) => state.activeTenantId);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-200 border-t-indigo-600" />
      </div>
    );
  }

  const isManagementRoute = ALLOWED_PATHS.some(
    (p) => location.pathname.startsWith(p)
  );

  if (isManagementRoute) {
    return <Outlet />;
  }

  if (!activeTenantId || !tenants || tenants.length === 0) {
    if (tenants && tenants.length > 0) {
      return <Navigate to="/tenant/tenants" replace />;
    }
    return <Outlet />;
  }

  const activeTenant = tenants.find((t) => t.id === activeTenantId);

  if (!activeTenant) {
    return <Navigate to="/tenant/tenants" replace />;
  }

  if (!activeTenant.isActive) {
    return <TenantBlockedPage reason="disabled" />;
  }

  if (activeTenant.subscriptionStatus === 4) {
    return <TenantBlockedPage reason="expired" />;
  }

  if (
    activeTenant.subscriptionStatus === 1 &&
    activeTenant.subscriptionExpiresAt &&
    new Date(activeTenant.subscriptionExpiresAt) < new Date()
  ) {
    return <TenantBlockedPage reason="trial" />;
  }

  return <Outlet />;
}
