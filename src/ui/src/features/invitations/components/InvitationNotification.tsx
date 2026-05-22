import { useState } from "react";
import { useMyInvitations, useAcceptInvitationAuthenticated } from "@/api/users";
import { useAuth } from "@/hooks/useAuth";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
} from "@/components/ui/dropdown-menu";
import { Bell, Mail, CheckCircle, AlertCircle } from "lucide-react";

export function InvitationNotification() {
  const { isAuthenticated } = useAuth();
  const { data: invitations, isLoading } = useMyInvitations();
  const acceptInvitation = useAcceptInvitationAuthenticated();

  const [acceptingId, setAcceptingId] = useState<string | null>(null);
  const [acceptedId, setAcceptedId] = useState<string | null>(null);
  const [errorId, setErrorId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  if (!isAuthenticated) {
    return null;
  }

  const pendingCount = invitations?.length ?? 0;

  const handleAccept = (invitationId: string, token: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setAcceptingId(invitationId);
    setErrorId(null);
    setErrorMessage(null);

    acceptInvitation.mutate(token, {
      onSuccess: () => {
        setAcceptingId(null);
        setAcceptedId(invitationId);
        setErrorId(null);
        setTimeout(() => setAcceptedId(null), 2000);
      },
      onError: (err) => {
        setAcceptingId(null);
        setAcceptedId(null);
        setErrorId(invitationId);
        setErrorMessage(err.message);
      },
    });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-5 w-5" />
          {pendingCount > 0 && (
            <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
              {pendingCount > 9 ? "9+" : pendingCount}
            </span>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-80">
        {isLoading && (
          <div className="px-3 py-4 text-center text-sm text-gray-400">Loading invitations...</div>
        )}

        {!isLoading && pendingCount === 0 && (
          <div className="px-3 py-4 text-center text-sm text-gray-400">No pending invitations</div>
        )}

        {!isLoading &&
          invitations?.map((inv) => {
            const isAccepting = acceptingId === inv.id;
            const isAccepted = acceptedId === inv.id;
            const hasError = errorId === inv.id;

            return (
              <DropdownMenuItem
                key={inv.id}
                className="flex-col items-start gap-1 p-3"
                onSelect={(e) => e.preventDefault()}
              >
                <div className="flex w-full items-start gap-3">
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-100">
                    <Mail className="h-4 w-4 text-indigo-600" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      Invited to {inv.tenantName}
                    </p>
                    <p className="text-xs text-gray-500">Role: {inv.roleName}</p>
                    <p className="text-xs text-gray-400">
                      Expires {new Date(inv.tokenExpiry).toLocaleDateString()}
                    </p>
                  </div>
                </div>

                {isAccepted && (
                  <div className="mt-2 flex w-full items-center gap-1.5 text-xs text-green-600">
                    <CheckCircle className="h-3.5 w-3.5" />
                    Accepted
                  </div>
                )}

                {hasError && (
                  <div className="mt-2 flex w-full items-center gap-1.5 text-xs text-red-500">
                    <AlertCircle className="h-3.5 w-3.5" />
                    {errorMessage}
                  </div>
                )}

                {!isAccepted && (
                  <Button
                    variant="outline"
                    size="sm"
                    className="mt-2 w-full"
                    disabled={isAccepting}
                    onClick={(e) => handleAccept(inv.id, inv.token, e)}
                  >
                    {isAccepting ? "Accepting..." : "Accept"}
                  </Button>
                )}
              </DropdownMenuItem>
            );
          })}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
