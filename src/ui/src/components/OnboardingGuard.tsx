import { Navigate, Outlet } from "react-router-dom";
import { useMyTenants } from "@/api/tenants";

export function OnboardingGuard() {
  const { data: tenants, isLoading } = useMyTenants();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-200 border-t-indigo-600" />
      </div>
    );
  }

  if (!tenants || tenants.length === 0) {
    return <Navigate to="/tenant/tenants" replace />;
  }

  return <Outlet />;
}
