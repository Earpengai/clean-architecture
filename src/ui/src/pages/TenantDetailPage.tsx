import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useTenantById } from "@/api/tenants";
import { usePermission } from "@/hooks/usePermission";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Building2, Users, CreditCard } from "lucide-react";

function statusLabel(status: number | null) {
  if (status === null) return { label: "Unknown", color: "bg-gray-100 text-gray-700" };
  switch (status) {
    case 0: return { label: "Active", color: "bg-green-100 text-green-700" };
    case 1: return { label: "Trialing", color: "bg-blue-100 text-blue-700" };
    case 2: return { label: "Past Due", color: "bg-yellow-100 text-yellow-700" };
    case 3: return { label: "Canceled", color: "bg-red-100 text-red-700" };
    case 4: return { label: "Expired", color: "bg-gray-100 text-gray-700" };
    default: return { label: "Unknown", color: "bg-gray-100 text-gray-700" };
  }
}

export function TenantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const { data: tenant, isLoading, error } = useTenantById(id);
  const canReadUsers = usePermission("users:read");
  const canReadRoles = usePermission("roles:read");

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <p className="text-sm text-gray-400">{t("todos.loading")}</p>
      </div>
    );
  }

  if (error || !tenant) {
    return (
      <div className="text-center py-12">
        <ErrorDisplay error={error} />
        <Button variant="outline" className="mt-4" onClick={() => navigate("/tenant/tenants")}>
          <ArrowLeft className="h-4 w-4 mr-1" />
          Back to Tenants
        </Button>
      </div>
    );
  }

  const status = statusLabel(tenant.subscriptionStatus);

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate("/tenant/tenants")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-2xl font-bold text-gray-900">Tenant Details</h1>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Building2 className="h-5 w-5 text-indigo-600" />
                  <CardTitle>{tenant.name}</CardTitle>
                </div>
                <Badge className={status.color}>{status.label}</Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-xs font-medium text-gray-500">Identifier</p>
                  <p className="text-gray-700">{tenant.identifier}</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-gray-500">Your Role</p>
                  <p className="text-gray-700">{tenant.role}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center gap-2">
                <CreditCard className="h-5 w-5 text-indigo-600" />
                <CardTitle>Subscription</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Plan</span>
                <span className="font-medium">{tenant.subscriptionPlanName ?? "None"}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Status</span>
                <span className="font-medium">{status.label}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Seats</span>
                <span className="font-medium">{tenant.seatCount}</span>
              </div>
              {tenant.billingPeriod && tenant.billingPeriod !== "None" && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Billing</span>
                  <span className="font-medium">{tenant.billingPeriod}</span>
                </div>
              )}
              {tenant.subscriptionExpiresAt && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Expires</span>
                  <span className="font-medium">{new Date(tenant.subscriptionExpiresAt).toLocaleDateString()}</span>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center gap-2">
                <Users className="h-5 w-5 text-indigo-600" />
                <CardTitle>Actions</CardTitle>
              </div>
            </CardHeader>
            <CardContent className="space-y-2">
              {canReadUsers && (
                <Button variant="outline" className="w-full" onClick={() => navigate("/tenant/users")}>
                  Manage Members
                </Button>
              )}
              {canReadRoles && (
                <Button variant="outline" className="w-full" onClick={() => navigate("/tenant/roles")}>
                  Manage Roles
                </Button>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
