import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMyTenants } from "@/api/tenants";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Building2 } from "lucide-react";

export function Dashboard() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: tenants } = useMyTenants();

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">{t("app.title")}</h1>
      <p className="mt-1 text-sm text-gray-500">Welcome to the Clean Architecture dashboard.</p>
      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {tenants && tenants.length === 0 && (
          <Card className="border-indigo-200 bg-indigo-50 sm:col-span-2 lg:col-span-3">
            <CardHeader>
              <div className="flex items-center gap-2">
                <Building2 className="h-5 w-5 text-indigo-600" />
                <CardTitle>Get Started</CardTitle>
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-gray-600 mb-4">
                Create your first tenant workspace to start managing todos, users, and roles.
              </p>
              <Button onClick={() => navigate("/tenant/tenants")}>
                <Building2 className="h-4 w-4 mr-1" />
                Go to Tenant Setup
              </Button>
            </CardContent>
          </Card>
        )}
        <Card>
          <CardHeader>
            <CardTitle>Todos</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-500">Manage your tasks and todo items.</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
