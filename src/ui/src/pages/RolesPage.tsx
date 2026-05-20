import { useTranslation } from "react-i18next";
import { useRoles } from "@/api/roles";
import { RoleFormDialog } from "@/features/roles/components/RoleFormDialog";
import { DeleteRoleDialog } from "@/features/roles/components/DeleteRoleDialog";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Shield, Pencil } from "lucide-react";

export function RolesPage() {
  const { t } = useTranslation();
  const { data: roles, isLoading, error } = useRoles();

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Roles</h1>
        <RoleFormDialog />
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <p className="text-sm text-red-500">{t("todos.error")}</p>}

      {roles && roles.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No custom roles yet.</p>
          <div className="mt-4">
            <RoleFormDialog />
          </div>
        </div>
      )}

      {roles && roles.length > 0 && (
        <div className="space-y-3">
          {roles.map((role) => (
            <Card key={role.id}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <Shield className="h-4 w-4 text-indigo-600" />
                      <h3 className="font-semibold text-gray-900">{role.name}</h3>
                      {role.isSystem && (
                        <Badge className="bg-gray-100 text-gray-700">System</Badge>
                      )}
                    </div>
                    {role.description && (
                      <p className="mt-1 text-sm text-gray-500">{role.description}</p>
                    )}
                    <div className="mt-2 flex flex-wrap gap-1">
                      {role.permissions.map((perm) => (
                        <Badge key={perm} className="bg-indigo-50 text-indigo-700">
                          {perm}
                        </Badge>
                      ))}
                    </div>
                  </div>
                  <div className="flex items-center gap-1">
                    <RoleFormDialog
                      role={role}
                      trigger={
                        <button className="rounded p-1.5 text-gray-400 hover:bg-gray-100 hover:text-indigo-600">
                          <Pencil className="h-4 w-4" />
                        </button>
                      }
                    />
                    <DeleteRoleDialog role={role} />
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
