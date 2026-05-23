import { type ReactNode } from "react";
import { usePermissions } from "@/hooks/usePermission";

interface PermissionGateProps {
  permission?: string;
  permissions?: string[];
  requireAll?: boolean;
  children: ReactNode;
  fallback?: ReactNode;
}

export function PermissionGate({
  permission,
  permissions,
  requireAll = true,
  children,
  fallback = null,
}: PermissionGateProps) {
  const perms = permissions ?? (permission ? [permission] : []);
  const granted = usePermissions(perms, requireAll);

  if (!granted) {
    return fallback as React.ReactElement | null;
  }

  return children as React.ReactElement;
}
