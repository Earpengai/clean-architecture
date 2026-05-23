import { create } from "zustand";

interface PermissionsState {
  permissions: Set<string>;
  tenantId: string | null;
  setPermissions: (tenantId: string, permissions: string[]) => void;
  hasPermission: (permission: string) => boolean;
  clearPermissions: () => void;
}

export const usePermissionsStore = create<PermissionsState>((set, get) => ({
  permissions: new Set<string>(),
  tenantId: null,

  setPermissions: (tenantId, permissions) =>
    set({ tenantId, permissions: new Set(permissions) }),

  hasPermission: (permission) => get().permissions.has(permission),

  clearPermissions: () => set({ permissions: new Set(), tenantId: null }),
}));
