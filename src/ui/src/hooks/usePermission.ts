import { usePermissionsStore } from "@/stores/permissionsStore";

export function usePermission(permission: string): boolean {
  return usePermissionsStore((state) => state.hasPermission(permission));
}

export function usePermissions(permissions: string[], requireAll = true): boolean {
  return usePermissionsStore((state) =>
    requireAll
      ? permissions.every((p) => state.hasPermission(p))
      : permissions.some((p) => state.hasPermission(p)),
  );
}
