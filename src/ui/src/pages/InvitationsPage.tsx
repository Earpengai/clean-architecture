import { useTranslation } from "react-i18next";
import { useInvitations } from "@/api/users";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Mail, Clock } from "lucide-react";

function invitationStatusLabel(status: number) {
  switch (status) {
    case 0: return { label: "Pending", color: "bg-yellow-100 text-yellow-700" };
    case 1: return { label: "Accepted", color: "bg-green-100 text-green-700" };
    case 2: return { label: "Expired", color: "bg-red-100 text-red-700" };
    case 3: return { label: "Canceled", color: "bg-gray-100 text-gray-700" };
    default: return { label: "Unknown", color: "bg-gray-100 text-gray-700" };
  }
}

export function InvitationsPage() {
  const { t } = useTranslation();
  const { data: invitations, isLoading, error } = useInvitations();

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Invitations</h1>
      </div>

      {isLoading && <p className="text-sm text-gray-400">{t("todos.loading")}</p>}
      {error && <ErrorDisplay error={error} className="mb-4" />}

      {invitations && invitations.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No invitations sent yet.</p>
        </div>
      )}

      {invitations && invitations.length > 0 && (
        <div className="space-y-2">
          {invitations.map((inv) => {
            const status = invitationStatusLabel(inv.status);
            return (
              <Card key={inv.id}>
                <CardContent className="p-4">
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-100">
                        <Mail className="h-4 w-4 text-indigo-600" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-900">{inv.email}</p>
                        <p className="text-xs text-gray-500">Role: {inv.roleName}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <div className="flex items-center gap-1 text-xs text-gray-500">
                        <Clock className="h-3 w-3" />
                        Expires {new Date(inv.tokenExpiry).toLocaleDateString()}
                      </div>
                      <Badge className={status.color}>{status.label}</Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
