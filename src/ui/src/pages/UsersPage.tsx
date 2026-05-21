import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useUsers } from "@/api/users";
import { InviteUserDialog } from "@/features/users/components/InviteUserDialog";
import { AssignRoleDialog } from "@/features/users/components/AssignRoleDialog";
import { RemoveUserDialog } from "@/features/users/components/RemoveUserDialog";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Users } from "lucide-react";

export function UsersPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: users, isLoading, error } = useUsers();

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Users</h1>
        <InviteUserDialog />
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <p className="text-sm text-red-500">{t("todos.error")}</p>}

      {users && users.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No users in this tenant yet.</p>
        </div>
      )}

      {users && users.length > 0 && (
        <div className="space-y-2">
          {users.map((user) => (
            <Card
              key={user.id}
              className="cursor-pointer hover:border-indigo-300 transition-colors"
              onClick={() => navigate(`/users/${user.id}`)}
            >
              <CardContent className="p-4">
                <div className="flex items-center justify-between gap-4">
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-indigo-100">
                      <Users className="h-4 w-4 text-indigo-600" />
                    </div>
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {user.firstName} {user.lastName}
                      </p>
                      <p className="text-xs text-gray-500">{user.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge className={user.emailConfirmed ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"}>
                      {user.emailConfirmed ? "Verified" : "Unverified"}
                    </Badge>
                    <Badge className="bg-indigo-50 text-indigo-700">{user.roleName}</Badge>
                    <div className="flex items-center gap-1 ml-2" onClick={(e) => e.stopPropagation()}>
                      <AssignRoleDialog user={user} />
                      <RemoveUserDialog user={user} />
                    </div>
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
