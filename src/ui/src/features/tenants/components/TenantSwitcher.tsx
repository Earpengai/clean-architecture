import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useMyTenants } from "@/api/tenants";
import { useTenantStore } from "@/stores/tenantStore";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { Building2, ChevronDown, Check } from "lucide-react";

export function TenantSwitcher() {
  const navigate = useNavigate();
  const { data: tenants, isLoading } = useMyTenants();
  const activeTenantId = useTenantStore((state) => state.activeTenantId);
  const setActiveTenant = useTenantStore((state) => state.setActiveTenant);

  useEffect(() => {
    if (isLoading || !tenants || tenants.length === 0) {
      return;
    }

    const exists = tenants.some((t) => t.id === activeTenantId);

    if (!exists) {
      const firstActive = tenants.find(
        (t) => t.isActive && t.subscriptionStatus !== 4
      );
      if (firstActive) {
        setActiveTenant(firstActive.id, firstActive.identifier);
      }
    }
  }, [tenants, isLoading, activeTenantId, setActiveTenant]);

  if (isLoading) {
    return (
      <Button variant="ghost" size="sm" className="w-40 justify-between" disabled>
        <Building2 className="h-4 w-4" />
        <span className="text-gray-400">Loading...</span>
      </Button>
    );
  }

  const activeTenant = tenants?.find((t) => t.id === activeTenantId) ?? tenants?.[0] ?? null;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="sm" className="w-44 justify-between">
          <div className="flex items-center gap-2 min-w-0">
            <Building2 className="h-4 w-4 shrink-0" />
            <span className="truncate">{activeTenant?.name ?? "No tenant"}</span>
          </div>
          <ChevronDown className="h-3 w-3 shrink-0 text-gray-400" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="start" className="w-44">
        <DropdownMenuLabel>My Tenants</DropdownMenuLabel>
        {tenants?.map((tenant) => (
          <DropdownMenuItem
            key={tenant.id}
            onClick={() => {
              if (tenant.isActive && tenant.subscriptionStatus !== 4) {
                setActiveTenant(tenant.id, tenant.identifier);
              }
            }}
            className={`justify-between ${
              !tenant.isActive || tenant.subscriptionStatus === 4
                ? "opacity-50 cursor-not-allowed"
                : ""
            }`}
          >
            <span className="flex items-center gap-2">
              <span
                className={`h-2 w-2 rounded-full shrink-0 ${
                  !tenant.isActive
                    ? "bg-red-400"
                    : tenant.subscriptionStatus === 4
                      ? "bg-gray-400"
                      : tenant.subscriptionStatus === 1
                        ? "bg-yellow-400"
                        : "bg-green-400"
                }`}
              />
              {tenant.name}
            </span>
            {tenant.id === (activeTenantId ?? tenants?.[0]?.id) && (
              <Check className="h-4 w-4 text-indigo-600" />
            )}
          </DropdownMenuItem>
        ))}
        <DropdownMenuItem onClick={() => navigate("/tenant/tenants")}>
          <span className="text-indigo-600">Manage tenants...</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
