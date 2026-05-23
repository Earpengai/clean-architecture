import { useEffect } from "react";
import { useMyPermissions } from "@/api/permissions";
import { usePermissionsStore } from "@/stores/permissionsStore";
import { useTenantStore } from "@/stores/tenantStore";

export function PermissionsSync() {
  const { data } = useMyPermissions();
  const activeTenantId = useTenantStore((state) => state.activeTenantId);
  const clearPermissions = usePermissionsStore((state) => state.clearPermissions);
  const setPermissions = usePermissionsStore((state) => state.setPermissions);
  const storeTenantId = usePermissionsStore((state) => state.tenantId);

  useEffect(() => {
    if (data && data.permissions && activeTenantId) {
      if (activeTenantId !== storeTenantId) {
        setPermissions(activeTenantId, data.permissions);
      }
    }
  }, [data, activeTenantId, storeTenantId, setPermissions]);

  useEffect(() => {
    if (activeTenantId === null) {
      clearPermissions();
    }
  }, [activeTenantId, clearPermissions]);

  return null;
}
