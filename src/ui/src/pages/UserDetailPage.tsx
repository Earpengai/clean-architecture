import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useUserById } from "@/api/users";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, User, Mail, Shield, Calendar } from "lucide-react";

export function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const { data: user, isLoading, error } = useUserById(id);

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <p className="text-sm text-gray-400">{t("todos.loading")}</p>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="text-center py-12">
        <p className="text-sm text-red-500">{t("todos.error")}</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate("/tenant/users")}>
          <ArrowLeft className="h-4 w-4 mr-1" />
          Back to Users
        </Button>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate("/tenant/users")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-2xl font-bold text-gray-900">User Details</h1>
      </div>

      <div className="max-w-2xl">
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <User className="h-5 w-5 text-indigo-600" />
              <CardTitle>
                {user.firstName} {user.lastName}
              </CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div className="flex items-center gap-2">
                <Mail className="h-4 w-4 text-gray-400" />
                <div>
                  <p className="text-xs text-gray-500">Email</p>
                  <p className="text-gray-700">{user.email}</p>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Shield className="h-4 w-4 text-gray-400" />
                <div>
                  <p className="text-xs text-gray-500">Role</p>
                  <p className="text-gray-700">{user.roleName}</p>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-gray-400" />
                <div>
                  <p className="text-xs text-gray-500">Joined</p>
                  <p className="text-gray-700">{new Date(user.createdAt).toLocaleDateString()}</p>
                </div>
              </div>
              <div>
                <p className="text-xs text-gray-500">Status</p>
                <Badge className={user.emailVerified ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"}>
                  {user.emailVerified ? "Verified" : "Unverified"}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
