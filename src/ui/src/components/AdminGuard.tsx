import { Navigate, Outlet } from "react-router-dom";
import { useIsAdmin } from "@/hooks/useIsAdmin";

export function AdminGuard() {
  const isAdmin = useIsAdmin();

  if (!isAdmin) {
    return <Navigate to="/app" replace />;
  }

  return <Outlet />;
}
