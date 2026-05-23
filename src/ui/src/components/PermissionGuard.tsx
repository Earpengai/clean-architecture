import { Outlet, Link } from "react-router-dom";
import { usePermissions } from "@/hooks/usePermission";
import { ShieldAlert } from "lucide-react";
import { Button } from "@/components/ui/button";

interface PermissionGuardProps {
  permissions: string[];
  requireAll?: boolean;
}

export function PermissionGuard({ permissions, requireAll = true }: PermissionGuardProps) {
  const granted = usePermissions(permissions, requireAll);

  if (!granted) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="text-center max-w-md">
          <div className="inline-flex h-16 w-16 items-center justify-center rounded-full bg-amber-100 mb-4">
            <ShieldAlert className="h-8 w-8 text-amber-600" />
          </div>
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Access Denied</h2>
          <p className="text-sm text-gray-500 mb-6">
            You don't have the required permissions to view this page.
          </p>
          <Button asChild variant="outline">
            <Link to="/app">Back to Dashboard</Link>
          </Button>
        </div>
      </div>
    );
  }

  return <Outlet />;
}
